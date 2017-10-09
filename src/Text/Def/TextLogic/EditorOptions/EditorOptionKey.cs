//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Represents a type-safe key for editor options.
    /// </summary>
    /// <typeparam name="T">The type of the option value.</typeparam>
    public struct EditorOptionKey<T>
    {
        #region Private data
        private string _name;
        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="EditorOptionKey&lt;T&gt;"/>.
        /// </summary>
        /// <param name="name">The name of the option key.</param>
        public EditorOptionKey(string name) { _name = name; }

        /// <summary>
        /// Gets the name of this key.
        /// </summary>
        public string Name { get { return _name; } }

        #region Object overrides

        /// <summary>
        /// Determines whether two <see cref="EditorOptionKey&lt;T&gt;"/> objects are the same.
        /// </summary>
        /// <param name="obj">The object to be compared.</param>
        /// <returns><c>true</c> if the objects are the same, otherwise <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is EditorOptionKey<T>)
            {
                EditorOptionKey<T> other = (EditorOptionKey<T>)obj;
                return other.Name == this.Name;
            }

            return false;
        }

        /// <summary>
        /// Gets the hash code for this object.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        /// <summary>
        /// Converts this object to a string. 
        /// </summary>
        /// <returns>The name of the option.</returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Determines whether two instances of this type are the same.
        /// </summary>
        public static bool operator ==(EditorOptionKey<T> left, EditorOptionKey<T> right)
        {
            return left.Name == right.Name;
        }

        /// <summary>
        /// Determines whether two instances of this type are different.
        /// </summary>
        public static bool operator !=(EditorOptionKey<T> left, EditorOptionKey<T> right)
        {
            return !(left == right);
        }

        #endregion
    }

}
