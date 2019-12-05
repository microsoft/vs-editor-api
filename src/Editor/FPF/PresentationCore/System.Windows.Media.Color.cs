using System.Runtime.InteropServices;
using System.Text;

namespace System.Windows.Media
{
	public struct Color : IFormattable, IEquatable<Color>
	{
		//------------------------------------------------------
		//
		//  Constructors
		//
		//------------------------------------------------------

		#region Constructors



		///<summary>
		/// FromAValues - general constructor for multichannel color values with explicit alpha channel and color context, i.e. spectral colors
		///</summary>
		public static Color FromAValues(float a, float[] values, Uri profileUri)
		{
			throw new NotImplementedException();
		}

		///<summary>
		/// FromValues - general color constructor for multichannel color values with opaque alpha channel and explicit color context, i.e. spectral colors
		///</summary>
		public static Color FromValues(float[] values, Uri profileUri)
		{
			Color c1 = Color.FromAValues(1.0f, values, profileUri);

			return c1;
		}

		///<summary>
		/// Color - sRgb legacy interface, assumes Rgb values are sRgb
		///</summary>
		internal static Color FromUInt32(uint argb)// internal legacy sRGB interface
		{
			Color c1 = new Color();

			c1.sRgbColor.a = (byte)((argb & 0xff000000) >> 24);
			c1.sRgbColor.r = (byte)((argb & 0x00ff0000) >> 16);
			c1.sRgbColor.g = (byte)((argb & 0x0000ff00) >> 8);
			c1.sRgbColor.b = (byte)(argb & 0x000000ff);
			c1.scRgbColor.a = (float)c1.sRgbColor.a / 255.0f;
			c1.scRgbColor.r = sRgbToScRgb(c1.sRgbColor.r);  // note that context is undefined and thus unloaded
			c1.scRgbColor.g = sRgbToScRgb(c1.sRgbColor.g);
			c1.scRgbColor.b = sRgbToScRgb(c1.sRgbColor.b);
			c1.context = null;

			c1.isFromScRgb = false;

			return c1;
		}

		///<summary>
		/// FromScRgb
		///</summary>
		public static Color FromScRgb(float a, float r, float g, float b)
		{
			Color c1 = new Color();

			c1.scRgbColor.r = r;
			c1.scRgbColor.g = g;
			c1.scRgbColor.b = b;
			c1.scRgbColor.a = a;
			if (a < 0.0f) {
				a = 0.0f;
			} else if (a > 1.0f) {
				a = 1.0f;
			}

			c1.sRgbColor.a = (byte)((a * 255.0f) + 0.5f);
			c1.sRgbColor.r = ScRgbTosRgb(c1.scRgbColor.r);
			c1.sRgbColor.g = ScRgbTosRgb(c1.scRgbColor.g);
			c1.sRgbColor.b = ScRgbTosRgb(c1.scRgbColor.b);
			c1.context = null;

			c1.isFromScRgb = true;

			return c1;
		}

		///<summary>
		/// Color - sRgb legacy interface, assumes Rgb values are sRgb, alpha channel is linear 1.0 gamma
		///</summary>
		public static Color FromArgb(byte a, byte r, byte g, byte b)// legacy sRGB interface, bytes are required to properly round trip
		{
			Color c1 = new Color();

			c1.scRgbColor.a = (float)a / 255.0f;
			c1.scRgbColor.r = sRgbToScRgb(r);  // note that context is undefined and thus unloaded
			c1.scRgbColor.g = sRgbToScRgb(g);
			c1.scRgbColor.b = sRgbToScRgb(b);
			c1.context = null;
			c1.sRgbColor.a = a;
			c1.sRgbColor.r = ScRgbTosRgb(c1.scRgbColor.r);
			c1.sRgbColor.g = ScRgbTosRgb(c1.scRgbColor.g);
			c1.sRgbColor.b = ScRgbTosRgb(c1.scRgbColor.b);

			c1.isFromScRgb = false;

			return c1;
		}

