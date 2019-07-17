//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.Text.Editor.Expansion
{
    public class ExpansionTemplate
    {
        private string snippetFullText;
        private readonly List<ExpansionField> tokens = new List<ExpansionField>();
        private int selectedOffset;
        private int editOffset;
        private string snippetFirst;
        private string snippetLast;

        public CodeSnippet Snippet { get; set; }

        public ExpansionTemplate(string filePath)
            :this(CodeSnippet.ReadSnippetsFromFile(filePath).First())
        {
            
        }

        public ExpansionTemplate(CodeSnippet snippet)
        {
            Snippet = snippet;
            Parse();
        }

        private void Parse()
        {
            foreach (var field in Snippet.Fields)
            {
                tokens.Add(field);
            }

            var matches = Regex.Matches(Snippet.Code, @"\$(\w+)\$");

            var total = new StringBuilder();
            int index = 0;
            int indexCorrection = 0;
            foreach (Match match in matches)
            {
                string fieldName = match.Groups[1].Value;
                string substitution = GetDefaultValue(fieldName) ?? "";
                if (match.Index > index)
                {
                    total.Append(Snippet.Code.Substring(index, match.Index - index));
                }

                AddToken(total.Length, fieldName);
                total.Append(substitution);

                index = match.Index + match.Length;
                indexCorrection += substitution.Length;
            }

            if (index <= Snippet.Code.Length - 1)
            {
                total.Append(Snippet.Code.Substring(index, Snippet.Code.Length - index));
            }

            this.snippetFullText = total.ToString();
            this.snippetFirst = snippetFullText.Substring(0, selectedOffset);
            this.snippetLast = snippetFullText.Substring(selectedOffset, snippetFullText.Length - selectedOffset);
        }

        private string GetDefaultValue(string fieldName)
        {
            ExpansionField field = FindField(fieldName);
            if (field == null)
            {
                return null;
            }

            return field.Default;
        }

        private void AddToken(int position, string fieldName)
        {
            ExpansionField field = FindField(fieldName);
            if (field != null)
            {
                field.AddOffset(position);
            }
            else if (fieldName == "selected")
            {
                this.selectedOffset = position;
            }
            else if (fieldName == "end")
            {
                this.editOffset = position;
            }
        }

        private ExpansionField FindField(string fieldName)
        {
            return tokens.FirstOrDefault(t => t.ID == fieldName);
        }

        public string GetCodeSnippet()
        {
            return snippetFullText;
        }

        public int GetSelectedOffset()
        {
            return selectedOffset;
        }

        public int GetEditOffset()
        {
            return editOffset;
        }

        public string GetCodeSnippet(bool all, bool first)
        {
            if (all)
            {
                return snippetFullText;
            }
            else if (first)
            {
                return snippetFirst;
            }
            else
            {
                return snippetLast;
            }
        }

        public IEnumerable<ExpansionField> Fields
        {
            get
            {
                return tokens;
            }
        }
    }
}
