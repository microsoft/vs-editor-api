//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Processes the keyboard input of the editor.
    /// </summary>
    /// <remarks>
    /// Export this functionality by using the <see cref="IKeyProcessorProvider"/>.
    /// </remarks>
    public abstract class KeyProcessor
    {
        /// <summary>
        /// Determines whether this processor should be called for events that have
        /// been handled by earlier <see cref="KeyProcessor"/> objects.
        /// </summary>
        public virtual bool IsInterestedInHandledEvents => false;

        /// <summary>
        /// Handles the KeyDown event.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        public virtual void KeyDown(KeyEvent e)
        {
        }

        /// <summary>
        /// Handles the KeyUp event.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        public virtual void KeyUp(KeyEvent e)
        {
        }

        /// <summary>
        /// Handles the FlagsChanged event.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        public virtual void FlagsChanged(KeyEvent e)
        {
        }
    }
}
