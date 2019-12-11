//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.VisualStudio.Text.Editor.Expansion
{
    public class CodeSnippet
    {
        public string Title { get; set; }
        public string Shortcut { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public List<string> SnippetTypes { get; set; }
        public string Code { get; set; }
        public List<ExpansionField> Fields { get; set; }
        public string FilePath { get; set; }
        public string Language { get; set; }
        public XElement Snippet { get; set; }

        public CodeSnippet()
        {

        }

        public CodeSnippet(XElement codeSnippetElement, string filePath)
        {
            this.FilePath = filePath;
            this.Fields = new List<ExpansionField>();
            this.SnippetTypes = new List<string>();

            var header = codeSnippetElement.Element(GetXName("Header"));
            this.Title = GetElementInnerText(header, "Title");
            this.Shortcut = GetElementInnerText(header, "Shortcut") ?? ""; // https://github.com/dotnet/roslyn/pull/31738
            this.Description = GetElementInnerText(header, "Description");
            this.Author = GetElementInnerText(header, "Author");
            var snippetTypes = header.Element(GetXName("SnippetTypes"));
            if (snippetTypes != null)
            {
                var snippetTypeElements = snippetTypes.Elements();
                foreach (var snippetType in snippetTypeElements)
                {
                    SnippetTypes.Add(snippetType.Value);
                }
            }

            Snippet = codeSnippetElement.Element(GetXName("Snippet"));
            var declarations = Snippet.Element(GetXName("Declarations"));
            ReadDeclarations(declarations);
            var code = Snippet.Element(GetXName("Code"));
            this.Code = code.Value.Replace("\n", "\r\n");
            this.Language = code.Attribute("Language").Value;
        }

        private void ReadDeclarations(XElement declarations)
        {
            if (declarations == null)
            {
                return;
            }

            foreach (var declarationElement in declarations.Elements())
            {
                var defaultAttribute = declarationElement.Attribute("Default");
                var editableAttribute = declarationElement.Attribute("Editable");
                this.Fields.Add(new ExpansionField
                {
                    ID = GetElementInnerText(declarationElement, "ID"),
                    ToolTip = GetElementInnerText(declarationElement, "ToolTip"),
                    Default = GetElementInnerText(declarationElement, "Default") ?? " ",
                    Function = GetElementInnerText(declarationElement, "Function"),
                    IsDefault = defaultAttribute != null && defaultAttribute.Value == "true",
                    Editable = editableAttribute == null || editableAttribute.Value == "true" || editableAttribute.Value == "1"
                });
            }
        }

        private static string GetElementInnerText(XElement element, string subElementName)
        {
            var subElement = element.Element(GetXName(subElementName));
            if (subElement == null)
            {
                return null;
            }

            return subElement.Value;
        }

        public static IEnumerable<CodeSnippet> ReadSnippetsFromFile(string filePath)
        {
            try
            {
                XDocument document = XDocument.Load(filePath);
                var codeSnippetsElement = document.Root;
                IEnumerable<XElement> codeSnippetElements = null;
                if (codeSnippetsElement.Name.LocalName == "CodeSnippets")
                {
                    codeSnippetElements = codeSnippetsElement.Elements(GetXName("CodeSnippet"));
                }
                else
                {
                    codeSnippetElements = new[] { codeSnippetsElement };
                }

                return codeSnippetElements.Select(element => new CodeSnippet(element, filePath));
            }
            catch (XmlException)
            {
                return Enumerable.Empty<CodeSnippet>();
            }
        }

        private static XName GetXName(string localName)
        {
            return XName.Get(localName, "http://schemas.microsoft.com/VisualStudio/2005/CodeSnippet");
        }
    }
}
