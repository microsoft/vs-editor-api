//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Creates an <see cref="ICocoaTextViewMargin"/> for a given <see cref="ICocoaTextViewHost"/>.  
    /// </summary>
    /// <remarks><para>This is a MEF component part, and should be exported with the following attribute:
    /// <code>[Export(typeof(ICocoaTextViewMarginProvider))]</code>
    /// Exporters must supply an MarginContainerAttribute, TextViewRoleAttribute,
    /// and NameAttribute.</para>
    /// <para>Exporters should provide one or more OrderAttributes, ContentTypeAttributes, and/or TextViewRoleAttributes.
    /// </para>
    /// <para>Exporters may provide zero or more ReplacesAttributes.</para>
    /// </remarks>
    public interface ICocoaTextViewMarginProvider
    {
        /// <summary>
        /// Creates an <see cref="ICocoaTextViewMargin"/> for the given <see cref="ICocoaTextViewHost"/>.
        /// </summary>d\
        /// <param name="textViewHost">The <see cref="ICocoaTextViewHost"/> for which to create the <see cref="ICocoaTextViewMargin"/>.</param>
        /// <param name="marginContainer">The margin that will contain the newly-created margin.</param>
        /// <returns>The <see cref="ICocoaTextViewMargin"/>.  
        /// The value may be null if this <see cref="ICocoaTextViewMarginProvider"/> does not participate for this context.</returns>
        ICocoaTextViewMargin CreateMargin(ICocoaTextViewHost textViewHost, ICocoaTextViewMargin marginContainer);
    }
}