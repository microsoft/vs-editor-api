using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Implementation
{
    [Export(typeof(EditorOptionDefinition))]
    [Name(OptionName)]
    internal class UseAsyncCompletionOptionDefinition : EditorOptionDefinition
    {
        public const string OptionName = "UseAsyncCompletion";

        /// <summary>
        /// The meaning of this option definition's values:
        /// -1 - user disabled async completion
        ///  0 - no changes from the user; check the experimentation service for whether to use async completion
        ///  1 - user enabled async completion
        /// </summary>
        public override object DefaultValue => 0;

        public override Type ValueType => typeof(int);

        public override string Name => OptionName;
    }
}
