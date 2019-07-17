//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.VisualStudio.Text.Editor.Expansion
{
    public class ExpansionField
    {
        public string ID { get; set; }
        public string ToolTip { get; set; }
        public string Default { get; set; }
        public string Function { get; set; }

        private List<int> offsets = new List<int>();

        public int GetOffsetCount()
        {
            return offsets.Count;
        }

        public void AddOffset(int offset)
        {
            offsets.Add(offset);
        }

        public int GetOffset(int index)
        {
            return offsets[index];
        }

        public string GetDefault()
        {
            return Default;
        }

        public int GetLength()
        {
            return Default.Length;
        }

        public bool IsEditable()
        {
            return Editable;
        }

        public XElement GetFunctionXML()
        {
            return new XElement(XName.Get("ExpansionField"))
            {
                Value = Function
            };
        }

        public bool UsesFunction()
        {
            return Function != null;
        }

        public bool IsDefault { get; set; }
        public bool Editable { get; set; }

        public string GetName()
        {
            return ID;
        }
    }
}
