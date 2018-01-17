using System;

namespace Microsoft.VisualStudio.Commanding
{
    public struct CommandState
    {
        /// <summary>
        /// If true, the command state is unspecified and should not be taken into account.
        /// </summary>
        public bool IsUnspecified { get; }

        /// <summary>
        /// If true, the command should be visible and enabled in the UI.
        /// </summary>
        public bool IsAvailable { get; }

        /// <summary>
        /// If true, the command should appear as checked (i.e. toggled) in the UI.
        /// </summary>
        public bool IsChecked { get; }

        /// <summary>
        /// If specified, returns the custom text that should be displayed in the UI.
        /// </summary>
        public string DisplayText { get; }

        public CommandState(bool isAvailable = false, bool isChecked = false, string displayText = null, bool isUnspecified = false)
        {
            if (isUnspecified && (isAvailable || isChecked || displayText != null))
            {
                throw new ArgumentException("Unspecified command state cannot be combined with other states or command text.");
            }

            this.IsAvailable = isAvailable;
            this.IsChecked = isChecked;
            this.IsUnspecified = isUnspecified;
            this.DisplayText = displayText;
        }

        /// <summary>
        /// A helper singleton representing an available command state.
        /// </summary>
        public static CommandState Available { get; } = new CommandState(isAvailable: true);

        /// <summary>
        /// A helper singleton representing an unavailable command state.
        /// </summary>
        public static CommandState Unavailable { get; } = new CommandState(isAvailable: false);

        /// <summary>
        /// A helper singleton representing an unspecified command state.
        /// </summary>
        public static CommandState Unspecified { get; } = new CommandState(isUnspecified: true);
    }
}
