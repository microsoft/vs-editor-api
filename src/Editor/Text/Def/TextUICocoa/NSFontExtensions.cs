// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;

using Foundation;
using ObjCRuntime;

namespace AppKit
{
    public static class NSFontWorkarounds
    {
        [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        static extern IntPtr IntPtr_objc_msgSend_IntPtr_nfloat(IntPtr receiver, IntPtr selector, IntPtr arg1, nfloat arg2);

        [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        static extern IntPtr IntPtr_objc_msgSend_IntPtr_nuint_nint_nfloat(IntPtr receiver, IntPtr selector, IntPtr arg1, nuint arg2, nint arg3, nfloat arg4);

        [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        static extern IntPtr IntPtr_objc_msgSend_nfloat(IntPtr receiver, IntPtr selector, nfloat arg1);

        [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        static extern IntPtr IntPtr_objc_msgSend_IntPtr_nuint(IntPtr receiver, IntPtr selector, IntPtr arg1, nuint arg2);

        [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        static extern bool bool_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

        static readonly IntPtr sel_respondsToSelector_ = Selector.GetHandle("respondsToSelector:");

        static bool RespondsToSelector(IntPtr receiver, IntPtr selector)
            => bool_objc_msgSend_IntPtr(receiver, sel_respondsToSelector_, selector);

        static readonly IntPtr classPtrNSFont = Class.GetHandle("NSFont");

        static readonly IntPtr selFontWithName_Size = Selector.GetHandle("fontWithName:size:");

        /// <summary>
        /// Workaround for https://github.com/xamarin/xamarin-macios/pull/5423
        /// </summary>
        public static NSFont FromFontName(string fontName, nfloat fontSize)
        {
            var fontNamePtr = NSString.CreateNative(fontName);

            try
            {
                return WrappedNSFont.Wrap(IntPtr_objc_msgSend_IntPtr_nfloat(
                    classPtrNSFont,
                    selFontWithName_Size,
                    fontNamePtr,
                    fontSize));
            }
            finally
            {
                NSString.ReleaseNative(fontNamePtr);
            }
        }

        static readonly IntPtr selUserFixedPitchFontOfSize = Selector.GetHandle("userFixedPitchFontOfSize:");

        public static NSFont UserFixedPitchFontOfSize(nfloat fontSize)
            => WrappedNSFont.Wrap(IntPtr_objc_msgSend_nfloat(
                classPtrNSFont,
                selUserFixedPitchFontOfSize,
                fontSize));

        static readonly IntPtr selUserFontOfSize = Selector.GetHandle("userFontOfSize:");

        public static NSFont UserFontOfSize(nfloat fontSize)
            => WrappedNSFont.Wrap(IntPtr_objc_msgSend_nfloat(
                classPtrNSFont,
                selUserFontOfSize,
                fontSize));

        static readonly IntPtr sel_systemFontOfSize_ = Selector.GetHandle("systemFontOfSize:");

        public static NSFont SystemFontOfSize(nfloat fontSize)
            => WrappedNSFont.Wrap(IntPtr_objc_msgSend_nfloat(
                classPtrNSFont,
                sel_systemFontOfSize_,
                fontSize));

        static readonly IntPtr sel__lightSystemFontOfSize_ = Selector.GetHandle("_lightSystemFontOfSize:");
        static readonly IntPtr sel_lightSystemFontOfSize_ = Selector.GetHandle("lightSystemFontOfSize:");

        public static NSFont LightSystemFontOfSize(nfloat fontSize)
        {
            IntPtr selectorHandle;

            if (RespondsToSelector(classPtrNSFont, sel__lightSystemFontOfSize_))
                // _lightSystemFontOfSize: is AppKit internal as of 10.14 but is used by Spotlight and Xcode
                selectorHandle = sel__lightSystemFontOfSize_;
            else if (RespondsToSelector(classPtrNSFont, sel_lightSystemFontOfSize_))
                // lightSystemFontOfSize: does not exist, but may in a future macOS... try it next
                selectorHandle = sel_lightSystemFontOfSize_;
            else
                // systemFontOfSize: has always existed, fall back to it
                selectorHandle = sel_systemFontOfSize_;

            return WrappedNSFont.Wrap(IntPtr_objc_msgSend_nfloat(
                classPtrNSFont,
                selectorHandle,
                fontSize));
        }

        static readonly IntPtr selToolTipsFontOfSize = Selector.GetHandle("toolTipsFontOfSize:");

        public static NSFont ToolTipsFontOfSize(nfloat fontSize)
            => WrappedNSFont.Wrap(IntPtr_objc_msgSend_nfloat(
                classPtrNSFont,
                selToolTipsFontOfSize,
                fontSize));

        static readonly IntPtr selFontWithDescriptor_Size = Selector.GetHandle("fontWithDescriptor:size:");

        public static NSFont FromDescriptor(NSFontDescriptor fontDescriptor, nfloat fontSize)
            => WrappedNSFont.Wrap(IntPtr_objc_msgSend_IntPtr_nfloat(
                classPtrNSFont,
                selFontWithDescriptor_Size,
                (fontDescriptor ?? throw new ArgumentNullException(nameof(fontDescriptor))).Handle,
                fontSize));

        static readonly IntPtr selFontWithFamily_traits_weight_size_ = Selector.GetHandle("fontWithFamily:traits:weight:size:");

        public static NSFont FontWithFamilyWorkaround(
            this NSFontManager fontManager,
            string family,
            NSFontTraitMask traits,
            nint weight,
            nfloat size)
        {
            if (family == null)
                throw new ArgumentNullException(nameof(family));

            var familyPtr = NSString.CreateNative(family);
            try
            {
                return WrappedNSFont.Wrap(IntPtr_objc_msgSend_IntPtr_nuint_nint_nfloat(
                    fontManager.Handle,
                    selFontWithFamily_traits_weight_size_,
                    familyPtr,
                    (nuint)(ulong)traits,
                    weight,
                    size));
            } finally
            {
                NSString.ReleaseNative(familyPtr);
            }
        }

        static readonly IntPtr selConvertFont_toHaveTrait_ = Selector.GetHandle("convertFont:toHaveTrait:");

        public static NSFont ConvertFontWorkaround(
            this NSFontManager fontManager,
            NSFont font,
            NSFontTraitMask trait)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            return WrappedNSFont.Wrap(
                IntPtr_objc_msgSend_IntPtr_nuint(
                    fontManager.Handle,
                    selConvertFont_toHaveTrait_,
                    font.Handle,
                    (nuint)(ulong)trait));
        }

        sealed class WrappedNSFont : NSFont
        {
            public static NSFont Wrap(IntPtr handle)
                => handle == IntPtr.Zero ? null : new WrappedNSFont(handle);

            WrappedNSFont(IntPtr handle) : base(handle)
            {
            }
        }
    }
}