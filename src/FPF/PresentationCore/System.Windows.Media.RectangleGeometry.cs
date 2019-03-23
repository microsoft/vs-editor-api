namespace System.Windows.Media
{
	public  class RectangleGeometry  : System.Windows.Media.Geometry
    {
        public Rect Rectangle;
        public RectangleGeometry(Rect rectangle)
        {
            this.Rectangle = rectangle;
        }

        public override Rect Bounds => Rectangle;

        public override bool IsEmpty()
        {
            return Rectangle.IsEmpty;
        }
    }
}