		///<summary>
		/// Color - sRgb legacy interface, assumes Rgb values are sRgb
		///</summary>
		public static Color FromRgb(byte r, byte g, byte b)// legacy sRGB interface, bytes are required to properly round trip
		{
			Color c1 = Color.FromArgb(0xff, r, g, b);
			return c1;
		}
		#endregion Constructors

		//------------------------------------------------------
		//
		//  Public Methods
		//
		//------------------------------------------------------

		#region Public Methods
		///<summary>
		/// GetHashCode
		///</summary>
		public override int GetHashCode()
		{
			return this.scRgbColor.GetHashCode(); //^this.context.GetHashCode();
		}

		/// <summary>
		/// Creates a string representation of this object based on the current culture.
		/// </summary>
		/// <returns>
		/// A string representation of this object.
		/// </returns>
		public override string ToString()
		{
			// Delegate to the internal method which implements all ToString calls.

			string format = isFromScRgb ? c_scRgbFormat : null;

			return ConvertToString(format, null);
		}

		/// <summary>
		/// Creates a string representation of this object based on the IFormatProvider
		/// passed in.  If the provider is null, the CurrentCulture is used.
		/// </summary>
		/// <returns>
		/// A string representation of this object.
		/// </returns>
		public string ToString(IFormatProvider provider)
		{
			// Delegate to the internal method which implements all ToString calls.

			string format = isFromScRgb ? c_scRgbFormat : null;

			return ConvertToString(format, provider);
		}

		/// <summary>
		/// Creates a string representation of this object based on the format string
		/// and IFormatProvider passed in.
		/// If the provider is null, the CurrentCulture is used.
		/// See the documentation for IFormattable for more information.
		/// </summary>
		/// <returns>
		/// A string representation of this object.
		/// </returns>
		string IFormattable.ToString(string format, IFormatProvider provider)
		{
			// Delegate to the internal method which implements all ToString calls.
			return ConvertToString(format, provider);
		}

