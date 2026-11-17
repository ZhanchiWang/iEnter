using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TypelessAutoEnter {
internal sealed class MainForm : Form {
    private readonly Settings settings;
    private readonly bool preview;
    private Engine engine;
    private NotifyIcon tray;
    private readonly Toggle enabled=new Toggle();
    private readonly Segment always=new Segment(), ending=new Segment();
    private readonly TextBox endWord=new TextBox();
    private readonly Label tagline=new Label(), activeTitle=new Label(), activeHint=new Label(), behaviorTitle=new Label(), wordLabel=new Label(), wordHint=new Label(), demoTitle=new Label(), demoHint=new Label(), count=new Label(), status=new Label(), shortcut=new Label();
    private readonly SoftButton language=new SoftButton(), options=new SoftButton();
    private readonly ToolStripMenuItem toggleMenu=new ToolStripMenuItem(), settingsMenu=new ToolStripMenuItem(), exitMenu=new ToolStripMenuItem();
    internal readonly TextBox Demo=new TextBox();
    private bool exiting,refreshing;
    private int enters;
    internal MainForm(Settings s,bool previewMode=false) {
        settings=s;preview=previewMode;UiText.Language=s.Language;Text="iEnter";
        SuspendLayout();Icon=AppIcon.LoadIcon(SystemInformation.IconSize);
        AutoScaleDimensions=new SizeF(96,96);AutoScaleMode=AutoScaleMode.Dpi;
        Font=new Font("Segoe UI",10);ForeColor=Theme.Ink;BackColor=Theme.Background;
        ClientSize=new Size(640,630);MinimumSize=new Size(620,620);StartPosition=FormStartPosition.CenterScreen;
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,Padding=new Padding(24),ColumnCount=1,RowCount=6,BackColor=Theme.Background};
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,72));root.RowStyles.Add(new RowStyle(SizeType.Absolute,80));root.RowStyles.Add(new RowStyle(SizeType.Absolute,192));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,46));root.RowStyles.Add(new RowStyle(SizeType.Absolute,26));Controls.Add(root);
        var header=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,Margin=new Padding(0,0,0,16)};
        header.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,52));header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,74));
        header.Controls.Add(new AppIcon {Size=new Size(40,40),Margin=new Padding(0,5,12,0)},0,0);
        var titles=new Panel {Dock=DockStyle.Fill};titles.Controls.Add(new Label {Text="iEnter",Font=new Font("Segoe UI",21,FontStyle.Bold),AutoSize=true,Location=new Point(0,-2)});
        tagline.AutoSize=true;tagline.ForeColor=Theme.Muted;tagline.Location=new Point(2,34);tagline.Font=new Font(Font.FontFamily,9);titles.Controls.Add(tagline);header.Controls.Add(titles,1,0);
        language.Size=new Size(70,32);language.Margin=new Padding(0,9,0,0);header.Controls.Add(language,2,0);root.Controls.Add(header,0,0);
        var enableCard=new Card {Dock=DockStyle.Fill,Margin=new Padding(0,0,0,12),Padding=new Padding(18,12,18,12)};
        var enableRow=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,BackColor=Color.White};enableRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));enableRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,52));
        enableRow.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var activeText=new Panel {Dock=DockStyle.Fill,BackColor=Color.White};activeTitle.Font=new Font(Font,FontStyle.Bold);activeTitle.AutoSize=true;activeTitle.Location=new Point(0,0);activeHint.Font=new Font(Font.FontFamily,9);activeHint.ForeColor=Theme.Muted;activeHint.AutoSize=true;activeHint.Location=new Point(0,23);activeText.Controls.Add(activeTitle);activeText.Controls.Add(activeHint);
        enableRow.Controls.Add(activeText,0,0);enabled.Anchor=AnchorStyles.Right;enabled.Margin=new Padding(0);enableRow.Controls.Add(enabled,1,0);enableCard.Controls.Add(enableRow);root.Controls.Add(enableCard,0,1);
        var behavior=new Card {Dock=DockStyle.Fill,Margin=new Padding(0,0,0,12),Padding=new Padding(18,12,18,12)};
        var behaviorGrid=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,BackColor=Color.White};behaviorGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        behaviorGrid.RowStyles.Add(new RowStyle(SizeType.Absolute,28));behaviorGrid.RowStyles.Add(new RowStyle(SizeType.Absolute,42));behaviorGrid.RowStyles.Add(new RowStyle(SizeType.Absolute,50));behaviorGrid.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        behaviorTitle.AutoSize=true;behaviorTitle.Font=new Font(Font,FontStyle.Bold);behaviorGrid.Controls.Add(behaviorTitle,0,0);
        var segments=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,BackColor=Color.White,Margin=new Padding(0,0,0,4)};segments.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));segments.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));always.Dock=DockStyle.Fill;ending.Dock=DockStyle.Fill;always.Margin=new Padding(0,0,4,0);ending.Margin=new Padding(4,0,0,0);segments.Controls.Add(always,0,0);segments.Controls.Add(ending,1,0);behaviorGrid.Controls.Add(segments,0,1);
        segments.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var wordRow=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,BackColor=Color.White,Margin=new Padding(0,8,0,4)};wordRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,104));wordRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));wordLabel.Dock=DockStyle.Fill;wordLabel.TextAlign=ContentAlignment.MiddleLeft;wordRow.Controls.Add(wordLabel,0,0);
        wordRow.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var field=new Panel {Dock=DockStyle.Fill,BackColor=Theme.Field,Padding=new Padding(12,6,10,5)};endWord.BorderStyle=BorderStyle.None;endWord.BackColor=Theme.Field;endWord.Dock=DockStyle.Fill;endWord.MaxLength=64;field.Controls.Add(endWord);wordRow.Controls.Add(field,1,0);behaviorGrid.Controls.Add(wordRow,0,2);
        wordHint.ForeColor=Theme.Muted;wordHint.Font=new Font(Font.FontFamily,9);wordHint.Dock=DockStyle.Fill;wordHint.TextAlign=ContentAlignment.MiddleLeft;behaviorGrid.Controls.Add(wordHint,0,3);behavior.Controls.Add(behaviorGrid);root.Controls.Add(behavior,0,2);
        var test=new Card {Dock=DockStyle.Fill,Margin=new Padding(0,0,0,12),Padding=new Padding(18,12,18,12)};
        var testGrid=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,BackColor=Color.White};testGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));testGrid.RowStyles.Add(new RowStyle(SizeType.Absolute,24));testGrid.RowStyles.Add(new RowStyle(SizeType.Absolute,24));testGrid.RowStyles.Add(new RowStyle(SizeType.Percent,100));testGrid.RowStyles.Add(new RowStyle(SizeType.Absolute,23));
        demoTitle.AutoSize=true;demoTitle.Font=new Font(Font,FontStyle.Bold);testGrid.Controls.Add(demoTitle,0,0);demoHint.AutoSize=true;demoHint.Font=new Font(Font.FontFamily,9);demoHint.ForeColor=Theme.Muted;testGrid.Controls.Add(demoHint,0,1);
        var demoField=new Panel {Dock=DockStyle.Fill,BackColor=Theme.Field,Padding=new Padding(10,6,10,5),Margin=new Padding(0,0,0,5)};
        Demo.Multiline=true;Demo.AcceptsReturn=true;Demo.BorderStyle=BorderStyle.None;Demo.BackColor=Theme.Field;Demo.Dock=DockStyle.Fill;Demo.ScrollBars=ScrollBars.Vertical;demoField.Controls.Add(Demo);testGrid.Controls.Add(demoField,0,2);count.AutoSize=true;count.ForeColor=Theme.Muted;count.Font=new Font(Font.FontFamily,9);testGrid.Controls.Add(count,0,3);test.Controls.Add(testGrid);root.Controls.Add(test,0,3);
        var footer=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,Margin=new Padding(0)};footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,110));shortcut.Dock=DockStyle.Fill;shortcut.TextAlign=ContentAlignment.MiddleLeft;shortcut.ForeColor=Theme.Muted;shortcut.Font=new Font(Font.FontFamily,9);footer.Controls.Add(shortcut,0,0);options.Dock=DockStyle.Fill;options.Margin=new Padding(0,2,0,8);footer.Controls.Add(options,1,0);root.Controls.Add(footer,0,4);
        footer.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        status.Dock=DockStyle.Fill;status.ForeColor=Theme.Muted;status.Font=new Font(Font.FontFamily,9);status.AutoEllipsis=true;status.TextAlign=ContentAlignment.MiddleLeft;root.Controls.Add(status,0,5);
        refreshing=true;enabled.Checked=settings.Enabled;always.Checked=settings.Mode=="always";ending.Checked=settings.Mode=="over";endWord.Text=settings.EndWord;refreshing=false;
        ResumeLayout(true);
        if(!preview) {
            engine=new Engine(settings);engine.TestWindow=Handle;engine.TestFocus=Demo.Handle;engine.Status=message=>status.Text=message;engine.ToggleRequested=()=>enabled.Checked=!enabled.Checked;
            tray=new NotifyIcon {Icon=AppIcon.LoadIcon(SystemInformation.SmallIconSize),Visible=true};var menu=new ContextMenuStrip();menu.Items.Add(toggleMenu);menu.Items.Add(settingsMenu);menu.Items.Add(new ToolStripSeparator());menu.Items.Add(exitMenu);tray.ContextMenuStrip=menu;tray.DoubleClick+=(o,e)=>Restore();
            toggleMenu.Click+=(o,e)=>enabled.Checked=!enabled.Checked;settingsMenu.Click+=(o,e)=>Restore();exitMenu.Click+=(o,e)=>{exiting=true;Close();};
        }
        enabled.CheckedChanged+=(o,e)=>{if(refreshing)return;settings.Enabled=enabled.Checked;if(engine!=null)engine.SetEnabled(enabled.Checked);Save();RefreshText();};
        always.CheckedChanged+=(o,e)=>{if(always.Checked)SetMode("always");};ending.CheckedChanged+=(o,e)=>{if(ending.Checked)SetMode("over");};
        endWord.TextChanged+=(o,e)=>{if(refreshing)return;Cancel();settings.EndWord=endWord.Text.Trim();if(Save())status.Text=Rules.IsValidEndWord(settings.EndWord)?UiText.T("Ending word saved: ","结束词已保存：")+settings.EndWord:UiText.T("Add an ending word to enable keyword sending.","填写结束词后才能触发发送。");};
        language.Click+=(o,e)=>{settings.Language=settings.Language=="en"?"zh":"en";UiText.Language=settings.Language;Save();RefreshText();};
        options.Click+=(o,e)=>{Cancel();using(var dialog=new OptionsForm(settings,Save))dialog.ShowDialog(this);RefreshText();};
        Demo.KeyDown+=(o,e)=>{if(e.KeyCode==Keys.Enter&&e.Modifiers==Keys.None){enters++;UpdateCount();e.SuppressKeyPress=true;}};
        FormClosing+=(o,e)=>{if(!preview&&!exiting&&e.CloseReason==CloseReason.UserClosing){e.Cancel=true;Hide();}};
        FormClosed+=(o,e)=>{if(tray!=null){tray.Visible=false;tray.Icon.Dispose();tray.Dispose();}if(engine!=null)engine.Dispose();Icon.Dispose();};
        RefreshText();
    }
    private void SetMode(string mode) {if(refreshing)return;Cancel();settings.Mode=mode;Save();RefreshText();}
    private void Cancel() {if(engine!=null)engine.Cancel(UiText.T("Settings changed; pending Enter canceled.","设置已改变，已取消待执行的回车。"));}
    private bool Save() {if(preview)return true;try{settings.Save();return true;}catch{status.Text=UiText.T("Could not save settings. Changes still apply for this session.","设置保存失败，本次运行仍然有效。");return false;}}
    private void UpdateCount(){count.Text=UiText.T("Enter received: ","收到回车：")+enters;}
    private void RefreshText() {
        tagline.Text=UiText.T("Speak. Send. Stay in flow.","说完，即发送。");language.Text=settings.Language=="en"?"中文":"EN";language.AccessibleName=UiText.T("Switch to Chinese","切换到英文");
        enabled.AccessibleName=UiText.T("Enable iEnter","开启自动回车");activeTitle.Text=settings.Enabled?UiText.T("iEnter is on","自动回车已开启"):UiText.T("iEnter is paused","自动回车已暂停");activeHint.Text=UiText.T("A finishing touch for Typeless.","为 Typeless 补上最后一次回车。");
        behaviorTitle.Text=UiText.T("When to send","发送方式");always.Text=UiText.T("Every dictation","每次输入后");ending.Text=UiText.T("With an ending word","仅有结束词时");wordLabel.Text=UiText.T("Ending word","结束词");endWord.AccessibleName=UiText.T("Custom ending word","自定义结束词");endWord.Enabled=settings.Mode=="over";
        wordHint.Text=settings.Mode=="over"?UiText.T("Removed before sending. Changes save automatically.","发送前去掉结束词，修改后自动保存。"):UiText.T("Presses Enter after every Typeless paste.","每次 Typeless 输入完成后，自动按回车。");
        demoTitle.Text=UiText.T("Try it here","在这里试一下");demoHint.Text=UiText.T("Dictate into this box. Nothing is sent anywhere.","在下方输入框中听写，内容不会发送出去。");Demo.AccessibleName=UiText.T("Local test box","本地测试框");UpdateCount();
        options.Text=UiText.T("Options…","更多设置");shortcut.Text=UiText.T("Toggle  Ctrl + Shift + Enter","快捷开关  Ctrl + Shift + Enter");status.Text=UiText.T("Runs in the tray when you close this window.","关闭窗口后继续在系统托盘运行。");
        if(tray!=null){toggleMenu.Text=settings.Enabled?UiText.T("Pause iEnter","暂停自动回车"):UiText.T("Enable iEnter","开启自动回车");toggleMenu.Checked=settings.Enabled;settingsMenu.Text=UiText.T("Open iEnter","打开 iEnter");exitMenu.Text=UiText.T("Quit","退出");tray.Text="iEnter · "+(settings.Enabled?UiText.T("On","已开启"):UiText.T("Paused","已暂停"));}
        if(engine!=null&&!engine.HotkeyRegistered)status.Text=UiText.T("Shortcut unavailable. Use the switch or tray menu.","快捷键被占用，可以使用开关或托盘菜单。");
    }
    private void Restore(){Show();WindowState=FormWindowState.Normal;Activate();}
    protected override bool ShowWithoutActivation { get { return preview || base.ShowWithoutActivation; } }
    internal static int RenderPreview(string directory) {
        Directory.CreateDirectory(directory);
        foreach(var lang in new[]{"en","zh"}) {
            var s=new Settings {Enabled=true,Mode="over",EndWord="over",Language=lang};
            using(var form=new MainForm(s,true)) {
                // Give child controls their normal visible layout, without displaying
                // a window or taking focus away from the user's active application.
                form.ShowInTaskbar=false;form.Opacity=0;form.StartPosition=FormStartPosition.Manual;
                form.Location=new Point(-10000,-10000);form.Show();form.PerformLayout();Application.DoEvents();
                using(var bitmap=new Bitmap(form.ClientSize.Width,form.ClientSize.Height)) {form.Controls[0].DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(Path.Combine(directory,"ienter-"+lang+".png"),System.Drawing.Imaging.ImageFormat.Png);}
                form.Close();
            }
        }
        return 0;
    }
}
internal sealed class OptionsForm : Form {
    internal OptionsForm(Settings settings,Func<bool> save) {
        SuspendLayout();Text=UiText.T("iEnter · Options","iEnter · 更多设置");AutoScaleDimensions=new SizeF(96,96);AutoScaleMode=AutoScaleMode.Dpi;Font=new Font("Segoe UI",10);ForeColor=Theme.Ink;BackColor=Theme.Background;
        ClientSize=new Size(490,260);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;StartPosition=FormStartPosition.CenterParent;ShowInTaskbar=false;
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,Padding=new Padding(24),ColumnCount=1,RowCount=6};root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));Controls.Add(root);
        var all=new CheckBox {AutoSize=true,Text=UiText.T("Enable in every app","在所有应用中生效"),Checked=settings.AllApps};root.Controls.Add(all);
        root.Controls.Add(new Label {AutoSize=true,Text=UiText.T("Or limit to these process names, separated by commas:","或仅在这些程序中生效，进程名用逗号分隔："),Margin=new Padding(0,12,0,6)});
        var apps=new TextBox {Dock=DockStyle.Fill,Text=settings.AllowedApps,Enabled=!settings.AllApps,AccessibleName=UiText.T("Allowed apps","允许的程序")};root.Controls.Add(apps);
        var timing=new FlowLayoutPanel {AutoSize=true,Dock=DockStyle.Fill,Margin=new Padding(0,16,0,0)};timing.Controls.Add(new Label {AutoSize=true,Text=UiText.T("Wait before Enter","回车前等待"),Margin=new Padding(0,5,14,0)});
        var delay=new NumericUpDown {Minimum=300,Maximum=3000,Increment=100,Value=settings.DelayMs,Width=100,AccessibleName=UiText.T("Delay in milliseconds","延迟毫秒数")};timing.Controls.Add(delay);timing.Controls.Add(new Label {Text="ms",AutoSize=true,Margin=new Padding(4,5,0,0)});root.Controls.Add(timing);
        root.Controls.Add(new Label {AutoSize=true,ForeColor=Theme.Muted,Font=new Font(Font.FontFamily,9),Text=UiText.T("Clicking, typing or changing focus cancels a pending Enter.","等待期间点击、输入或切换焦点，会取消待执行的回车。"),Margin=new Padding(0,12,0,10)});
        var done=new SoftButton {Text=UiText.T("Done","完成"),Size=new Size(90,34),Anchor=AnchorStyles.Right,DialogResult=DialogResult.OK};root.Controls.Add(done);AcceptButton=done;
        all.CheckedChanged+=(o,e)=>{settings.AllApps=all.Checked;apps.Enabled=!all.Checked;save();};apps.TextChanged+=(o,e)=>{settings.AllowedApps=apps.Text;save();};delay.ValueChanged+=(o,e)=>{settings.DelayMs=(int)delay.Value;save();};ResumeLayout(true);
    }
}
}
