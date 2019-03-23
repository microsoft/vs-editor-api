namespace System.Windows.Media
{
    public abstract class Geometry : System.Windows.Media.Animation.Animatable
    { public abstract System.Boolean IsEmpty();


        public static System.Windows.Media.Geometry Parse(System.String param0) { throw new System.NotImplementedException(); }
        public System.Windows.Media.IntersectionDetail FillContainsWithDetail(System.Windows.Media.Geometry param0) { throw new System.NotImplementedException(); }
        public System.Windows.Media.PathGeometry GetOutlinedPathGeometry() { throw new System.NotImplementedException(); }


        public System.Windows.Media.Transform Transform { get { throw new System.NotImplementedException(); } set { throw new System.NotImplementedException(); } }
        public static System.Windows.Media.Geometry Empty { get; } = new GeometryGroup();
        public virtual System.Windows.Rect Bounds { get {  throw new System.NotImplementedException(); } }

	}
}