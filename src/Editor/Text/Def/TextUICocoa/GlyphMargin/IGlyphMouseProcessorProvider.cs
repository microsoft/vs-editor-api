//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Provides a mouse binding for the glyph margin.
    /// </summary>
    /// <remarks>This is a MEF component part, and should be exported with the following attribute:
    /// [Export(typeof(IGlyphMouseProcessorProvider))]
    /// Exporters must supply a NameAttribute, OrderAttribute, 
    /// and at least one ContentTypeAttribute.
    /// </remarks>
    public interface IGlyphMouseProcessorProvider
    {
        /// <summary>
        /// Creates an <see cref="ICocoaMouseProcessor"/> for the glyph margin, given a <see cref="ICocoaTextViewHost"/> and a <see cref="ITextBuffer"/>.
        /// </summary>
        /// <param name="textViewHost">The <see cref="ICocoaTextViewHost"/> associated with the glyph margin.</param>
        /// <param name="margin">The <see cref="ICocoaTextViewMargin"/>.</param>
        /// <returns>The <see cref="ICocoaMouseProcessor"/> for the glyph margin.  
        /// The value may be null if this <see cref="IGlyphMouseProcessorProvider"/> does not participate.</returns>
        ICocoaMouseProcessor GetAssociatedMouseProcessor(ICocoaTextViewHost textViewHost, ICocoaTextViewMargin margin);
    }
}