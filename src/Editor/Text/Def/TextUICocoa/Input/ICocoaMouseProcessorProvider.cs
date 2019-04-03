//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Creates an <see cref="ICocoaMouseProcessor"/> for a <see cref="ICocoaTextView"/>.
    /// </summary>
    /// <remarks>This is a MEF component part, and should be exported with the following attribute:
    /// [Export(typeof(IMouseProcessorProvider))]
    /// Exporters must supply a NameAttribute, a ContentTypeAttribute, at least one TextViewRoleAttribute, and optionally an OrderAttribute.
    /// </remarks>
    public interface ICocoaMouseProcessorProvider
    {
        /// <summary>
        /// Creates an <see cref="ICocoaMouseProcessor"/> for a <see cref="ICocoaTextView"/>.
        /// </summary>
        /// <param name="textView">The <see cref="ICocoaTextView"/> for which to create the <see cref="ICocoaMouseProcessor"/>.</param>
        /// <returns>The created <see cref="ICocoaMouseProcessor"/>.
        /// The value may be null if this <see cref="ICocoaMouseProcessorProvider"/> does not wish to participate in the current context.</returns>
        ICocoaMouseProcessor GetAssociatedProcessor(ICocoaTextView textView);
    }
}