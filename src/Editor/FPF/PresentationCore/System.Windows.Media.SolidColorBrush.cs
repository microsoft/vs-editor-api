namespace System.Windows.Media
{
	public class SolidColorBrush : System.Windows.Media.Brush
	{
		public SolidColorBrush(System.Windows.Media.Color color)
		{
			this.Color = color;
		}

		public new System.Windows.Media.SolidColorBrush Clone() { return new SolidColorBrush(Color); }
		public System.Windows.Media.Color Color { get; set; }

	}
}