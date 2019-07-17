//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.VisualStudio.Text.Editor.Expansion
{
    public enum ExpansionFunctionType
    {
        List,
        Value
    };

    public interface IExpansionFunction
    {
        void GetDefaultValue(out string value, out bool hasDefaultValue);

        void GetCurrentValue(out string value, out bool hasCurrentValue);

        bool FieldChanged(string fieldName);

        ExpansionFunctionType GetFunctionType();

        int GetListCount();

        string GetListText(int index);

        void ReleaseFunction();
    }
}
