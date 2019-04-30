//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
namespace Microsoft.VisualStudio.Text.Editor
{
    using AppKit;

    /// <summary>
    /// Processes the keyboard input of the editor.
    /// </summary>
    /// <remarks>
    /// Export this functionality by using the <see cref="IKeyProcessorProvider"/>.
    /// </remarks>
    public abstract class KeyProcessor
    {
        /// <summary>
        /// Handles the KeyDown event.
        /// </summary>
        /// <param name="theEvent">
        /// A <see cref="NSEvent"/> describing the key event.
        /// </param>
        public virtual void KeyDown(NSEvent theEvent) { }

        /// <summary>
        /// Handles the KeyUp event.
        /// </summary>
        /// <param name="theEvent">
        /// A <see cref="NSEvent"/> describing the key event.
        /// </param>
        public virtual void KeyUp(NSEvent theEvent) { }

        /// <summary>
        /// Handles the FlagsChanged event.
        /// </summary>
        /// <param name="theEvent">
        /// A <see cref="FlagsChanged"/> describing the key event.
        /// </param>
        public virtual void FlagsChanged(NSEvent theEvent) { }
    }
}
