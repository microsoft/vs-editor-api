using System;
using System.Diagnostics;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data
{
    /// <summary>
    /// What triggered the completion, but not where it happened.
    /// The reason we don't expose location is that for each extension,
    /// we map the point to a buffer with matching content type.
    /// </summary>
    [DebuggerDisplay("{Reason} {Character}")]
    public struct InitialTrigger : IEquatable<InitialTrigger>
    {
        /// <summary>
        /// The reason that completion was started.
        /// </summary>
        public InitialTriggerReason Reason { get; }

        /// <summary>
        /// The text edit associated with the triggering action.
        /// </summary>
        public char Character { get; }

        /// <summary>
        /// Creates a <see cref="InitialTrigger"/> associated with a text edit
        /// </summary>
        /// <param name="reason">The kind of action that triggered completion to start</param>
        /// <param name="character">Character that triggered completion</param>
        public InitialTrigger(InitialTriggerReason reason, char character)
        {
            this.Reason = reason;
            this.Character = character;
        }

        /// <summary>
        /// Creates a <see cref="InitialTrigger"/> not associated with a text edit
        /// </summary>
        /// <param name="reason">The kind of action that triggered completion to start</param>
        public InitialTrigger(InitialTriggerReason reason) : this(reason, default)
        { }

        bool IEquatable<InitialTrigger>.Equals(InitialTrigger other) => Reason.Equals(other.Reason) && Character.Equals(other.Character);

        public override bool Equals(object other) => (other is InitialTrigger otherImage) ? ((IEquatable<InitialTrigger>)this).Equals(otherImage) : false;

        public static bool operator ==(InitialTrigger left, InitialTrigger right) => left.Equals(right);

        public static bool operator !=(InitialTrigger left, InitialTrigger right) => !(left == right);

        public override int GetHashCode() => Reason.GetHashCode() ^ Character.GetHashCode();
    }
}
