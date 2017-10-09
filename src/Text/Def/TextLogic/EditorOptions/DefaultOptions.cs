//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods
{
    /// <summary>
    /// Extension methods for common general options.
    /// </summary>
    public static class DefaultOptionExtensions
    {
        #region Extension methods
        /// <summary>
        /// Determines whether the option to convert tabs to spaces is enabled in the specified <see cref="IEditorOptions"/>.
        /// </summary>
        /// <param name="options">The <see cref="IEditorOptions"/>.</param>
        /// <returns><c>true</c> if the option is enabled, otherwise <c>false</c>.</returns>
        public static bool IsConvertTabsToSpacesEnabled(this IEditorOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            return options.GetOptionValue(DefaultOptions.ConvertTabsToSpacesOptionId);
        }

        /// <summary>
        ///Gets the size of the tab for the specified <see cref="IEditorOptions"/>.
        /// </summary>
        /// <param name="options">The <see cref="IEditorOptions"/>.</param>
        /// <returns>The number of spaces of the tab size.</returns>
        public static int GetTabSize(this IEditorOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            return options.GetOptionValue(DefaultOptions.TabSizeOptionId);
        }

        /// <summary>
        ///Gets the size of an indent for the specified <see cref="IEditorOptions"/>.
        /// </summary>
        /// <param name="options">The <see cref="IEditorOptions"/>.</param>
        /// <returns>The number of spaces of the indent size.</returns>
        public static int GetIndentSize(this IEditorOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            return options.GetOptionValue(DefaultOptions.IndentSizeOptionId);
        }

        /// <summary>
        /// Determines whether to duplicate the new line character if it is already present when inserting a new line.
        /// </summary>
        /// <param name="options">The <see cref="IEditorOptions"/>.</param>
        /// <returns><c>true</c> if the new line character should be duplicated, otherwise <c>false</c>.</returns>
        public static bool GetReplicateNewLineCharacter(this IEditorOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            return options.GetOptionValue(DefaultOptions.ReplicateNewLineCharacterOptionId);
        }

        /// <summary>
        /// Gets the new line character for the specified editor options.
        /// </summary>
        /// <param name="options">The <see cref="IEditorOptions"/>.</param>
        /// <returns>A string containing the new line character or characters.</returns>
        public static string GetNewLineCharacter(this IEditorOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            return options.GetOptionValue(DefaultOptions.NewLineCharacterOptionId);
        }

        /// <summary>
        /// Determines whether to trim trailing whitespace.
        /// </summary>
        /// <param name="options">The <see cref="IEditorOptions"/>.</param>
        /// <returns><c>true</c> if trailing whitespace should be trimmed, otherwise <c>false</c>.</returns>
        public static bool GetTrimTrailingWhieSpace(this IEditorOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            return options.GetOptionValue(DefaultOptions.TrimTrailingWhiteSpaceOptionId);
        }

        /// <summary>
        /// Determines whether to insert final newline.
        /// </summary>
        /// <param name="options">The <see cref="IEditorOptions"/>.</param>
        /// <returns><c>true</c> if a final new line should be inserted, otherwise <c>false</c>.</returns>
        public static bool GetInsertFinalNewLine(this IEditorOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            return options.GetOptionValue(DefaultOptions.InsertFinalNewLineOptionId);
        }

        #endregion
    }
}

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Common general options.
    /// </summary>
    public static class DefaultOptions
    {
        #region Option identifiers
        /// <summary>
        /// The default option that determines whether to convert tabs to spaces.
        /// </summary>
        public static readonly EditorOptionKey<bool> ConvertTabsToSpacesOptionId = new EditorOptionKey<bool>(ConvertTabsToSpacesOptionName);
        public const string ConvertTabsToSpacesOptionName = "Tabs/ConvertTabsToSpaces";

        /// <summary>
        /// The default option that determines size of a tab.
        /// </summary>
        /// <remarks>This option is used to determine the numerical column offset of a tab
        /// character ('\t') and, if <see cref="ConvertTabsToSpaces"/> is enabled, the number of spaces to which a tab
        /// should be converted.</remarks>
        public static readonly EditorOptionKey<int> TabSizeOptionId = new EditorOptionKey<int>(TabSizeOptionName);
        public const string TabSizeOptionName = "Tabs/TabSize";

        /// <summary>
        /// The default option that determines size of an indent.
        /// </summary>
        /// <remarks>This option is used to determine the numerical column offset of an indent level.</remarks>
        public static readonly EditorOptionKey<int> IndentSizeOptionId = new EditorOptionKey<int>(IndentSizeOptionName);
        public const string IndentSizeOptionName = "Tabs/IndentSize";

        /// <summary>
        /// The default option that determines whether to duplicate the new line character already present
        /// when inserting a new line.
        /// </summary>
        public static readonly EditorOptionKey<bool> ReplicateNewLineCharacterOptionId = new EditorOptionKey<bool>(ReplicateNewLineCharacterOptionName);
        public const string ReplicateNewLineCharacterOptionName = "ReplicateNewLineCharacter";

        /// <summary>
        /// The default option that determines the newline character or characters. 
        /// </summary>
        /// <remarks>The newline character can be a string, as in the common case of "\r\n". This setting applies
        /// when <see cref="ReplicateNewLineCharacter"/> is <c>false</c>, or when <see cref="ReplicateNewLineCharacter"/> is <c>true</c> and
        /// the text buffer is empty.</remarks>
        public static readonly EditorOptionKey<string> NewLineCharacterOptionId = new EditorOptionKey<string>(NewLineCharacterOptionName);
        public const string NewLineCharacterOptionName = "NewLineCharacter";

        /// <summary>
        /// The default option that determines the threshold for special handling of long lines.
        /// </summary>
        /// <remarks>
        /// Some operations will not operate on lines longer than this threshold.
        /// </remarks>
        public static readonly EditorOptionKey<int> LongBufferLineThresholdId = new EditorOptionKey<int>(LongBufferLineThresholdOptionName);
        public const string LongBufferLineThresholdOptionName = "LongBufferLineThreshold";

        /// <summary>
        /// The default option that determines the chunking size for long lines.
        /// </summary>
        /// <remarks>
        /// Lines longer than <see cref="LongBufferLineThreshold"/> may be considered in chunks of this size.
        /// </remarks>
        public static readonly EditorOptionKey<int> LongBufferLineChunkLengthId = new EditorOptionKey<int>(LongBufferLineChunkLengthOptionName);
        public const string LongBufferLineChunkLengthOptionName = "LongBufferLineChunkLength";

        /// <summary>
        /// The default option that determines whether to trim trailing whitespace.
        /// </summary>
        public static readonly EditorOptionKey<bool> TrimTrailingWhiteSpaceOptionId = new EditorOptionKey<bool>(TrimTrailingWhiteSpaceOptionName);
        public const string TrimTrailingWhiteSpaceOptionName = "TrimTrailingWhiteSpace";

        /// <summary>
        /// The default option that determines whether to insert final new line charcter.
        /// </summary>
        public static readonly EditorOptionKey<bool> InsertFinalNewLineOptionId = new EditorOptionKey<bool>(InsertFinalNewLineOptionName);
        public const string InsertFinalNewLineOptionName = "InsertFinalNewLine";

        #endregion
    }

    #region Option definitions
    /// <summary>
    /// The option definition that determines whether to convert tabs to spaces.
    /// </summary>
    [Export(typeof(EditorOptionDefinition))]
    [Name(DefaultOptions.ConvertTabsToSpacesOptionName)]
    public sealed class ConvertTabsToSpaces : EditorOptionDefinition<bool>
    {
        /// <summary>
        /// Gets the default value (<c>true</c>)>.
        /// </summary>
        public override bool Default { get { return true; } }

        /// <summary>
        /// Gets the editor option key.
        /// </summary>
        public override EditorOptionKey<bool> Key { get { return DefaultOptions.ConvertTabsToSpacesOptionId; } }
    }

    /// <summary>
    /// The option definition that determines the size (in number of spaces) of a tab.
    /// </summary>
    [Export(typeof(EditorOptionDefinition))]
    [Name(DefaultOptions.TabSizeOptionName)]
    public sealed class TabSize : EditorOptionDefinition<int>
    {
        /// <summary>
        /// Gets the default value (4).
        /// </summary>
        public override int Default { get { return 4; } }

        /// <summary>
        /// Gets the editor option key.
        /// </summary>
        public override EditorOptionKey<int> Key { get { return DefaultOptions.TabSizeOptionId; } }

        /// <summary>
        /// Determines whether a given tab size is valid.
        /// </summary>
        /// <param name="proposedValue">The size of the tab, in number of spaces.</param>
        /// <returns><c>true</c> if <paramref name="proposedValue"/> is a valid size, otherwise <c>false</c>.</returns>
        public override bool IsValid(ref int proposedValue)
        {
            return proposedValue > 0;
        }
    }

    /// <summary>
    /// The option definition that determines the size (in number of spaces) of an indent.
    /// </summary>
    [Export(typeof(EditorOptionDefinition))]
    [Name(DefaultOptions.IndentSizeOptionName)]
    public sealed class IndentSize : EditorOptionDefinition<int>
    {
        /// <summary>
        /// Gets the default value (4).
        /// </summary>
        public override int Default { get { return 4; } }

        /// <summary>
        /// Gets the editor option key.
        /// </summary>
        public override EditorOptionKey<int> Key { get { return DefaultOptions.IndentSizeOptionId; } }

        /// <summary>
        /// Determines whether a given indent size is valid.
        /// </summary>
        /// <param name="proposedValue">The size of the indent, in number of spaces.</param>
        /// <returns><c>true</c> if <paramref name="proposedValue"/> is a valid size, otherwise <c>false</c>.</returns>
        public override bool IsValid(ref int proposedValue)
        {
            return proposedValue > 0;
        }
    }

    /// <summary>
    /// The option definition that determines whether to duplicate a newline character when inserting a line.
    /// </summary>
    [Export(typeof(EditorOptionDefinition))]
    [Name(DefaultOptions.ReplicateNewLineCharacterOptionName)]
    public sealed class ReplicateNewLineCharacter : EditorOptionDefinition<bool>
    {
        /// <summary>
        /// Gets the default value (<c>true</c>).
        /// </summary>
        public override bool Default { get { return true; } }

        /// <summary>
        /// Gets the editor option key.
        /// </summary>
        public override EditorOptionKey<bool> Key { get { return DefaultOptions.ReplicateNewLineCharacterOptionId; } }
    }

    /// <summary>
    /// The option definition that specifies the newline character or characters.
    /// </summary>
    [Export(typeof(EditorOptionDefinition))]
    [Name(DefaultOptions.NewLineCharacterOptionName)]
    public sealed class NewLineCharacter : EditorOptionDefinition<string>
    {
        /// <summary>
        /// Gets the default value ("\r\n").
        /// </summary>
        public override string Default { get { return "\r\n"; } }

        /// <summary>
        /// Gets the editor option key.
        /// </summary>
        public override EditorOptionKey<string> Key { get { return DefaultOptions.NewLineCharacterOptionId; } }
    }

    /// <summary>
    /// The option definition that determines the threshold for special handling of long lines.
    /// </summary>
    [Export(typeof(EditorOptionDefinition))]
    [Name(DefaultOptions.LongBufferLineThresholdOptionName)]
    public sealed class LongBufferLineThreshold : EditorOptionDefinition<int>
    {
        /// <summary>
        /// Gets the default value (32K).
        /// </summary>
        public override int Default { get { return 32 * 1024; } }

        /// <summary>
        /// Gets the editor option key.
        /// </summary>
        public override EditorOptionKey<int> Key { get { return DefaultOptions.LongBufferLineThresholdId; } }
    }

    /// <summary>
    /// The option definition that determines the determines the chunking size for long lines.
    /// </summary>
    [Export(typeof(EditorOptionDefinition))]
    [Name(DefaultOptions.LongBufferLineChunkLengthOptionName)]
    public sealed class LongBufferLineChunk : EditorOptionDefinition<int>
    {
        /// <summary>
        /// Gets the default value (4K).
        /// </summary>
        public override int Default { get { return 4 * 1024; } }

        /// <summary>
        /// Gets the editor option key.
        /// </summary>
        public override EditorOptionKey<int> Key { get { return DefaultOptions.LongBufferLineChunkLengthId; } }
    }

    /// <summary>
    /// The option definition that determines whether to trim trailing whitespace.
    /// </summary>
    [Export(typeof(EditorOptionDefinition))]
    [Name(DefaultOptions.TrimTrailingWhiteSpaceOptionName)]
    public sealed class TrimTrailingWhiteSpace : EditorOptionDefinition<bool>
    {
        /// <summary>
        /// Gets the default value (<c>false</c>).
        /// </summary>
        public override bool Default { get { return false; } }

        /// <summary>
        /// Gets the editor option key.
        /// </summary>
        public override EditorOptionKey<bool> Key { get { return DefaultOptions.TrimTrailingWhiteSpaceOptionId; } }
    }

    /// <summary>
    /// The option definition that determines whether to insert a final newline.
    /// </summary>
    [Export(typeof(EditorOptionDefinition))]
    [Name(DefaultOptions.InsertFinalNewLineOptionName)]
    public sealed class InsertFinalNewLine : EditorOptionDefinition<bool>
    {
        /// <summary>
        /// Gets the default value (<c>false</c>).
        /// </summary>
        public override bool Default { get { return false; } }

        /// <summary>
        /// Gets the editor option key.
        /// </summary>
        public override EditorOptionKey<bool> Key { get { return DefaultOptions.InsertFinalNewLineOptionId; } }
    }

    #endregion
}
