namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data
{
    /// <summary>
    /// Describes the kind of action that initially triggered completion to open.
    /// </summary>
    public enum InitialTriggerReason
    {
        /// <summary>
        /// Completion was triggered by a direct invocation of the completion feature
        /// using the Edit.ListMember command.
        /// </summary>
        Invoke,

        /// <summary>
        /// Completion was triggered with a request to commit if a single item would be selected
        /// using the Edit.CompleteWord command.
        /// </summary>
        InvokeAndCommitIfUnique,

        /// <summary>
        /// Completion was triggered via an action inserting a character into the document.
        /// </summary>
        Insertion,

        /// <summary>
        /// Completion was triggered via an action deleting a character from the document.
        /// </summary>
        Deletion,

        /// <summary>
        /// Completion was triggered for snippets only.
        /// </summary>
        Snippets,
    }
}
