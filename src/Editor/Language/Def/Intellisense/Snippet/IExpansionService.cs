//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Threading;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Text.Editor.Expansion
{
    public interface IExpansionService
    {
        IExpansionSession InsertExpansion(SnapshotSpan triggerSpan, IExpansionClient expansionClient, IContentType contentType, CancellationToken cancellationToken = default);
        IExpansionSession InsertNamedExpansion(string title, string pszPath, SnapshotSpan triggerSpan, IExpansionClient expansionClient, IContentType contentType, bool showDisambiguationUI);
        void InvokeInsertionUI(IExpansionClient expansionClient, IContentType contentType, string[] types, bool includeNullType, string[] kinds, bool includeNullKind, string prefixText, string completionChar);
    }
}
