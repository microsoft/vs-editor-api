//-----------------------------------------------------------------------
//
//  Microsoft Windows Client Platform
//  Copyright (C) Microsoft Corporation
//
//  File:      TextRunProperties.cs
//
//  Contents:  Text run properties
//
//  Spec:      http://team/sites/Avalon/Specs/Text%20Formatting%20API.doc
//
//  Created:   1-7-2005 Worachai Chaoweeraprasit (wchao)
//
//------------------------------------------------------------------------


using System;
using System.Globalization;
using System.Windows;

namespace System.Windows.Media.TextFormatting
{
	/// <summary>
	/// Properties that can change from one run to the next, such as typeface or foreground brush.
	/// </summary>
	/// <remarks>
	/// The client provides a concrete implementation of this abstract run properties class. This
	/// allows client to implement their run properties the way that fits with their run formatting
	/// store.
	/// </remarks>
	public abstract class TextRunProperties
	{
	}
}