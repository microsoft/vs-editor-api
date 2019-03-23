using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Utilities;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Implementation
{
    internal static class CompletionUtilities
    {
        /// <summary>
        /// Maps given point to buffers that contain this point. Requires UI thread.
        /// </summary>
        /// <param name="textView"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        internal static IEnumerable<ITextBuffer> GetBuffersForPoint(ITextView textView, SnapshotPoint point)
        {
            // We are looking at the buffer to the left of the caret.
            return textView.BufferGraph.GetTextBuffers(n =>
                textView.BufferGraph.MapDownToBuffer(point, PointTrackingMode.Negative, n, PositionAffinity.Predecessor) != null);
        }

        /// <summary>
        /// Returns whether the <see cref="ITextView"/> is furnished by the debugger,
        /// e.g. it is a view in the breakpoint settings window or watch window.
        /// We use this to pick an appropriate option with suggestion mode setting.
        /// </summary>
        /// <param name="textView">View to examine</param>
        /// <returns>True if the view has "DEBUGVIEW" text view role.</returns>
        internal static bool IsDebuggerTextView(ITextView textView) => textView.Roles.Contains("DEBUGVIEW");

        /// <summary>
        /// Returns whether the <see cref="ITextView"/> is in immediate window.
        /// We use this to make the view temporarily writable during commit (it is typically read-only).
        /// </summary>
        /// <param name="textView">View to examine</param>
        /// <returns>True if the view has "COMMANDVIEW" text view role.</returns>
        internal static bool IsImmediateTextView(ITextView textView) => textView.Roles.Contains("COMMANDVIEW");

        static readonly EditorOptionKey<bool> NonBlockingCompletionOptionKey = new EditorOptionKey<bool>(PredefinedCompletionNames.NonBlockingCompletionOptionName);
        static readonly EditorOptionKey<bool> SuggestionModeOptionKey = new EditorOptionKey<bool>(PredefinedCompletionNames.SuggestionModeInCompletionOptionName);
        static readonly EditorOptionKey<bool> SuggestionModeInDebuggerCompletionOptionKey = new EditorOptionKey<bool>(PredefinedCompletionNames.SuggestionModeInDebuggerCompletionOptionName);
        private const bool NonBlockingCompletionDefaultValue = false;
        private const bool UseSuggestionModeDefaultValue = false;
        private const bool UseSuggestionModeInDebuggerCompletionDefaultValue = true;

        [Export(typeof(EditorOptionDefinition))]
        [Name(PredefinedCompletionNames.SuggestionModeInCompletionOptionName)]
        class SuggestionModeOptionDefinition : EditorOptionDefinition
        {
            public override object DefaultValue => UseSuggestionModeDefaultValue;

            public override Type ValueType => typeof(bool);

            public override string Name => PredefinedCompletionNames.SuggestionModeInCompletionOptionName;
        }

        [Export(typeof(EditorOptionDefinition))]
        [Name(PredefinedCompletionNames.SuggestionModeInDebuggerCompletionOptionName)]
        class SuggestionModeInDebuggerCompletionOptionDefinition : EditorOptionDefinition
        {
            public override object DefaultValue => UseSuggestionModeInDebuggerCompletionDefaultValue;

            public override Type ValueType => typeof(bool);

            public override string Name => PredefinedCompletionNames.SuggestionModeInDebuggerCompletionOptionName;
        }

        [Export(typeof(EditorOptionDefinition))]
        [Name(PredefinedCompletionNames.NonBlockingCompletionOptionName)]
        class NonBlockingCompletionOptionDefinition : EditorOptionDefinition
        {
            public override object DefaultValue => NonBlockingCompletionDefaultValue;

            public override Type ValueType => typeof(bool);

            public override string Name => PredefinedCompletionNames.NonBlockingCompletionOptionName;
        }

        internal static bool GetSuggestionModeOption(ITextView textView)
        {
            var options = textView.Options.GlobalOptions;
            var useDebuggerViewOption = IsDebuggerTextView(textView) || IsImmediateTextView(textView);
            var optionKey = useDebuggerViewOption ? SuggestionModeInDebuggerCompletionOptionKey : SuggestionModeOptionKey;

            if (!(options.IsOptionDefined(optionKey, localScopeOnly: false)))
            {
                var defaultValue = useDebuggerViewOption ? UseSuggestionModeInDebuggerCompletionDefaultValue : UseSuggestionModeDefaultValue;
                options.SetOptionValue(optionKey, defaultValue);
            }
            return options.GetOptionValue(optionKey);
        }

        internal static void SetSuggestionModeOption(ITextView textView, bool value)
        {
            var options = textView.Options.GlobalOptions;
            var useDebuggerViewOption = IsDebuggerTextView(textView) || IsImmediateTextView(textView);
            var optionKey = useDebuggerViewOption ? SuggestionModeInDebuggerCompletionOptionKey : SuggestionModeOptionKey;
            options.SetOptionValue(optionKey, value);
        }

        internal static bool GetNonBlockingCompletionOption(ITextView textView)
        {
            var options = textView.Options.GlobalOptions;
            if (!(options.IsOptionDefined(NonBlockingCompletionOptionKey, localScopeOnly: false)))
            {
                options.SetOptionValue(NonBlockingCompletionOptionKey, NonBlockingCompletionDefaultValue);
            }
            return options.GetOptionValue(NonBlockingCompletionOptionKey);
        }

        internal static void SetNonBlockingModeOption(ITextView textView, bool value)
        {
            var options = textView.Options.GlobalOptions;
            options.SetOptionValue(NonBlockingCompletionOptionKey, value);
        }
    }
}
