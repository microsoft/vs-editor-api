//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Xml.Linq;

namespace Microsoft.VisualStudio.Text.Editor.Expansion
{
    public interface IExpansionSession
    {
        void EndCurrentExpansion(bool leaveCaret);
        bool GoToNextExpansionField(bool commitIfLast);
        bool GoToPreviousExpansionField();
        string GetFieldValue(string fieldName);
        void SetFieldDefault(string fieldName, string newValue);
        SnapshotSpan GetFieldSpan(string fieldName);
        XElement GetHeaderNode();
        XElement GetDeclarationNode();
        XElement GetSnippetNode();
        SnapshotSpan GetSnippetSpan();
        SnapshotSpan EndSpan { get; set; }
    }
}
