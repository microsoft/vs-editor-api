namespace System.Windows.Media.Animation
{
	public abstract class Animatable  : System.Windows.Freezable
	{		public virtual void BeginAnimation(System.Windows.DependencyProperty param0, System.Windows.Media.Animation.AnimationTimeline param1){throw new System.NotImplementedException();}

		protected override Freezable CreateInstanceCore()
		{
			return null;
		}
	}
}