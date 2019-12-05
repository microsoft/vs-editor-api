namespace System.Windows.Media
{
    public class Pen : Animation.Animatable
    {
        public Brush Brush { get; set; }
        public double Thickness { get; set; }
        public PenLineCap EndLineCap { get; set; }
        public PenLineJoin LineJoin { get; set; }
        public double MiterLimit { get; set; }
        public PenLineCap StartLineCap { get; set; }
        public DashStyle DashStyle { get; set; }

        public Pen () { }

        public Pen (Brush brush, double thickness)
        {
            Thickness = thickness;
            Brush = brush;
        }

        public new Pen Clone () { throw new NotImplementedException (); }
    }
}
