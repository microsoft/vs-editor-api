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
        /// </summary>
        /// <param name="textView">View to examine</param>
        /// <returns>True if the view has "DEBUGVIEW" text view role.</returns>
        internal static bool IsDebuggerTextView(ITextView textView) => textView.Roles.Contains("DEBUGVIEW");

        static readonly EditorOptionKey<bool> SuggestionModeOptionKey = new EditorOptionKey<bool>(PredefinedCompletionNames.SuggestionModeInCompletionOptionName);
        static readonly EditorOptionKey<bool> SuggestionModeInDebuggerCompletionOptionKey = new EditorOptionKey<bool>(PredefinedCompletionNames.SuggestionModeInDebuggerCompletionOptionName);
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

        internal static bool GetSuggestionModeOption(ITextView textView)
        {
            var options = textView.Options.GlobalOptions;
            if (!(options.IsOptionDefined(SuggestionModeOptionKey, localScopeOnly: false)))
                options.SetOptionValue(SuggestionModeOptionKey, UseSuggestionModeDefaultValue);
            return options.GetOptionValue(SuggestionModeOptionKey);
        }

        internal static void SetSuggestionModeOption(ITextView textView, bool value)
        {
            var options = textView.Options.GlobalOptions;
            options.SetOptionValue(SuggestionModeOptionKey, value);
        }

        internal static bool GetSuggestionModeInDebuggerCompletionOption(ITextView textView)
        {
            var options = textView.Options.GlobalOptions;
            if (!(options.IsOptionDefined(SuggestionModeInDebuggerCompletionOptionKey, localScopeOnly: false)))
                options.SetOptionValue(SuggestionModeInDebuggerCompletionOptionKey, UseSuggestionModeInDebuggerCompletionDefaultValue);
            return options.GetOptionValue(SuggestionModeInDebuggerCompletionOptionKey);
        }

        internal static void SetSuggestionModeDuringDebuggingOption(ITextView textView, bool value)
        {
            var options = textView.Options.GlobalOptions;
            options.SetOptionValue(SuggestionModeInDebuggerCompletionOptionKey, value);
        }
    }
}
