using System;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data
{
    /// <summary>
    /// Describes the kind of action that triggered completion to filter.
    /// </summary>
    public enum UpdateTriggerReason
    {
        /// <summary>
        /// Completion was triggered by a direct invocation of the completion feature
        /// using the Edit.ListMember command.
        /// </summary>
        Initial,

        /// <summary>
        /// Completion was triggered via an action inserting a character into the document.
        /// </summary>
        Insertion,

        /// <summary>
        /// Completion was triggered via an action deleting a character from the document.
        /// </summary>
        Deletion,

        /// <summary>
        /// Update was triggered by changing filters
        /// </summary>
        FilterChange,
    }
}
