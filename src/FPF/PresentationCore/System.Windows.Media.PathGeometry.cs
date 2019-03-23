namespace System.Windows.Media
{
	public  class PathGeometry  : System.Windows.Media.Geometry
	{		public PathGeometry(){}
		public PathGeometry(System.Collections.Generic.IEnumerable<System.Windows.Media.PathFigure> param0){}
		public void AddGeometry(System.Windows.Media.Geometry param0){throw new System.NotImplementedException();}

		public override bool IsEmpty()
		{
			throw new NotImplementedException();
		}

		public System.Windows.Media.FillRule FillRule { set {  throw new System.NotImplementedException(); } }
		public System.Windows.Media.PathFigureCollection Figures { get {  throw new System.NotImplementedException(); } }

	}
}