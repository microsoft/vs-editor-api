namespace System.Windows.Media.Imaging
{
	public  class BitmapImage  : System.Windows.Media.Imaging.BitmapSource
	{		public BitmapImage(){}
		public virtual void BeginInit(){throw new System.NotImplementedException();}
		public virtual void EndInit(){throw new System.NotImplementedException();}


		public System.IO.Stream StreamSource { set {  throw new System.NotImplementedException(); } }

		public override double Height => throw new NotImplementedException();

		public override double Width => throw new NotImplementedException();
	}
}