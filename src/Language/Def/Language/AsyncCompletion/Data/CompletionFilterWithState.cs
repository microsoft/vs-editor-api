using System;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data
{
    /// <summary>
    /// Immutable data transfer object used to communicate between the completion session and completion UI
    /// </summary>
    public sealed class CompletionFilterWithState
    {
        /// <summary>
        /// Reference to the completion filter
        /// </summary>
        public CompletionFilter Filter { get; }

        /// <summary>
        /// Whether the filter is available.
        /// Filter should be available when there are visible <see cref="CompletionItem"/>s that define this <see cref="Filter"/> in their <see cref="CompletionItem.Filters"/>
        /// </summary>
        public bool IsAvailable { get; }

        /// <summary>
        /// Whether the filter is selected by the user.
        /// </summary>
        public bool IsSelected { get; }

        /// <summary>
        /// Constructs a new instance of <see cref="CompletionFilterWithState"/>.
        /// </summary>
        /// <param name="filter">Reference to <see cref="CompletionFilter"/></param>
        /// <param name="isAvailable">Whether this <see cref="CompletionFilter"/> is available</param>
        public CompletionFilterWithState(CompletionFilter filter, bool isAvailable)
            : this(filter, isAvailable, isSelected: false)
        { }

        /// <summary>
        /// Constructs a new instance of <see cref="CompletionFilterWithState"/> when selected state is known.
        /// </summary>
        /// <param name="filter">Reference to <see cref="CompletionFilter"/></param>
        /// <param name="isAvailable">Whether this <see cref="CompletionFilter"/> is available</param>
        /// <param name="isSelected">Whether this <see cref="CompletionFilter"/> is selected</param>
        public CompletionFilterWithState(CompletionFilter filter, bool isAvailable, bool isSelected)
        {
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
            IsAvailable = isAvailable;
            IsSelected = isSelected;
        }

        /// <summary>
        /// Returns instance of <see cref="CompletionFilterWithState"/> with specified <see cref="IsAvailable"/>
        /// </summary>
        /// <param name="isAvailable">Value to use for <see cref="IsAvailable"/></param>
        /// <returns>Updated instance of <see cref="CompletionFilterWithState"/></returns>
        public CompletionFilterWithState WithAvailability(bool isAvailable)
        {
            return this.IsAvailable == isAvailable
                ? this
                : new CompletionFilterWithState(Filter, isAvailable, IsSelected);
        }

        /// <summary>
        /// Returns instance of <see cref="CompletionFilterWithState"/> with specified <see cref="IsSelected"/>
        /// </summary>
        /// <param name="availability">Value to use for <see cref="IsSelected"/></param>
        /// <returns>Updated instance of <see cref="CompletionFilterWithState"/></returns>
        public CompletionFilterWithState WithSelected(bool isSelected)
        {
            return this.IsSelected == isSelected
                ? this
                : new CompletionFilterWithState(Filter, IsAvailable, isSelected);
        }

        /// <summary>
        /// Override for nice debugger display
        /// </summary>
        public override string ToString()
        {
            var availableStatus = IsAvailable ? "available" : "unavailable";
            var selectedStatus = IsSelected ? "selected" : "not selected";
            return $"{Filter.DisplayText} - {availableStatus}, {selectedStatus}";
        }
    }
}
