//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Windows;

using AppKit;

namespace Microsoft.VisualStudio.Text.Classification
{
    using static ClassificationFormatDefinition;

    public static class ClassificationExtensions
    {
        internal const string FontRenderingScaleId = "FontRenderingScale";

        public static void UpdateFont(
            this ResourceDictionary resourceDictionary,
            NSFont font)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            resourceDictionary[TypefaceId] = font.FontDescriptor.FontDescriptorWithSize(0);
            resourceDictionary[FontRenderingSizeId] = (double)font.PointSize;
        }

        public static NSFont GetFont(
            this ResourceDictionary resourceDictionary,
            bool respectScale = true)
        {
            NSFontDescriptor fontDescriptor = null;
            nfloat fontSize = 0;

            switch (resourceDictionary[TypefaceId])
            {
                case NSFontDescriptor fd:
                    fontDescriptor = fd;
                    break;
                case NSFont rdFont:
                    fontDescriptor = rdFont.FontDescriptor;
                    fontSize = rdFont.PointSize;
                    break;
                case string fontName:
                    fontDescriptor = NSFontWorkarounds
                        .FromFontName(fontName, 0)
                        .FontDescriptor;
                    break;
            }

            switch (resourceDictionary[FontRenderingSizeId])
            {
                case double d:
                    fontSize = (nfloat)d;
                    break;
                case float f:
                    fontSize = f;
                    break;
                case nfloat nf:
                    fontSize = nf;
                    break;
                case int i:
                    fontSize = i;
                    break;
            }

            if (fontSize <= 0)
                fontSize = NSFont.SystemFontSize;

            if (respectScale && resourceDictionary[FontRenderingScaleId] is double scale)
                fontSize *= (nfloat)scale;

            var font = fontDescriptor == null
                ? NSFontWorkarounds.UserFixedPitchFontOfSize(fontSize)
                : NSFontWorkarounds.FromDescriptor(fontDescriptor, fontSize);

            NSFontTraitMask convertFontTraits = 0;

            if (resourceDictionary.Contains(IsBoldId) &&
                resourceDictionary[IsBoldId] is bool isBold &&
                isBold)
                convertFontTraits |= NSFontTraitMask.Bold;

            if (resourceDictionary.Contains(IsItalicId) &&
                resourceDictionary[IsItalicId] is bool isItalic &&
                isItalic)
                convertFontTraits |= NSFontTraitMask.Italic;

            if (convertFontTraits != 0)
                font = NSFontManager.SharedFontManager.ConvertFontWorkaround(
                    font,
                    convertFontTraits);

            return font;
        }
    }
}