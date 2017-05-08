using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Provides information for the <see cref="IEditorOptions.OptionChanged"/> event.
    /// </summary>
    public class EditorOptionChangedEventArgs : EventArgs
    {
        private string _optionId;

        /// <summary>
        /// Initializes a new instance of <see cref="EditorOptionChangedEventArgs"/>.
        /// </summary>
        /// <param name="optionId">The ID of the option.</param>
        public EditorOptionChangedEventArgs(string optionId)
        {
            _optionId = optionId;
        }

        /// <summary>
        /// Gets the ID of the option that has changed.
        /// </summary>
        public string OptionId { get { return _optionId; } }
    }
}
