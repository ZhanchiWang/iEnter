using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TypelessAutoEnter {
internal static class Native {
    internal const ulong TypelessTag = 0x54594C45, OwnTag = 0x54414557;
    internal const int WH_KEYBOARD_LL = 13, WH_MOUSE_LL = 14;
    internal delegate IntPtr HookProc(int code, IntPtr wp, IntPtr lp);
    internal delegate void WinEventProc(IntPtr hook, uint ev, IntPtr hwnd, int obj, int child, uint thread, uint time);
    [StructLayout(LayoutKind.Sequential)] internal struct Keyboard { public uint vk, scan, flags, time; public UIntPtr extra; }
    [StructLayout(LayoutKind.Sequential)] internal struct Point { public int x, y; }
    [StructLayout(LayoutKind.Sequential)] internal struct Rect { public int left, top, right, bottom; }
    [StructLayout(LayoutKind.Sequential)] internal struct GuiInfo {
        public int size; public uint flags; public IntPtr active, focus, capture, menu, move, caret; public Rect rect;
    }
    [StructLayout(LayoutKind.Sequential)] internal struct KeyInput { public ushort vk, scan; public uint flags, time; public UIntPtr extra; }
    [StructLayout(LayoutKind.Explicit, Size=32)] internal struct InputUnion { [FieldOffset(0)] public KeyInput key; }
    [StructLayout(LayoutKind.Sequential)] internal struct Input { public uint type; public InputUnion u; }
    [DllImport("user32.dll", SetLastError=true)] internal static extern IntPtr SetWindowsHookEx(int id, HookProc callback, IntPtr module, uint thread);
    [DllImport("user32.dll")] internal static extern bool UnhookWindowsHookEx(IntPtr hook);
    [DllImport("user32.dll")] internal static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wp, IntPtr lp);
    [DllImport("kernel32.dll", CharSet=CharSet.Unicode)] internal static extern IntPtr GetModuleHandle(string name);
    [DllImport("user32.dll")] internal static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] internal static extern bool GetGUIThreadInfo(uint thread, ref GuiInfo info);
    [DllImport("user32.dll")] internal static extern short GetAsyncKeyState(int key);
    [DllImport("user32.dll", SetLastError=true)] internal static extern uint SendInput(uint n, Input[] inputs, int size);
    [DllImport("user32.dll")] internal static extern IntPtr SetWinEventHook(uint min, uint max, IntPtr module, WinEventProc callback, uint process, uint thread, uint flags);
    [DllImport("user32.dll")] internal static extern bool UnhookWinEvent(IntPtr hook);
    [DllImport("user32.dll")] internal static extern bool OpenClipboard(IntPtr hwnd);
    [DllImport("user32.dll")] internal static extern bool CloseClipboard();
    [DllImport("user32.dll")] internal static extern IntPtr GetClipboardData(uint format);
    [DllImport("user32.dll")] internal static extern IntPtr SetClipboardData(uint format, IntPtr memory);
    [DllImport("user32.dll")] internal static extern bool EmptyClipboard();
    [DllImport("user32.dll")] internal static extern uint GetClipboardSequenceNumber();
    [DllImport("kernel32.dll")] internal static extern IntPtr GlobalAlloc(uint flags, UIntPtr size);
    [DllImport("kernel32.dll")] internal static extern IntPtr GlobalFree(IntPtr memory);
    [DllImport("kernel32.dll")] internal static extern IntPtr GlobalLock(IntPtr memory);
    [DllImport("kernel32.dll")] internal static extern bool GlobalUnlock(IntPtr memory);
    [DllImport("kernel32.dll")] internal static extern UIntPtr GlobalSize(IntPtr memory);
    [DllImport("user32.dll")] internal static extern bool RegisterHotKey(IntPtr hwnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] internal static extern bool UnregisterHotKey(IntPtr hwnd, int id);
    [DllImport("user32.dll")] internal static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)] internal static extern IntPtr LoadLibrary(string path);
    [DllImport("kernel32.dll", CharSet=CharSet.Ansi)] internal static extern IntPtr GetProcAddress(IntPtr module, string name);
    internal static bool Down(int key) { return (GetAsyncKeyState(key) & 0x8000) != 0; }
    internal static bool ModifiersDown() { return Down(0x10) || Down(0x11) || Down(0x12) || Down(0x5B) || Down(0x5C); }
    internal static IntPtr FocusOf(IntPtr hwnd) {
        uint pid; var thread = GetWindowThreadProcessId(hwnd, out pid);
        var info = new GuiInfo { size=Marshal.SizeOf(typeof(GuiInfo)) };
        return thread != 0 && GetGUIThreadInfo(thread, ref info) ? info.focus : IntPtr.Zero;
    }
    internal static Input Key(ushort vk, bool up, ulong tag) {
        return new Input { type=1, u=new InputUnion { key=new KeyInput { vk=vk, flags=up ? 2u : 0u, extra=new UIntPtr(tag) } } };
    }
    internal static bool SendKey(ushort vk) {
        var a = new[] { Key(vk, false, OwnTag), Key(vk, true, OwnTag) };
        return SendInput(2, a, Marshal.SizeOf(typeof(Input))) == 2;
    }
    // Tests only: exactly the four-event paste used by InputHelper.dll 2.11.0.138.
    internal static bool Paste(ulong tag) {
        var a = new[] { Key(0x11,false,tag), Key(0x56,false,tag), Key(0x56,true,tag), Key(0x11,true,tag) };
        return SendInput(4,a,Marshal.SizeOf(typeof(Input))) == 4;
    }
    // One non-blocking clipboard attempt in the hook; never retry/sleep here.
    internal static bool ReadPasteText(IntPtr owner, string endWord, out string text, out bool matched) {
        text=null; matched=false;
        if (!OpenClipboard(owner)) return false;
        try {
            IntPtr h=GetClipboardData(13);
            ulong size=h==IntPtr.Zero ? 0 : GlobalSize(h).ToUInt64();
            if (size < 2 || size > 2*1024*1024) return false;
            IntPtr p=GlobalLock(h); if (p==IntPtr.Zero) return false;
            try { text=Marshal.PtrToStringUni(p,(int)size/2); int end=text.IndexOf('\0'); if(end>=0) text=text.Substring(0,end); }
            finally { GlobalUnlock(h); }
            if (String.IsNullOrWhiteSpace(text)) return false;
            if (endWord==null) return true;
            string clean;
            if (!Rules.TryStripEndWord(text,endWord,out clean)) return true;
            byte[] bytes=Encoding.Unicode.GetBytes(clean+"\0");
            byte[] original=Encoding.Unicode.GetBytes(text+"\0");
            IntPtr fallback=GlobalAlloc(0x42,new UIntPtr((uint)original.Length));
            if(fallback==IntPtr.Zero) return false;
            IntPtr fp=GlobalLock(fallback);
            if(fp==IntPtr.Zero) { GlobalFree(fallback);return false; }
            Marshal.Copy(original,0,fp,original.Length);GlobalUnlock(fallback);
            IntPtr mem=GlobalAlloc(0x42,new UIntPtr((uint)bytes.Length));
            if(mem==IntPtr.Zero) { GlobalFree(fallback);return false; }
            IntPtr ptr=GlobalLock(mem);
            if(ptr==IntPtr.Zero) { GlobalFree(mem);GlobalFree(fallback); return false; }
            Marshal.Copy(bytes,0,ptr,bytes.Length); GlobalUnlock(mem);
            if(!EmptyClipboard()) { GlobalFree(mem);GlobalFree(fallback); return false; }
            if(SetClipboardData(13,mem)==IntPtr.Zero) {
                GlobalFree(mem);
                if(SetClipboardData(13,fallback)==IntPtr.Zero) GlobalFree(fallback);
                return false;
            }
            GlobalFree(fallback);
            matched=true; text=clean; return true;
        } finally { CloseClipboard(); }
    }
}
}
