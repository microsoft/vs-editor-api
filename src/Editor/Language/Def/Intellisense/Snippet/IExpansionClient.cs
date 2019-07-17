//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Xml.Linq;

namespace Microsoft.VisualStudio.Text.Editor.Expansion
{
    public interface IExpansionClient
    {
        IExpansionFunction GetExpansionFunction(XElement xmlFunctionNode, string fieldName);
        void FormatSpan(SnapshotSpan span);

        void EndExpansion();

        void OnBeforeInsertion(IExpansionSession session);
        void OnAfterInsertion(IExpansionSession session);

        void PositionCaretForEditing(ITextView textView, SnapshotPoint point);
        void OnItemChosen(string tittle, string path);
    }
}
