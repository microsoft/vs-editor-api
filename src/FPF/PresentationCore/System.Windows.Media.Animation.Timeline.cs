namespace System.Windows.Media.Animation
{
	public abstract class Timeline  : System.Windows.Media.Animation.Animatable
	{

		public event System.EventHandler Completed;
		public System.Windows.Media.Animation.RepeatBehavior RepeatBehavior { set {  throw new System.NotImplementedException(); } }

	}
}