using System.Drawing;
using System.Drawing.Drawing2D;
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
    internal static void Draw(Graphics g, RectangleF bounds) {
        var state=g.Save();
        try {
            float scale=System.Math.Min(bounds.Width,bounds.Height)/512f;
            g.TranslateTransform(bounds.X+(bounds.Width-512*scale)/2,bounds.Y+(bounds.Height-512*scale)/2);
            g.ScaleTransform(scale,scale);g.SmoothingMode=SmoothingMode.AntiAlias;
            using(var bg=Theme.Round(new RectangleF(16,16,480,480),112))
            using(var fill=new SolidBrush(Color.FromArgb(0,122,255))) g.FillPath(fill,bg);
            using(var path=new GraphicsPath())
            using(var pen=new Pen(Color.White,56) {StartCap=LineCap.Round,EndCap=LineCap.Round,LineJoin=LineJoin.Round}) {
                path.AddLine(376,132,376,240);
                path.AddBezier(376,240,376,275.346f,347.346f,304,312,304);
                path.AddLine(312,304,136,304);path.StartFigure();
                path.AddLine(208,232,136,304);path.AddLine(136,304,208,376);
                g.DrawPath(pen,path);
            }
        } finally {g.Restore(state);}
    }
    protected override void OnPaint(PaintEventArgs e) {
        e.Graphics.Clear(Parent==null ? Theme.Background : Parent.BackColor);
        Draw(e.Graphics,ClientRectangle);
    }
}
}
