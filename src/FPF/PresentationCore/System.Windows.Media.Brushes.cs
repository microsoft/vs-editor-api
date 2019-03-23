namespace System.Windows.Media
{
	public  class Brushes  : System.Object
	{
		static SolidColorBrush pink = new SolidColorBrush(Colors.Pink);
		public static System.Windows.Media.SolidColorBrush Black { get => pink; }
		public static System.Windows.Media.SolidColorBrush Blue { get => pink; }
		public static System.Windows.Media.SolidColorBrush DarkGray { get => pink; }
		public static System.Windows.Media.SolidColorBrush Gray { get => pink; }
		public static System.Windows.Media.SolidColorBrush LightBlue { get => pink; }
		public static System.Windows.Media.SolidColorBrush LightGray { get => pink; }
		public static System.Windows.Media.SolidColorBrush LightYellow { get => pink; }
		public static System.Windows.Media.SolidColorBrush PaleVioletRed { get => pink; }
		public static System.Windows.Media.SolidColorBrush Red { get => pink; }
		public static System.Windows.Media.SolidColorBrush Transparent { get; } = new SolidColorBrush(Colors.Transparent);
		public static System.Windows.Media.SolidColorBrush White { get => pink; }
        public static System.Windows.Media.SolidColorBrush Yellow { get => pink; }
        public static System.Windows.Media.SolidColorBrush Green { get => pink; }

    }
}