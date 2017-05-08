// Copyright (c) Microsoft Corporation
// All rights reserved

namespace Microsoft.VisualStudio.Text
{
    /// <summary>
    /// Describes whether a position in a <see cref="ITextBuffer"/> that can be thought of as
    /// lying between two characters is coupled to the preceding character or the following character.
    /// </summary>
    public enum PositionAffinity
    {
        /// <summary>
        /// The position is coupled to with the preceding character.
        /// </summary>
        Predecessor,

        /// <summary>
        /// The position is coupled to the following character.
        /// </summary>
        Successor
    }
}