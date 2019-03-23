namespace System.Windows
{
	public abstract class SystemColors : System.Object
	{
		static System.Windows.Media.Color pinkColor = new Media.Color {
			R = 255, G = 192, B = 203, A = 255
		};
		public static System.Windows.Media.Color ControlColor { get { return pinkColor; } }
		public static System.Windows.Media.Color ControlTextColor { get { return pinkColor; } }
		public static System.Windows.Media.Color GrayTextColor { get { return pinkColor; } }
		public static System.Windows.Media.Color HighlightColor { get { return pinkColor; } }
		public static System.Windows.Media.Color HighlightTextColor { get { return pinkColor; } }
		public static System.Windows.Media.SolidColorBrush ActiveBorderBrush { get { return new Media.SolidColorBrush(pinkColor); } }
		public static System.Windows.Media.SolidColorBrush ActiveCaptionBrush { get { return new Media.SolidColorBrush(pinkColor); } }
		public static System.Windows.Media.SolidColorBrush ControlBrush { get { return new Media.SolidColorBrush(pinkColor); } }
		public static System.Windows.Media.SolidColorBrush ControlDarkBrush { get { return new Media.SolidColorBrush(pinkColor); } }
		public static System.Windows.Media.SolidColorBrush ControlLightBrush { get { return new Media.SolidColorBrush(pinkColor); } }
		public static System.Windows.Media.SolidColorBrush ControlLightLightBrush { get { return new Media.SolidColorBrush(pinkColor); } }
		public static System.Windows.Media.SolidColorBrush GrayTextBrush { get { return new Media.SolidColorBrush(pinkColor); } }
		public static System.Windows.Media.SolidColorBrush HighlightBrush { get { return new Media.SolidColorBrush(pinkColor); } }
		public static System.Windows.Media.SolidColorBrush HotTrackBrush { get { return new Media.SolidColorBrush(pinkColor); } }
		public static System.Windows.Media.SolidColorBrush InfoBrush { get { return new Media.SolidColorBrush(pinkColor); } }
		public static System.Windows.Media.SolidColorBrush InfoTextBrush { get { return new Media.SolidColorBrush(pinkColor); } }
		public static System.Windows.Media.SolidColorBrush WindowBrush { get { return new Media.SolidColorBrush(pinkColor); } }
		public static System.Windows.Media.SolidColorBrush WindowFrameBrush { get { return new Media.SolidColorBrush(pinkColor); } }
		public static System.Windows.Media.SolidColorBrush WindowTextBrush {
			get { return new Windows.Media.SolidColorBrush(new Media.Color { R = 0, G = 0, B = 0, A = 255 }); }
		}
	}
}