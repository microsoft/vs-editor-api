namespace System.Windows.Media
{
	public abstract class Brush  : System.Windows.Media.Animation.Animatable
	{
		public System.String ToString(System.IFormatProvider param0){throw new System.NotImplementedException();}

		public new System.Windows.Media.Brush Clone() { return this; }
		public System.Double Opacity { get; set; } = 1.0;

	}
}
