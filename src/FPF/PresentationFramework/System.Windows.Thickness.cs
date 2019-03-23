//---------------------------------------------------------------------------
//
// Copyright (C) Microsoft Corporation.  All rights reserved.
// 
// File: Thickness.cs
//
// Description: Contains the Thickness (double x4) value type. 
//
// History:  
//  06/02/2003 : greglett  - created.
//
//---------------------------------------------------------------------------

using System.ComponentModel;
using System.Globalization;

namespace System.Windows
{
    /// <summary>
    /// Thickness is a value type used to describe the thickness of frame around a rectangle.
    /// It contains four doubles each corresponding to a side: Left, Top, Right, Bottom.
    /// </summary>
    public struct Thickness : IEquatable<Thickness>
    {
        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------

        #region Constructors
        /// <summary>
        /// This constructur builds a Thickness with a specified value on every side.
        /// </summary>
        /// <param name="uniformLength">The specified uniform length.</param>
        public Thickness(double uniformLength)
        {
            _Left = _Top = _Right = _Bottom = uniformLength;
        }

        /// <summary>
        /// This constructor builds a Thickness with the specified number of pixels on each side.
        /// </summary>
        /// <param name="left">The thickness for the left side.</param>
        /// <param name="top">The thickness for the top side.</param>
        /// <param name="right">The thickness for the right side.</param>
        /// <param name="bottom">The thickness for the bottom side.</param>
        public Thickness(double left, double top, double right, double bottom)
        {
            _Left = left;
            _Top = top;
            _Right = right;
            _Bottom = bottom;
        }


        #endregion


        //-------------------------------------------------------------------
        //
        //  Public Methods
        //
        //-------------------------------------------------------------------

        #region Public Methods

        /// <summary>
        /// This function compares to the provided object for type and value equality.
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if object is a Thickness and all sides of it are equal to this Thickness'.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Thickness)
            {
                Thickness otherObj = (Thickness)obj;
                return (this == otherObj);
            }
            return (false);
        }

        /// <summary>
        /// Compares this instance of Thickness with another instance.
        /// </summary>
        /// <param name="thickness">Thickness instance to compare.</param>
        /// <returns><c>true</c>if this Thickness instance has the same value 
        /// and unit type as thickness.</returns>
        public bool Equals(Thickness thickness)
        {
            return (this == thickness);
        }

        /// <summary>
        /// This function returns a hash code.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return _Left.GetHashCode() ^ _Top.GetHashCode() ^ _Right.GetHashCode() ^ _Bottom.GetHashCode();
        }

        #endregion


        //-------------------------------------------------------------------
        //
        //  Public Operators
        //
        //-------------------------------------------------------------------

        #region Public Operators

        /// <summary>
        /// Overloaded operator to compare two Thicknesses for equality.
        /// </summary>
        /// <param name="t1">first Thickness to compare</param>
        /// <param name="t2">second Thickness to compare</param>
        /// <returns>True if all sides of the Thickness are equal, false otherwise</returns>
        //  SEEALSO
        public static bool operator ==(Thickness t1, Thickness t2)
        {
            return ((t1._Left == t2._Left || (double.IsNaN(t1._Left) && double.IsNaN(t2._Left)))
                    && (t1._Top == t2._Top || (double.IsNaN(t1._Top) && double.IsNaN(t2._Top)))
                    && (t1._Right == t2._Right || (double.IsNaN(t1._Right) && double.IsNaN(t2._Right)))
                    && (t1._Bottom == t2._Bottom || (double.IsNaN(t1._Bottom) && double.IsNaN(t2._Bottom)))
                    );
        }

        /// <summary>
        /// Overloaded operator to compare two Thicknesses for inequality.
        /// </summary>
        /// <param name="t1">first Thickness to compare</param>
        /// <param name="t2">second Thickness to compare</param>
        /// <returns>False if all sides of the Thickness are equal, true otherwise</returns>
        //  SEEALSO
        public static bool operator !=(Thickness t1, Thickness t2)
        {
            return (!(t1 == t2));
        }

        #endregion


        //-------------------------------------------------------------------
        //
        //  Public Properties
        //
        //-------------------------------------------------------------------

        #region Public Properties

        /// <summary>This property is the Length on the thickness' left side</summary>
        public double Left
        {
            get { return _Left; }
            set { _Left = value; }
        }

        /// <summary>This property is the Length on the thickness' top side</summary>
        public double Top
        {
            get { return _Top; }
            set { _Top = value; }
        }

        /// <summary>This property is the Length on the thickness' right side</summary>
        public double Right
        {
            get { return _Right; }
            set { _Right = value; }
        }

        /// <summary>This property is the Length on the thickness' bottom side</summary>
        public double Bottom
        {
            get { return _Bottom; }
            set { _Bottom = value; }
        }
        #endregion

        //-------------------------------------------------------------------
        //
        //  INternal API
        //
        //-------------------------------------------------------------------

        #region Internal API

        internal Size Size
        {
            get
            {
                return new Size(_Left + _Right, _Top + _Bottom);
            }
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Private Fields
        //
        //-------------------------------------------------------------------

        #region Private Fields

        private double _Left;
        private double _Top;
        private double _Right;
        private double _Bottom;

        #endregion
    }
}