//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Text.Editor.Expansion
{
	public interface IExpansionManager
	{
        IEnumerable<ExpansionTemplate> EnumerateExpansions(IContentType contentType, bool shortcutOnly, string[] snippetTypes, bool includeNullType, bool includeDuplicates);
		ExpansionTemplate GetTemplateByName(IExpansionClient expansionClient, IContentType contentType, string name, string filePath, ITextView textView, SnapshotSpan span, bool showDisambiguationUI);
		ExpansionTemplate GetTemplateByShortcut(IExpansionClient expansionClient, string shortcut, IContentType contentType, ITextView textView, SnapshotSpan span, bool showDisambiguationUI);
	}
}
