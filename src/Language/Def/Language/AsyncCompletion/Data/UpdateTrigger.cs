using System;
using System.Diagnostics;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data
{
    /// <summary>
    /// What triggered updating of completion.
    /// </summary>
    [DebuggerDisplay("{Reason} {Character}")]
    public struct UpdateTrigger : IEquatable<UpdateTrigger>
    {
        /// <summary>
        /// The reason that completion was updated.
        /// </summary>
        public UpdateTriggerReason Reason { get; }

        /// <summary>
        /// The text edit associated with the triggering action.
        /// </summary>
        public char Character { get; }

        /// <summary>
        /// Creates a <see cref="UpdateTrigger"/> associated with a text edit
        /// </summary>
        /// <param name="reason">The kind of action that triggered completion to update</param>
        /// <param name="character">Character that triggered the update</param>
        public UpdateTrigger(UpdateTriggerReason reason, char character)
        {
            this.Reason = reason;
            this.Character = character;
        }

        /// <summary>
        /// Creates a <see cref="InitialTrigger"/> not associated with a text edit
        /// </summary>
        /// <param name="reason">The kind of action that triggered completion to update</param>
        public UpdateTrigger(UpdateTriggerReason reason) : this(reason, default(char))
        { }

        bool IEquatable<UpdateTrigger>.Equals(UpdateTrigger other) => Reason.Equals(other.Reason) && Character.Equals(other.Character);

        public override bool Equals(object other) => (other is InitialTrigger otherImage) ? ((IEquatable<UpdateTrigger>)this).Equals(otherImage) : false;

        public static bool operator ==(UpdateTrigger left, UpdateTrigger right) => left.Equals(right);

        public static bool operator !=(UpdateTrigger left, UpdateTrigger right) => !(left == right);

        public override int GetHashCode() => Reason.GetHashCode() ^ Character.GetHashCode();
    }
}
