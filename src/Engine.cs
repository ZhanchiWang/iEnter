using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TypelessAutoEnter {
internal sealed class Engine : NativeWindow, IDisposable {
    internal readonly Settings Settings;
    internal Action<string> Status;
    internal Action ToggleRequested;
    internal IntPtr TestWindow;
    internal IntPtr TestFocus;
    internal int Detected, Sent;
    internal bool HotkeyRegistered;
    private readonly Stopwatch watch=Stopwatch.StartNew();
    private readonly Timer timer=new Timer();
    private readonly Queue<string> messages=new Queue<string>();
    private readonly Native.HookProc keyboard,mouse;
    private readonly Native.WinEventProc focusChanged;
    private IntPtr kh,mh,fh,wh;
    private Pending pending;
    private int generation;
    private bool disposed;
    internal Engine(Settings settings) {
        Settings=settings;
        CreateHandle(new CreateParams { Caption="TypelessAutoEnter.Listener", Parent=new IntPtr(-3) });
        keyboard=KeyboardHook; mouse=MouseHook; focusChanged=FocusChanged;
        kh=Native.SetWindowsHookEx(Native.WH_KEYBOARD_LL,keyboard,Native.GetModuleHandle(null),0);
        mh=Native.SetWindowsHookEx(Native.WH_MOUSE_LL,mouse,Native.GetModuleHandle(null),0);
        fh=Native.SetWinEventHook(0x8005,0x8005,IntPtr.Zero,focusChanged,0,0,0);
        wh=Native.SetWinEventHook(3,3,IntPtr.Zero,focusChanged,0,0,0);
        if(kh==IntPtr.Zero || mh==IntPtr.Zero || fh==IntPtr.Zero || wh==IntPtr.Zero) {
            int error=Marshal.GetLastWin32Error(); Dispose(); throw new Win32Exception(error,UiText.T("Could not start input monitoring","无法启动输入监听"));
        }
        HotkeyRegistered=Native.RegisterHotKey(Handle,1,0x4000|2|4,0x0D);
        timer.Interval=25; timer.Tick+=Tick; timer.Start();
    }
    internal void Cancel(string reason) {
        generation++;
        if(pending!=null) { pending=null; messages.Enqueue(reason); }
    }
    internal void SetEnabled(bool enabled) {
        Settings.Enabled=enabled; Cancel(UiText.T("Switch changed; pending Enter canceled.","开关已改变，取消本次回车"));
        messages.Enqueue(enabled ? UiText.T("Auto Enter is on.","已开启") : UiText.T("Auto Enter is paused.","已暂停"));
    }
    private IntPtr KeyboardHook(int code,IntPtr wp,IntPtr lp) {
        try {
            if(code>=0 && (wp.ToInt32()==0x100 || wp.ToInt32()==0x104)) {
                var k=(Native.Keyboard)Marshal.PtrToStructure(lp,typeof(Native.Keyboard));
                if(k.extra.ToUInt64()!=Native.OwnTag) {
                    if(Rules.IsTypelessPaste(k.vk,k.flags,k.extra.ToUInt64(),Native.Down(0x11))) OnPaste();
                    else if((k.flags&0x10)==0 || k.extra.ToUInt64()!=Native.TypelessTag) Cancel(UiText.T("Typing canceled the pending Enter.","检测到其他按键，取消本次回车"));
                }
            }
        } catch { Cancel(UiText.T("Could not verify input; Enter canceled.","检测失败，已取消本次回车")); }
        return Native.CallNextHookEx(kh,code,wp,lp);
    }
    private IntPtr MouseHook(int code,IntPtr wp,IntPtr lp) {
        if(code>=0) {
            int m=wp.ToInt32();
            if(m==0x201 || m==0x204 || m==0x207 || m==0x20B || m==0x20A || m==0x20E) Cancel(UiText.T("Mouse input canceled the pending Enter.","鼠标操作已取消本次回车"));
        }
        return Native.CallNextHookEx(mh,code,wp,lp);
    }
    private void FocusChanged(IntPtr hook,uint ev,IntPtr hwnd,int obj,int child,uint thread,uint time) {
        if(pending!=null) Cancel(UiText.T("Focus changed; pending Enter canceled.","焦点变化，取消本次回车"));
    }
    private void OnPaste() {
        Detected++;
        if(!Settings.Enabled) { messages.Enqueue(UiText.T("Typeless paste detected (paused).","检测到 Typeless 粘贴（当前已暂停）")); return; }
        var hwnd=Native.GetForegroundWindow();
        uint pid; Native.GetWindowThreadProcessId(hwnd,out pid);
        string name;
        try { using(var p=Process.GetProcessById((int)pid)) name=p.ProcessName; } catch { Cancel(UiText.T("Could not identify the current app.","无法确认当前应用")); return; }
        var focus=Native.FocusOf(hwnd);
        if(hwnd==IntPtr.Zero || focus==IntPtr.Zero) { Cancel(UiText.T("Could not verify the input focus.","没有可确认的输入焦点")); return; }
        bool localTest=hwnd==TestWindow && focus==TestFocus;
        if(!localTest && (hwnd==TestWindow || !Rules.Allows(Settings,name))) { Cancel(UiText.T("Auto Enter is not enabled for this app.","此应用未启用自动回车")); messages.Enqueue(UiText.T("Skipped: ","已忽略：")+name); return; }
        if(Settings.Mode=="over" && !Rules.IsValidEndWord(Settings.EndWord)) { Cancel(UiText.T("No ending word set; text kept.","结束词未设置，保留输入")); messages.Enqueue(UiText.T("Set an ending word to enable sending.","请设置结束词：当前只输入文字")); return; }
        string text; bool matched;
        if(!Native.ReadPasteText(Handle,Settings.Mode=="over" ? Settings.EndWord : null,out text,out matched)) { Cancel(UiText.T("Could not read this paste; Enter canceled.","无法读取本次粘贴，取消回车")); messages.Enqueue(UiText.T("Clipboard unavailable; paste skipped.","剪贴板暂不可读，已跳过")); return; }
        if(Settings.Mode=="over" && !matched) { Cancel(UiText.T("Ending word absent; text kept.","结尾没有结束词，保留输入")); messages.Enqueue(UiText.T("No ending word: text inserted without Enter.","结尾没有结束词：只输入文字")); return; }
        var now=watch.ElapsedMilliseconds;
        pending=new Pending { Window=hwnd,Focus=focus,Generation=generation,Due=now+Settings.DelayMs,Expires=now+Settings.DelayMs+1500 };
        messages.Enqueue(UiText.T("Typeless \u2192 ","检测到 Typeless → ")+name+UiText.T(" \u00b7 waiting ","，等待 ")+Settings.DelayMs+" ms");
    }
    private void Tick(object sender,EventArgs e) {
        if(pending!=null) {
            var p=pending; long now=watch.ElapsedMilliseconds;
            var hwnd=Native.GetForegroundWindow(); var focus=Native.FocusOf(hwnd);
            if(hwnd!=p.Window || focus!=p.Focus || now>p.Expires || p.Generation!=generation || !Settings.Enabled) Cancel(UiText.T("Target changed or timed out; Enter canceled.","输入目标已变化或超时，取消本次回车"));
            else if(p.CanFire(now,hwnd,focus,generation,Settings.Enabled,Native.ModifiersDown())) {
                pending=null;
                // The focus check and injection are adjacent; there is no way to make
                // another process's focus + SendInput atomic. Scope apps accordingly.
                if(Native.SendKey(0x0D)) { Sent++; messages.Enqueue(UiText.T("Enter sent.","已自动按下 Enter")); }
                else messages.Enqueue(UiText.T("Windows did not accept Enter. Check app permissions.","Windows 未接受回车事件（请检查目标程序权限）"));
            }
        }
        while(messages.Count>0) { var s=messages.Dequeue(); if(Status!=null) Status(s); }
    }
    protected override void WndProc(ref Message m) {
        if(m.Msg==0x312 && m.WParam.ToInt32()==1 && ToggleRequested!=null) ToggleRequested();
        base.WndProc(ref m);
    }
    public void Dispose() {
        if(disposed) return; disposed=true;
        timer.Stop(); timer.Dispose();
        Native.UnregisterHotKey(Handle,1);
        if(kh!=IntPtr.Zero) Native.UnhookWindowsHookEx(kh);
        if(mh!=IntPtr.Zero) Native.UnhookWindowsHookEx(mh);
        if(fh!=IntPtr.Zero) Native.UnhookWinEvent(fh);
        if(wh!=IntPtr.Zero) Native.UnhookWinEvent(wh);
        DestroyHandle();
    }
}
}
