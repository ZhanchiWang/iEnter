using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TypelessAutoEnter {
// Shared vector artwork for the live UI and all exported Windows icon sizes.
// Geometry matches the approved assets/ienter.svg at a 384-unit design size.
public static class IconArtwork {
    public static void Draw(Graphics g, RectangleF bounds) {
        var state=g.Save();
        try {
            float scale=Math.Min(bounds.Width,bounds.Height)/384f;
            g.TranslateTransform(bounds.X+(bounds.Width-384*scale)/2,bounds.Y+(bounds.Height-384*scale)/2);
            g.ScaleTransform(scale,scale);g.SmoothingMode=SmoothingMode.AntiAlias;
            using(var p=Outer(true)) using(var b=new SolidBrush(Color.FromArgb(29,32,38))) g.FillPath(b,p);
            using(var p=Outer(false)) using(var b=new SolidBrush(Color.FromArgb(89,94,104))) g.FillPath(b,p);
            using(var p=Face()) using(var b=new SolidBrush(Color.FromArgb(56,59,66))) g.FillPath(b,p);
            // Approved position: centered horizontally, then lowered by one stroke.
            g.TranslateTransform(-45,49);
            using(var p=new GraphicsPath())
            using(var pen=new Pen(Color.White,20) {StartCap=LineCap.Round,EndCap=LineCap.Round,LineJoin=LineJoin.Round}) {
                p.AddLine(292,102,292,178);
                // Quadratic Q292 194 276 194 expressed as a cubic Bezier.
                p.AddBezier(292,178,292,188.666667f,286.666667f,194,276,194);
                p.AddLine(276,194,182,194);p.StartFigure();
                p.AddLine(212,164,182,194);p.AddLine(182,194,212,224);
                g.DrawPath(pen,p);
            }
        } finally {g.Restore(state);}
    }
    private static GraphicsPath Outer(bool baseLayer) {
        var p=new GraphicsPath();p.AddLine(206,42,308,42);
        p.AddBezier(308,42,329,42,346,59,346,80);
        if(baseLayer) {
            p.AddLine(346,80,346,304);p.AddBezier(346,304,346,325,329,342,308,342);
            p.AddLine(308,342,76,342);p.AddBezier(76,342,55,342,38,325,38,304);p.AddLine(38,304,38,212);
        } else {
            p.AddLine(346,80,346,282);p.AddBezier(346,282,346,304,331,320,309,320);
            p.AddLine(309,320,75,320);p.AddBezier(75,320,53,320,38,304,38,282);p.AddLine(38,282,38,212);
        }
        p.AddBezier(38,212,38,191,55,174,76,174);p.AddLine(76,174,156,174);
        p.AddBezier(156,174,165,174,172,167,172,158);p.AddLine(172,158,172,76);
        p.AddBezier(172,76,172,55,186,42,206,42);p.CloseFigure();return p;
    }
    private static GraphicsPath Face() {
        var p=new GraphicsPath();p.AddLine(208,49,305,49);
        p.AddBezier(305,49,322,49,332,60,332,77);p.AddLine(332,77,332,269);
        p.AddBezier(332,269,332,286,322,297,305,297);p.AddLine(305,297,79,297);
        p.AddBezier(79,297,62,297,52,286,52,269);p.AddLine(52,269,52,214);
        p.AddBezier(52,214,52,197,62,187,79,187);p.AddLine(79,187,164,187);
        p.AddBezier(164,187,177,187,186,178,186,165);p.AddLine(186,165,186,77);
        p.AddBezier(186,77,186,60,192,49,208,49);p.CloseFigure();return p;
    }
}
}
