//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Provides an <see cref="IGlyphFactory"/>.
    /// </summary>
    /// <remarks>This is a MEF component part, and should be exported with the following attribute:
    /// [Export(typeof(IGlyphFactoryProvider))]
    /// Exporters must supply a NameAttribute, OrderAttribute, 
    /// at least one ContentTypeAttribute, and at least one TagTypeAttribute.
    /// </remarks>
    public interface IGlyphFactoryProvider
    {
        /// <summary>
        /// Gets the <see cref="IGlyphFactory"/> for the given text view and margin.
        /// </summary>
        /// <param name="textView">The view for which the factory is being created.</param>
        /// <param name="margin">The margin for which the factory will create glyphs.</param>
        /// <returns>An <see cref="IGlyphFactory"/> for the given view and margin.</returns>
        IGlyphFactory GetGlyphFactory(ICocoaTextView textView, ICocoaTextViewMargin margin);
    }
}