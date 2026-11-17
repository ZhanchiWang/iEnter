using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace TypelessAutoEnter {
// Matches assets/ienter.svg. Paths are rasterized at the control's actual
// physical size, never enlarged from the 32px shell icon.
internal sealed class AppIcon : Control {
    internal AppIcon() {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
        AccessibleName="iEnter";
    }
    internal static Icon LoadIcon(Size size) {
        using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("iEnter.Icon"))
        using(var icon=new Icon(stream,size)) return (Icon)icon.Clone();
    }
    protected override void OnPaint(PaintEventArgs e) {
        e.Graphics.Clear(Parent==null ? Theme.Background : Parent.BackColor);
        IconArtwork.Draw(e.Graphics,ClientRectangle);
    }
}
}
