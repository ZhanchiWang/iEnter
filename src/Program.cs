using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace TypelessAutoEnter {
internal static class Program {
    [STAThread] private static int Main(string[] args) {
        Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
        if(args.Length>0 && args[0]=="--self-test") return Tests.Run(args.Length>1?args[1]:"test-results");
        if(args.Length>1 && args[0]=="--integration-test") return Integration.Run(args[1],args.Length>2?args[2]:null);
        if(args.Length>1 && args[0]=="--render-preview") return MainForm.RenderPreview(args[1]);
        var settings=Settings.Load();UiText.Language=settings.Language;
        bool created;
        using(var mutex=new Mutex(true,"Local\\TypelessAutoEnter.Windows.v1",out created)) {
            if(!created) { MessageBox.Show(UiText.T("iEnter is already running. Open it from the system tray.","iEnter 已在运行，请在任务栏托盘中打开。"),"iEnter");return 0; }
            try { Application.Run(new MainForm(settings)); return 0; }
            catch(Exception ex) { MessageBox.Show(UiText.T("Could not start: ","启动失败：")+ex.Message,"iEnter");return 1; }
        }
    }
}
}
