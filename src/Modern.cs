using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TypelessAutoEnter {
internal static class Theme {
    internal static readonly Color Background=Color.FromArgb(246,247,249), Ink=Color.FromArgb(29,29,31), Muted=Color.FromArgb(110,113,121), Blue=Color.FromArgb(0,112,245), Line=Color.FromArgb(230,232,237), Field=Color.FromArgb(244,245,248);
    internal static GraphicsPath Round(RectangleF r,float radius) {
        float d=radius*2;var p=new GraphicsPath();
        p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);
        p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;
    }
}
internal sealed class Card : Panel {
    internal Card() { DoubleBuffered=true; BackColor=Color.White; Padding=new Padding(18); }
    protected override void OnPaintBackground(PaintEventArgs e) {
        e.Graphics.Clear(Parent==null ? Theme.Background : Parent.BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        using(var p=Theme.Round(new RectangleF(0.5f,0.5f,Width-1,Height-1),14*DeviceDpi/96f))
        using(var b=new SolidBrush(BackColor)) using(var pen=new Pen(Theme.Line)) {e.Graphics.FillPath(b,p);e.Graphics.DrawPath(pen,p);}
    }
}
internal sealed class SoftButton : Button {
    internal Color Fill=Color.White;
    internal SoftButton() { FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;Cursor=Cursors.Hand;SetStyle(ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.AllPaintingInWmPaint,true); }
    protected override void OnPaint(PaintEventArgs e) {
        e.Graphics.Clear(Parent.BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        using(var p=Theme.Round(new RectangleF(0.5f,0.5f,Width-1,Height-1),9*DeviceDpi/96f))
        using(var b=new SolidBrush(Enabled ? Fill : Theme.Field)) using(var pen=new Pen(Focused ? Theme.Blue : Theme.Line)) {e.Graphics.FillPath(b,p);e.Graphics.DrawPath(pen,p);}
        TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,Enabled ? ForeColor : Theme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
    }
}
internal sealed class Toggle : CheckBox {
    internal Toggle() { AutoSize=false;Size=new Size(50,28);Cursor=Cursors.Hand;Text="";SetStyle(ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.AllPaintingInWmPaint,true); }
    protected override void OnCheckedChanged(EventArgs e) { base.OnCheckedChanged(e);Invalidate(); }
    protected override void OnPaint(PaintEventArgs e) {
        e.Graphics.Clear(Parent.BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        float scale=DeviceDpi/96f;var rect=new RectangleF(scale,2*scale,Width-2*scale,Height-4*scale);
        using(var p=Theme.Round(rect,rect.Height/2)) using(var b=new SolidBrush(Checked ? Theme.Blue : Color.FromArgb(196,200,208))) e.Graphics.FillPath(b,p);
        float d=rect.Height-4*scale,x=Checked?rect.Right-d-2*scale:rect.Left+2*scale;
        using(var b=new SolidBrush(Color.White))e.Graphics.FillEllipse(b,x,rect.Y+2*scale,d,d);
        if(Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics,ClientRectangle);
    }
}
internal sealed class Segment : RadioButton {
    internal Segment() { AutoSize=false;Appearance=Appearance.Button;FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;Cursor=Cursors.Hand;TextAlign=ContentAlignment.MiddleCenter;SetStyle(ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.AllPaintingInWmPaint,true); }
    protected override void OnCheckedChanged(EventArgs e) {base.OnCheckedChanged(e);Invalidate();}
    protected override void OnPaint(PaintEventArgs e) {
        e.Graphics.Clear(Parent.BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        using(var p=Theme.Round(new RectangleF(1,1,Width-2,Height-2),8*DeviceDpi/96f))using(var b=new SolidBrush(Checked?Color.FromArgb(232,242,255):Theme.Field))e.Graphics.FillPath(b,p);
        TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,Checked?Theme.Blue:Theme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
        if(Focused && ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(ClientRectangle,-4,-4));
    }
}
}
