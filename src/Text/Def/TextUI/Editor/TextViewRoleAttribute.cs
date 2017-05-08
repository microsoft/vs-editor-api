namespace Microsoft.VisualStudio.Text.Editor
{
    using System;
    using Microsoft.VisualStudio.Utilities;

    /// <summary>
    /// Use this attribute to specify the kinds of TextViews to which an extension applies.
    /// </summary>
    public sealed class TextViewRoleAttribute : MultipleBaseMetadataAttribute
    {
        string roles;

        /// <summary>
        /// Construct a new instance of the attribute.
        /// </summary>
        /// <param name="role">The case-insensitive name of the role.</param>
        /// <exception cref="ArgumentNullException"><paramref name="role"/> is null or empty.</exception>
        public TextViewRoleAttribute(string role)
        {
            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentNullException("role");
            }
            this.roles = role;
        }

        /// <summary>
        /// The role name.
        /// </summary>
        public string TextViewRoles
        {
            get { return this.roles; }
        }
    }
}