		/// <summary>
		/// Creates a string representation of this object based on the format string
		/// and IFormatProvider passed in.
		/// If the provider is null, the CurrentCulture is used.
		/// See the documentation for IFormattable for more information.
		/// </summary>
		/// <returns>
		/// A string representation of this object.
		/// </returns>
		internal string ConvertToString(string format, IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Compares two colors for fuzzy equality.  This function
		/// helps compensate for the fact that float values can
		/// acquire error when operated upon
		/// </summary>
		/// <param name='color1'>The first color to compare</param>
		/// <param name='color2'>The second color to compare</param>
		/// <returns>Whether or not the two colors are equal</returns>
		public static bool AreClose(Color color1, Color color2)
		{
			return color1.IsClose(color2);
		}

		/// <summary>
		/// Compares two colors for fuzzy equality.  This function
		/// helps compensate for the fact that float values can
		/// acquire error when operated upon
		/// </summary>
		/// <param name='color'>The color to compare to this</param>
		/// <returns>Whether or not the two colors are equal</returns>
		private bool IsClose(Color color)
		{
			throw new NotImplementedException(); ;
		}

		///<summary>
		/// Clamp - the color channels to the gamut [0..1].  If a channel is out
		/// of gamut, it will be set to 1, which represents full saturation.
		/// todo: sync up context values if they exist
		///</summary>
		public void Clamp()
		{
			scRgbColor.r = (scRgbColor.r < 0) ? 0 : (scRgbColor.r > 1.0f) ? 1.0f : scRgbColor.r;
			scRgbColor.g = (scRgbColor.g < 0) ? 0 : (scRgbColor.g > 1.0f) ? 1.0f : scRgbColor.g;
			scRgbColor.b = (scRgbColor.b < 0) ? 0 : (scRgbColor.b > 1.0f) ? 1.0f : scRgbColor.b;
			scRgbColor.a = (scRgbColor.a < 0) ? 0 : (scRgbColor.a > 1.0f) ? 1.0f : scRgbColor.a;
			sRgbColor.a = (byte)(scRgbColor.a * 255f);
			sRgbColor.r = ScRgbTosRgb(scRgbColor.r);
			sRgbColor.g = ScRgbTosRgb(scRgbColor.g);
			sRgbColor.b = ScRgbTosRgb(scRgbColor.b);

			//
		}

		///<summary>
		/// GetNativeColorValues - return color values from color context
		///</summary>
		public float[] GetNativeColorValues()
		{
			if (context != null) {
				return (float[])nativeColorValue.Clone();
			} else {
				throw new InvalidOperationException();
			}
		}
		#endregion Public Methods

		//------------------------------------------------------
		//
		//  Public Operators
		//
		//------------------------------------------------------

		#region Public Operators
		///<summary>
		/// Addition operator - Adds each channel of the second color to each channel of the
		/// first and returns the result
		///</summary>
		public static Color operator +(Color color1, Color color2)
		{
			throw new NotImplementedException();
		}

		///<summary>
		/// Addition method - Adds each channel of the second color to each channel of the
		/// first and returns the result
		///</summary>
		public static Color Add(Color color1, Color color2)
		{
			return (color1 + color2);
		}

		/// <summary>
		/// Subtract operator - substracts each channel of the second color from each channel of the
		/// first and returns the result
		/// </summary>
		/// <param name='color1'>The minuend</param>
		/// <param name='color2'>The subtrahend</param>
		/// <returns>Returns the unclamped differnce</returns>
		public static Color operator -(Color color1, Color color2)
		{

			throw new NotImplementedException();
		}

		///<summary>
		/// Subtract method - subtracts each channel of the second color from each channel of the
		/// first and returns the result
		///</summary>
		public static Color Subtract(Color color1, Color color2)
		{
			return (color1 - color2);
		}

		/// <summary>
		/// Multiplication operator - Multiplies each channel of the color by a coefficient and returns the result
		/// </summary>
		/// <param name='color'>The color</param>
		/// <param name='coefficient'>The coefficient</param>
		/// <returns>Returns the unclamped product</returns>
		public static Color operator *(Color color, float coefficient)
		{
			Color c1 = FromScRgb(color.scRgbColor.a * coefficient, color.scRgbColor.r * coefficient, color.scRgbColor.g * coefficient, color.scRgbColor.b * coefficient);

			if (color.context == null) {
				return c1;
			} else {
				c1.context = color.context;

#pragma warning disable 6506 // c1.context is obviously not null
				c1.ComputeNativeValues(0);
#pragma warning restore 6506
			}

			return c1;
		}

		///<summary>
		/// Multiplication method - Multiplies each channel of the color by a coefficient and returns the result
		///</summary>
		public static Color Multiply(Color color, float coefficient)
		{
			return (color * coefficient);
		}

		///<summary>
		/// Equality method for two colors - return true of colors are equal, otherwise returns false
		///</summary>
		public static bool Equals(Color color1, Color color2)
		{
			return (color1 == color2);
		}

		/// <summary>
		/// Compares two colors for exact equality.  Note that float values can acquire error
		/// when operated upon, such that an exact comparison between two values which are logically
		/// equal may fail. see cref="AreClose" for a "fuzzy" version of this comparison.
		/// </summary>
		/// <param name='color'>The color to compare to "this"</param>
		/// <returns>Whether or not the two colors are equal</returns>
		public bool Equals(Color color)
		{
			return this == color;
		}

		/// <summary>
		/// Compares two colors for exact equality.  Note that float values can acquire error
		/// when operated upon, such that an exact comparison between two vEquals(color);alues which are logically
		/// equal may fail. see cref="AreClose" for a "fuzzy" version of this comparison.
		/// </summary>
		/// <param name='o'>The object to compare to "this"</param>
		/// <returns>Whether or not the two colors are equal</returns>
		public override bool Equals(object o)
		{
			if (o is Color) {
				Color color = (Color)o;

				return (this == color);
			} else {
				return false;
			}
		}

		///<summary>
		/// IsEqual operator - Compares two colors for exact equality.  Note that float values can acquire error
		/// when operated upon, such that an exact comparison between two values which are logically
		/// equal may fail. see cref="AreClose".
		///</summary>
		public static bool operator ==(Color color1, Color color2)
		{
			if (color1.scRgbColor.r != color2.scRgbColor.r) {
				return false;
			}

			if (color1.scRgbColor.g != color2.scRgbColor.g) {
				return false;
			}

			if (color1.scRgbColor.b != color2.scRgbColor.b) {
				return false;
			}

			if (color1.scRgbColor.a != color2.scRgbColor.a) {
				return false;
			}

			return true;
		}

		///<summary>
		/// !=
		///</summary>
		public static bool operator !=(Color color1, Color color2)
		{
			return (!(color1 == color2));
		}
		#endregion Public Operators

		//------------------------------------------------------
		//
		//  Public Properties
		//
		//------------------------------------------------------

		#region Public Properties


		///<summary>
		/// A
		///</summary>
		public byte A {
			get {
				return sRgbColor.a;
			}
			set {
				scRgbColor.a = (float)value / 255.0f;
				sRgbColor.a = value;
			}
		}

		/// <value>The Red channel as a byte whose range is [0..255].
		/// the value is not allowed to be out of range</value>
		/// <summary>
		/// R
		/// </summary>
		public byte R {
			get {
				return sRgbColor.r;
			}
			set {
				if (context == null) {
					scRgbColor.r = sRgbToScRgb(value);
					sRgbColor.r = value;
				} else {
					throw new InvalidOperationException();
				}
			}
		}

		///<value>The Green channel as a byte whose range is [0..255].
		/// the value is not allowed to be out of range</value><summary>
		/// G
		///</summary>
		public byte G {
			get {
				return sRgbColor.g;
			}
			set {
				if (context == null) {
					scRgbColor.g = sRgbToScRgb(value);
					sRgbColor.g = value;
				} else {
					throw new InvalidOperationException();
				}
			}
		}

		///<value>The Blue channel as a byte whose range is [0..255].
		/// the value is not allowed to be out of range</value><summary>
		/// B
		///</summary>
		public byte B {
			get {
				return sRgbColor.b;
			}
			set {
				if (context == null) {
					scRgbColor.b = sRgbToScRgb(value);
					sRgbColor.b = value;
				} else {
					throw new InvalidOperationException();
				}
			}
		}

		///<value>The Alpha channel as a float whose range is [0..1].
		/// the value is allowed to be out of range</value><summary>
		/// ScA
		///</summary>
		public float ScA {
			get {
				return scRgbColor.a;
			}
			set {
				scRgbColor.a = value;
				if (value < 0.0f) {
					sRgbColor.a = 0;
				} else if (value > 1.0f) {
					sRgbColor.a = (byte)255;
				} else {
					sRgbColor.a = (byte)(value * 255f);
				}
			}
		}

		///<value>The Red channel as a float whose range is [0..1].
		/// the value is allowed to be out of range</value>
		///<summary>
		/// ScR
		///</summary>
		public float ScR {
			get {
				return scRgbColor.r;
				// throw new ArgumentException(SR.Get(SRID.Color_ColorContextNotsRgb_or_ScRgb, null));
			}
			set {
				if (context == null) {
					scRgbColor.r = value;
					sRgbColor.r = ScRgbTosRgb(value);
				} else {
					throw new InvalidOperationException();
				}
			}
		}

		///<value>The Green channel as a float whose range is [0..1].
		/// the value is allowed to be out of range</value><summary>
		/// ScG
		///</summary>
		public float ScG {
			get {
				return scRgbColor.g;
				// throw new ArgumentException(SR.Get(SRID.Color_ColorContextNotsRgb_or_ScRgb, null));
			}
			set {
				if (context == null) {
					scRgbColor.g = value;
					sRgbColor.g = ScRgbTosRgb(value);
				} else {
					throw new InvalidOperationException();
				}
			}
		}

		///<value>The Blue channel as a float whose range is [0..1].
		/// the value is allowed to be out of range</value><summary>
		/// ScB
		///</summary>
		public float ScB {
			get {
				return scRgbColor.b;
				// throw new ArgumentException(SR.Get(SRID.Color_ColorContextNotsRgb_or_ScRgb, null));
			}
			set {
				if (context == null) {
					scRgbColor.b = value;
					sRgbColor.b = ScRgbTosRgb(value);
				} else {
					throw new InvalidOperationException();
				}
			}
		}

		#endregion Public Properties

		//------------------------------------------------------
		//
		//  Public Events
		//
		//------------------------------------------------------
		//------------------------------------------------------
		//
		//  Public Events
		//
		//------------------------------------------------------
		//------------------------------------------------------
		//
		//  Protected Methods
		//
		//------------------------------------------------------
		//------------------------------------------------------
		//
		//  Internal Properties
		//
		//------------------------------------------------------
		//------------------------------------------------------
		//
		//  Internal Events
		//
		//------------------------------------------------------
		//------------------------------------------------------
		//
		//  Internal Methods
		//
		//------------------------------------------------------
		//------------------------------------------------------
		//
		//  Private Methods
		//
		//------------------------------------------------------
		#region Private Methods

		///<summary>
		/// private helper function to set context values from a color value with a set context and ScRgb values
		///</summary>
		private static float sRgbToScRgb(byte bval)
		{
			float val = ((float)bval / 255.0f);

			if (!(val > 0.0))       // Handles NaN case too. (Though, NaN isn't actually
									// possible in this case.)
			{
				return (0.0f);
			} else if (val <= 0.04045) {
				return (val / 12.92f);
			} else if (val < 1.0f) {
				return (float)Math.Pow(((double)val + 0.055) / 1.055, 2.4);
			} else {
				return (1.0f);
			}
		}

		///<summary>
		/// private helper function to set context values from a color value with a set context and ScRgb values
		///</summary>
		///
		private static byte ScRgbTosRgb(float val)
		{
			if (!(val > 0.0))       // Handles NaN case too
			{
				return (0);
			} else if (val <= 0.0031308) {
				return ((byte)((255.0f * val * 12.92f) + 0.5f));
			} else if (val < 1.0) {
				return ((byte)((255.0f * ((1.055f * (float)Math.Pow((double)val, (1.0 / 2.4))) - 0.055f)) + 0.5f));
			} else {
				return (255);
			}
		}

		///<summary>
		/// private helper function to set context values from a color value with a set context and ScRgb values
		///</summary>
		///
		private void ComputeScRgbValues()
		{
			throw new NotImplementedException();
		}

		private void ComputeNativeValues(int numChannels)
		{
			throw new NotImplementedException();
		}

		#endregion Private Methods

		//------------------------------------------------------
		//
		//  Private Properties
		//
		//------------------------------------------------------
		//------------------------------------------------------
		//
		//  Private Events
		//
		//------------------------------------------------------
		//------------------------------------------------------
		//
		//  Private Fields
		//
		//------------------------------------------------------

		#region Private Fields

		[MarshalAs(UnmanagedType.Interface)]
		object context;

		private struct MILColorF // this structure is the "milrendertypes.h" structure and should be identical for performance
		{
			public float a, r, g, b;
		};

		private MILColorF scRgbColor;

		private struct MILColor
		{
			public byte a, r, g, b;
		}

		private MILColor sRgbColor;

#pragma warning disable 649
		private float[] nativeColorValue;
#pragma warning restore 649

		private bool isFromScRgb;

		private const string c_scRgbFormat = "R";

		#endregion Private Fields
	}
}
