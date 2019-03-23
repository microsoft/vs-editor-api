// Copyright (c) Microsoft Corporation
// All rights reserved

using System;

namespace Microsoft.VisualStudio.Language.Intellisense
{
    /// <summary>
    /// Extends <see cref="IQuickInfoSession"/> with support for an interactive Quick Info content. 
    /// </summary>
    [Obsolete("Use " + nameof(IAsyncQuickInfoSession) + " instead")]
    public interface IQuickInfoSession2 : IQuickInfoSession
    {
        /// <summary>
        /// This <see cref="IQuickInfoSession"/> contains an interactive content.
        /// </summary>
        bool HasInteractiveContent { get; }
    }
}
