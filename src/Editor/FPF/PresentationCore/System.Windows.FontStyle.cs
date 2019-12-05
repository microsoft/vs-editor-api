namespace System.Windows
{
	public struct FontStyle
	{
		public static bool operator ==(FontStyle p1, FontStyle p2) { return true; }
		public static bool operator !=(FontStyle p1, FontStyle p2) { return false; }

		public override bool Equals(object obj) => true;
		public override int GetHashCode() => 0;
	}
}
