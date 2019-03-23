//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
// This file contain internal APIs that are subject to change without notice.
// Use at your own risk.
//

using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Text.Editor
{
    public interface ITextViewFactoryService
    {
        /// <summary>
        /// Creates an <see cref="ITextView"/> that displays the contents of <paramref name="viewModel"/>.
        /// </summary>
        /// <param name="viewModel">The <see cref="ITextViewModel"/> that provides the text buffers for the view.</param>
        /// <param name="roles">The set of roles filled by the view.</param>
        /// <param name="parentOptions">The options environment for the text view.</param>
        /// <returns>An <see cref="ITextView"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewModel"/> or <paramref name="roles"/> or <paramref name="parentOptions"/> is null.</exception>
        ITextView CreateTextView(ITextViewModel viewModel, ITextViewRoleSet roles, IEditorOptions parentOptions);

        /// <summary>
        /// Creates an <see cref="ITextView"/> that displays the contents of <paramref name="dataModel"/>.
        /// </summary>
        /// <param name="dataModel">The <see cref="ITextDataModel"/> that provides the text buffers over which an <see cref="ITextViewModel"/>
        /// will be built for the view.</param>
        /// <param name="roles">The set of roles filled by the view.</param>
        /// <param name="parentOptions">The options environment for the text view.</param>
        /// <returns>An <see cref="ITextView"/>.</returns>
        /// <remarks>
        /// An <see cref="ITextDataModel"/> can be displayed in multiple views. An <see cref="ITextViewModel"/> will be constructed based on 
        /// the <see cref="IContentType"/> of the <paramref name="dataModel"/> and the <paramref name="roles"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="dataModel"/> or <paramref name="roles"/> or <paramref name="parentOptions"/> is null.</exception>
        ITextView CreateTextView(ITextDataModel dataModel, ITextViewRoleSet roles, IEditorOptions parentOptions);

        /// <summary>
        /// Creates an <see cref="ITextView"/> that displays the contents of <paramref name="textBuffer"/>.
        /// </summary>
        /// <param name="textBuffer">The <see cref="ITextBuffer"/> that provides the text for the view.</param>
        /// <param name="roles">The set of roles filled by the view.</param>
        /// <param name="parentOptions">The options environment for the text view.</param>
        /// <returns>An <see cref="ITextView"/>.</returns>
        /// <remarks>
        /// An <see cref="ITextBuffer"/> can be displayed in multiple views. A trivial <see cref="ITextDataModel"/> will be constructed and
        /// an <see cref="ITextViewModel"/> will be constructed based on 
        /// the <see cref="IContentType"/> of the <paramref name="textBuffer"/> and the <paramref name="roles"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="textBuffer"/> or <paramref name="roles"/> or <paramref name="parentOptions"/> is null.</exception>
        ITextView CreateTextView(ITextBuffer textBuffer, ITextViewRoleSet roles, IEditorOptions parentOptions);

        /// <summary>
        /// Creates an <see cref="ITextView"/> that displays the contents of <paramref name="textBuffer"/>.
        /// </summary>
        /// <param name="textBuffer">The <see cref="ITextBuffer"/> that provides the text for the view.</param>
        /// <param name="roles">The set of roles filled by the view.</param>
        /// <returns>An <see cref="ITextView"/>.</returns>
        /// <remarks>
        /// An <see cref="ITextBuffer"/> can be displayed in multiple views. A trivial <see cref="ITextDataModel"/> will be constructed and
        /// an <see cref="ITextViewModel"/> will be constructed based on 
        /// the <see cref="IContentType"/> of the <paramref name="textBuffer"/> and the <paramref name="roles"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="textBuffer"/> or <paramref name="roles"/> is null.</exception>
        ITextView CreateTextView(ITextBuffer textBuffer, ITextViewRoleSet roles);

        /// <summary>
        /// Creates an <see cref="ITextView"/> that displays the contents of <paramref name="textBuffer"/>.
        /// </summary>
        /// <param name="textBuffer">The <see cref="ITextBuffer"/> that provides the text for the view.</param>
        /// <returns>An <see cref="ITextView"/> having the default set of text view roles.</returns>
        /// <remarks>
        /// An <see cref="ITextBuffer"/> can be displayed in multiple views. A trivial <see cref="ITextDataModel"/> will be constructed and
        /// an <see cref="ITextViewModel"/> will be constructed based on 
        /// the <see cref="IContentType"/> of the <paramref name="textBuffer"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="textBuffer"/> is null.</exception>
        ITextView CreateTextView(ITextBuffer textBuffer);

        /// <summary>
        /// Creates an <see cref="ITextView"/> on a newly created <see cref="ITextBuffer"/> having
        /// content type <code>Text</code>.
        /// </summary>
        /// <returns>A <see cref="ITextView"/>.</returns>
        ITextView CreateTextView();

        ///// <summary>
        ///// Creates a host for the text view.
        ///// </summary>
        ///// <param name="wpfTextView">The text view to host.</param>
        ///// <param name="setFocus"><c>true</c> if the <see cref="IWpfTextViewHost"/> should take focus after it is initialized, <c>false</c> otherwise.</param>
        ///// <returns>An <see cref="IWpfTextViewHost"/>.</returns>
        ///// <exception cref="ArgumentNullException"><paramref name="wpfTextView"/> is null.</exception>
        //IWpfTextViewHost CreateTextViewHost(ITextView wpfTextView, bool setFocus);

        /// <summary>
        /// The empty ITextViewRoleSet.
        /// </summary>
        ITextViewRoleSet NoRoles { get; }

        /// <summary>
        /// The set of all predefined text view roles.
        /// </summary>
        ITextViewRoleSet AllPredefinedRoles { get; }

        /// <summary>
        /// The set of roles that are used when creating a text view without specifying text view roles.
        /// </summary>
        ITextViewRoleSet DefaultRoles { get; }

        /// <summary>
        /// Creates a <see cref="ITextViewRoleSet"/> containing the given roles.
        /// </summary>
        /// <param name="roles">The roles of interest.</param>
        /// <returns>The text view role set.</returns>
        /// <exception cref="ArgumentNullException"> roles is null.</exception>
        ITextViewRoleSet CreateTextViewRoleSet(IEnumerable<string> roles);

        /// <summary>
        /// Creates a <see cref="ITextViewRoleSet"/> containing the given roles.
        /// </summary>
        /// <param name="roles">The roles of interest.</param>
        /// <returns>The text view role set.</returns>
        ITextViewRoleSet CreateTextViewRoleSet(params string[] roles);

        /// <summary>
        /// Raised when any <see cref="ITextView"/> is created.
        /// </summary>
        event EventHandler<TextViewCreatedEventArgs> TextViewCreated;

        /// <summary>
        /// Creates an <see cref="ITextView"/> that displays the contents of <paramref name="dataModel"/> without initializing it.
        /// </summary>
        /// <param name="dataModel">The <see cref="ITextDataModel"/> that provides the text buffers over which an <see cref="ITextViewModel"/>
        /// will be built for the view.</param>
        /// <param name="roles">The set of roles filled by the view.</param>
        /// <param name="parentOptions">The options environment for the text view.</param>
        /// <returns>An <see cref="ITextView"/>.</returns>
        /// <remarks>
        /// <para>An <see cref="ITextDataModel"/> can be displayed in multiple views. An <see cref="ITextViewModel"/> will be constructed based on 
        /// the <see cref="IContentType"/> of the <paramref name="dataModel"/> and the <paramref name="roles"/>.</para>
        /// <para>ITextEditorFactoryService2.InitializeTextView(view) must be called on the view returned by this funtion.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="dataModel"/> or <paramref name="roles"/> or <paramref name="parentOptions"/> is null.</exception>
        ITextView CreateTextViewWithoutInitialization(ITextDataModel dataModel, ITextViewRoleSet roles, IEditorOptions parentOptions);

        ///// <summary>
        ///// Creates a host for the text view without initializing it.
        ///// </summary>
        ///// <param name="wpfTextView">The text view to host.</param>
        ///// <param name="setFocus"><c>true</c> if the <see cref="IWpfTextViewHost"/> should take focus after it is initialized, <c>false</c> otherwise.</param>
        ///// <returns>An <see cref="IWpfTextViewHost"/>.</returns>
        ///// <remarks>
        ///// <para>ITextEditorFactoryService2.InitializeTextViewHost(host) must be called on the host returned by this funtion.</para>
        ///// </remarks>
        ///// <exception cref="ArgumentNullException"><paramref name="wpfTextView"/> is null.</exception>
        //IWpfTextViewHost CreateTextViewHostWithoutInitialization(IWpfTextView wpfTextView, bool setFocus);

        /// <summary>
        /// Initialize a view created using CreateTextViewWithoutInitialization.
        /// </summary>
        void InitializeTextView(ITextView view);

        ///// <summary>
        ///// Initialize a view created using CreateTextViewHostWithoutInitialization.
        ///// </summary>
        //void InitializeTextViewHost(IWpfTextViewHost host);
    }
}
