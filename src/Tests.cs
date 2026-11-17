using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TypelessAutoEnter {
internal static class Tests {
    internal static int Run(string directory) {
        Directory.CreateDirectory(directory); var lines=new List<string>(); int fails=0;
        Action<string,bool> check=(name,ok)=>{lines.Add((ok?"PASS ":"FAIL ")+name);if(!ok) fails++;};
        check("native INPUT layout is x64 40 bytes",Marshal.SizeOf(typeof(Native.Input))==40);
        check("native KBDLLHOOKSTRUCT layout is x64 24 bytes",Marshal.SizeOf(typeof(Native.Keyboard))==24);
        check("recognize Typeless tagged injected Ctrl+V",Rules.IsTypelessPaste(0x56,0x10,Native.TypelessTag,true));
        check("ordinary physical Ctrl+V never triggers",!Rules.IsTypelessPaste(0x56,0,0,true));
        check("other automation Ctrl+V never triggers",!Rules.IsTypelessPaste(0x56,0x10,0,true));
        check("tag alone cannot trigger without injection flag",!Rules.IsTypelessPaste(0x56,0,Native.TypelessTag,true));
        check("Typeless Ctrl+C cannot trigger",!Rules.IsTypelessPaste(0x43,0x10,Native.TypelessTag,true));
        check("plain V cannot trigger",!Rules.IsTypelessPaste(0x56,0x10,Native.TypelessTag,false));
        string clean;
        check("Chinese + comma + OVER punctuation",Rules.TryStripEndWord("五分钟后到， OVER。 ","over",out clean)&&clean=="五分钟后到");
        check("Chinese adjoining over",Rules.TryStripEndWord("我到了over","over",out clean)&&clean=="我到了");
        check("preserve body punctuation",Rules.TryStripEndWord("我到了。 over!","over",out clean)&&clean=="我到了。");
        check("do not send empty command",!Rules.TryStripEndWord("over","over",out clean));
        check("do not match turnover",!Rules.TryStripEndWord("turnover","over",out clean));
        check("do not match leftover",!Rules.TryStripEndWord("leftover","over",out clean));
        check("do not match middle word",!Rules.TryStripEndWord("over 明天再说","over",out clean));
        check("custom Chinese ending with punctuation",Rules.TryStripEndWord("明天见，发送。","发送",out clean)&&clean=="明天见");
        check("Chinese end word adjoining body",Rules.TryStripEndWord("我到了发送","发送",out clean)&&clean=="我到了");
        check("custom English is case insensitive",Rules.TryStripEndWord("明天见，DONE!","done",out clean)&&clean=="明天见");
        check("old end word no longer triggers",!Rules.TryStripEndWord("明天见，over","done",out clean));
        check("English phrase supported",Rules.TryStripEndWord("明天见，send now.","send now",out clean)&&clean=="明天见");
        check("English suffix inside another word rejected",!Rules.TryStripEndWord("please resend","send",out clean));
        check("custom end word in middle rejected",!Rules.TryStripEndWord("请发送文件","发送",out clean));
        check("empty custom word never sends",!Rules.TryStripEndWord("明天见","",out clean));
        check("whitespace custom word never sends",!Rules.TryStripEndWord("明天见","  ",out clean));
        check("only custom word never sends",!Rules.TryStripEndWord("发送。","发送",out clean));
        check("word surrounding spaces ignored",Rules.TryStripEndWord("明天见，发送"," 发送 ",out clean)&&clean=="明天见");
        check("regex characters treated literally",Rules.TryStripEndWord("明天见，a+b.","a+b",out clean)&&clean=="明天见");
        check("regex wildcard cannot match arbitrary text",!Rules.TryStripEndWord("明天见，anything",".*",out clean));
        check("term with final punctuation matched literally",Rules.TryStripEndWord("明天见，go!。","go!",out clean)&&clean=="明天见");
        check("end word length bounded",!Rules.IsValidEndWord(new string('a',65)));
        check("end word cannot contain control characters",!Rules.IsValidEndWord("send\nnow"));
        var serializer=new DataContractJsonSerializer(typeof(Settings));
        using(var stream=new MemoryStream(Encoding.UTF8.GetBytes("{\"AllApps\":true,\"Enabled\":true,\"Mode\":\"over\",\"DelayMs\":500}"))) {
            var migrated=(Settings)serializer.ReadObject(stream);migrated.Normalize();
            check("old config migrates to over and keeps user switches",migrated.EndWord=="over"&&migrated.Mode=="over"&&migrated.Enabled&&migrated.AllApps);
            check("old config gets English UI by default",migrated.Language=="en");
        }
        using(var stream=new MemoryStream()) {
            serializer.WriteObject(stream,new Settings {Mode="over",EndWord="发送",Language="zh"});stream.Position=0;
            var saved=(Settings)serializer.ReadObject(stream);saved.Normalize();
            check("custom Chinese setting survives serialization",saved.EndWord=="发送"&&saved.Mode=="over");
            check("language choice survives serialization",saved.Language=="zh");
        }
        var empty=new Settings {Mode="over",EndWord=""};empty.Normalize();
        check("empty setting stays empty after normalization",empty.EndWord=="");
        var unknownLanguage=new Settings {Language="invalid"};unknownLanguage.Normalize();
        check("unknown language safely falls back to English",unknownLanguage.Language=="en");
        UiText.Language="zh";check("Chinese strings selected",UiText.T("On","开启")=="开启");
        UiText.Language="en";check("English strings selected",UiText.T("On","开启")=="On");
        var s=new Settings {AllowedApps="Codex.exe, WeChat; msedge"};
        check("app list normalizes exe and case",Rules.Allows(s,"codex")&&Rules.Allows(s,"WeChat"));
        check("app list excludes unrelated process",!Rules.Allows(s,"pwsh"));
        var p=new Pending { Window=new IntPtr(1),Focus=new IntPtr(2),Generation=5,Due=500,Expires=2000 };
        check("not before due",!p.CanFire(499,p.Window,p.Focus,5,true,false));
        check("can fire when settled",p.CanFire(500,p.Window,p.Focus,5,true,false));
        check("foreground switch blocks",!p.CanFire(600,new IntPtr(3),p.Focus,5,true,false));
        check("same window different field blocks",!p.CanFire(600,p.Window,new IntPtr(4),5,true,false));
        check("user input generation blocks",!p.CanFire(600,p.Window,p.Focus,6,true,false));
        check("pause blocks",!p.CanFire(600,p.Window,p.Focus,5,false,false));
        check("held modifier blocks",!p.CanFire(600,p.Window,p.Focus,5,true,true));
        check("expired event never fires",!p.CanFire(2001,p.Window,p.Focus,5,true,false));
        lines.Add("Failures: "+fails); File.WriteAllLines(Path.Combine(directory,"self-test.txt"),lines,Encoding.UTF8); return fails==0?0:1;
    }
}
internal static class Integration {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int InsertText(IntPtr utf8);
    internal static int Run(string directory,string dll) {
        Directory.CreateDirectory(directory); var lines=new List<string>(); int fails=0;
        var originalClipboard=new DataObject();
        try { var source=Clipboard.GetDataObject(); if(source!=null) foreach(var format in source.GetFormats(false)) { try { originalClipboard.SetData(format,false,source.GetData(format,false)); } catch {} } } catch {}
        var form=new Form {Text="iEnter — isolated integration test",Width=600,Height=300,StartPosition=FormStartPosition.CenterScreen};
        var edit=new TextBox {Multiline=true,Dock=DockStyle.Fill,AcceptsReturn=true};
        var other=new TextBox {Dock=DockStyle.Bottom}; form.Controls.Add(edit);form.Controls.Add(other);
        int enters=0; edit.KeyDown+=(o,e)=>{if(e.KeyCode==Keys.Enter){enters++;e.SuppressKeyPress=true;}};
        other.KeyDown+=(o,e)=>{if(e.KeyCode==Keys.Enter){enters++;e.SuppressKeyPress=true;}};
        var settings=new Settings {Enabled=true,AllApps=false,AllowedApps="__no_external_apps__",DelayMs=500};
        using(var engine=new Engine(settings)) {
            engine.TestWindow=form.Handle; engine.TestFocus=edit.Handle; engine.Status=s=>lines.Add("EVENT "+s);
            InsertText insert=null;
            if(!String.IsNullOrEmpty(dll)) {
                var lib=Native.LoadLibrary(dll); var ptr=lib==IntPtr.Zero?IntPtr.Zero:Native.GetProcAddress(lib,"insertText");
                if(ptr!=IntPtr.Zero) insert=(InsertText)Marshal.GetDelegateForFunctionPointer(ptr,typeof(InsertText));
                else {lines.Add("FAIL Cannot load official InputHelper.dll");fails++;}
            }
            Func<string,Task> paste=async text=>{
                if(Native.GetForegroundWindow()!=form.Handle || !edit.Focused) throw new InvalidOperationException("Test lost focus; stopping to protect other apps.");
                if(insert==null) {Clipboard.SetText(text);Native.Paste(Native.TypelessTag);}
                else {
                    await Task.Run(()=>{
                        byte[] bytes=Encoding.UTF8.GetBytes(text+"\0"); var p=Marshal.AllocHGlobal(bytes.Length);
                        try {Marshal.Copy(bytes,0,p,bytes.Length);if(insert(p)!=1) throw new InvalidOperationException("Official insertText returned failure");}
                        finally {Marshal.FreeHGlobal(p);}
                    });
                }
            };
            Action<string,bool> check=(name,ok)=>{lines.Add((ok?"PASS ":"FAIL ")+name);if(!ok) fails++;};
            form.Shown+=async (o,e)=>{
                try {
                    await Task.Delay(100); form.Hide();form.Show();edit.Focus();
                    for(int i=0;i<600 && Native.GetForegroundWindow()!=form.Handle;i++) await Task.Delay(100);
                    if(Native.GetForegroundWindow()!=form.Handle || !edit.Focused) throw new InvalidOperationException("Test window not active; no keys will be sent.");
                    await Task.Delay(350);
                    if(Native.GetForegroundWindow()!=form.Handle || !edit.Focused) throw new InvalidOperationException("Test window lost focus before first paste.");
                    check("test window foreground",true);
                    Clipboard.SetText("ordinary paste"); Native.Paste(0); await Task.Delay(800);
                    check("ordinary paste enters text without auto-Enter",edit.Text=="ordinary paste"&&enters==0);
                    edit.Clear(); await paste("官方输入测试"); await Task.Delay(800);
                    check("official paste triggers exactly one Enter",edit.Text=="官方输入测试"&&enters==1);
                    edit.Clear(); settings.Mode="over"; await paste("五分钟后到，over。"); await Task.Delay(800);
                    check("over removed before paste and one Enter",edit.Text=="五分钟后到"&&enters==2);
                    edit.Clear(); await paste("turnover"); await Task.Delay(800);
                    check("ordinary word ending over stays unchanged, no Enter",edit.Text=="turnover"&&enters==2);
                    edit.Clear(); settings.Mode="always";engine.SetEnabled(false); await paste("暂停测试"); await Task.Delay(800);
                    check("paused listener never sends",edit.Text=="暂停测试"&&enters==2);
                    engine.SetEnabled(true); edit.Clear(); await paste("切换输入框测试");other.Focus();await Task.Delay(800);
                    check("same-window focus switch cancels Enter",enters==2);
                    edit.Focus();await Task.Delay(200);edit.Clear();
                    engine.TestWindow=IntPtr.Zero;await paste("未授权应用测试");await Task.Delay(800);
                    check("app outside allowlist never sends",enters==2);engine.TestWindow=form.Handle;
                    // The desktop client serializes insertText calls with a 500 ms
                    // guard. Allow its native clipboard restore to finish as well.
                    edit.Clear();settings.DelayMs=1500;await paste("第一段");await Task.Delay(700);await paste("第二段");await Task.Delay(2000);
                    if(edit.Text!="第一段第二段"||enters!=3)lines.Add("DETAIL burst text="+edit.Text+" enters="+enters);
                    check("consecutive paste debounce emits only one Enter",edit.Text=="第一段第二段"&&enters==3);
                    check("tagged events were observed",engine.Detected>=8);
                    settings.DelayMs=500;settings.Mode="over";settings.EndWord="发送";edit.Clear();
                    await paste("明天见，发送。");await Task.Delay(800);
                    if(edit.Text!="明天见"||enters!=4)lines.Add("DETAIL Chinese text="+edit.Text+" enters="+enters);
                    check("custom Chinese word removed and sends exactly once",edit.Text=="明天见"&&enters==4);
                    settings.EndWord="done";edit.Clear();await paste("第一条完成，发送");await Task.Delay(800);
                    check("changing word disables previous word immediately",edit.Text=="第一条完成，发送"&&enters==4);
                    edit.Clear();await paste("第二条完成，DoNe!");await Task.Delay(800);
                    check("new English word case-insensitive paste and Enter",edit.Text=="第二条完成"&&enters==5);
                    settings.EndWord="";edit.Clear();await paste("留空测试，done");await Task.Delay(800);
                    check("empty word leaves text and never sends",edit.Text=="留空测试，done"&&enters==5);
                    settings.EndWord="send now";edit.Clear();await paste("测试短语，send now.");await Task.Delay(800);
                    check("custom multi-word phrase through official helper",edit.Text=="测试短语"&&enters==6);
                } catch(Exception ex) {lines.Add("FAIL "+ex.GetType().Name+": "+ex.Message);fails++;}
                finally {try{Clipboard.SetDataObject(originalClipboard,true);}catch{} lines.Add("Failures: "+fails);File.WriteAllLines(Path.Combine(directory,"integration-test.txt"),lines,Encoding.UTF8);form.Close();}
            };
            Application.Run(form);
        }
        return fails==0?0:1;
    }
}
}
