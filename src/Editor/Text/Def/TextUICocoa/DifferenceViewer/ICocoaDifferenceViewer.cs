//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System.Windows;
using AppKit;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Text.Differencing
{
    /// <summary>
    /// A Cocoa-specific version of an <see cref="IDifferenceViewer"/>, which provides access to the
    /// <see cref="VisualElement" /> used to host the viewer and the various text view hosts as <see cref="ICocoaTextViewHost" />.
    /// </summary>
    public interface ICocoaDifferenceViewer : IDifferenceViewer
    {
        /// <summary>
        /// Initialize the DifferenceViewer, hooking it to the specified buffer and using the callback to create the text view hosts.
        /// </summary>
        /// <param name="differenceBuffer"></param>
        /// <param name="createTextViewHost"></param>
        /// <param name="parentOptions"></param>
        /// <remarks>
        /// <para>This method should only be called if the CreateUninitializedDifferenceView method on the <see cref="ICocoaDifferenceViewerFactoryService"/> is used. Otherwise, it is
        /// called by the factory.</para>
        /// <para>The viewer does not have to be initialized immediately. You can wait until the Loaded event on the VisualElement.</para>
        /// </remarks>
        void Initialize(IDifferenceBuffer differenceBuffer,
                        CreateTextViewHostCallback createTextViewHost,
                        IEditorOptions parentOptions = null);

        /// <summary>
        /// Has this viewer been initialized?
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// The view for displaying <see cref="DifferenceViewMode.Inline"/> differences.
        /// </summary>
        /// <remarks>Will never be <c>null</c>, but will only be visible when <see cref="IDifferenceViewer.ViewMode"/>
        /// is set to <see cref="DifferenceViewMode.Inline"/>.</remarks>
        new ICocoaTextView InlineView { get; }

        /// <summary>
        /// The view for displaying the left buffer for <see cref="DifferenceViewMode.SideBySide"/> differences.
        /// </summary>
        /// <remarks>Will never be <c>null</c>, but will only be visible when <see cref="IDifferenceViewer.ViewMode"/>
        /// is set to <see cref="DifferenceViewMode.SideBySide"/>.</remarks>
        new ICocoaTextView LeftView { get; }

        /// <summary>
        /// The view for displaying the right buffer for <see cref="DifferenceViewMode.SideBySide"/> differences.
        /// </summary>
        /// <remarks>Will never be <c>null</c>, but will only be visible when <see cref="IDifferenceViewer.ViewMode"/>
        /// is set to <see cref="DifferenceViewMode.SideBySide"/>.</remarks>
        new ICocoaTextView RightView { get; }

        /// <summary>
        /// The host for displaying <see cref="DifferenceViewMode.Inline"/> differences.
        /// </summary>
        /// <remarks>Will never be <c>null</c>, but will only be visible when <see cref="IDifferenceViewer.ViewMode"/>
        /// is set to <see cref="DifferenceViewMode.Inline"/>.</remarks>
        ICocoaTextViewHost InlineHost { get; }

        /// <summary>
        /// The host for displaying the left buffer for <see cref="DifferenceViewMode.SideBySide"/> differences.
        /// </summary>
        /// <remarks>Will never be <c>null</c>, but will only be visible when <see cref="IDifferenceViewer.ViewMode"/>
        /// is set to <see cref="DifferenceViewMode.SideBySide"/>.</remarks>
        ICocoaTextViewHost LeftHost { get; }

        /// <summary>
        /// The host for displaying the right buffer for <see cref="DifferenceViewMode.SideBySide"/> differences.
        /// </summary>
        /// <remarks>Will never be <c>null</c>, but will only be visible when <see cref="IDifferenceViewer.ViewMode"/>
        /// is set to <see cref="DifferenceViewMode.SideBySide"/>.</remarks>
        ICocoaTextViewHost RightHost { get; }

        /// <summary>
        /// The visual element of this viewer.
        /// </summary>
        NSView VisualElement { get; }
    }
}
