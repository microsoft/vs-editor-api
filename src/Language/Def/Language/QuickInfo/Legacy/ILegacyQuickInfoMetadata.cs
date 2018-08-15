namespace Microsoft.Internal.VisualStudio.Language.Intellisense
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.Utilities;

#pragma warning disable 618
    /// <summary>
    /// This interface supports the product infrastructure and should not be used.
    /// </summary>
    /// <remarks>
    /// This is a MEF metadata view, similar to IContentTypeMetadata, however it uses
    /// an explicit metadata class to allow it to be internal. Internal MEF metadata
    /// view interfaces are supported but are currently suffering from intermittent
    /// type load exceptions resulting from a bug in either the CLR or VS MEF.
    /// </remarks>
    [Obsolete("This interface supports legacy product infrastructure, is subject to breakage without notice, and should not be used")]
    internal class LegacyQuickInfoMetadata : IContentTypeMetadata, IOrderable
    {
        public LegacyQuickInfoMetadata(IDictionary<string, object> data)
        {
            // Values are all optional.
            data.TryGetValue(nameof(Name), out var name);
            data.TryGetValue(nameof(ContentTypes), out var contentTypes);
            data.TryGetValue(nameof(Before), out var before);
            data.TryGetValue(nameof(After), out var after);

            this.ContentTypes = (IEnumerable<string>)contentTypes;
            this.Name = (string)name;
            this.Before = (IEnumerable<string>)before;
            this.After = (IEnumerable<string>)after;
        }

        internal LegacyQuickInfoMetadata(
            string name,
            IEnumerable<string> contentTypes,
            IEnumerable<string> before,
            IEnumerable<string> after)
        {
            this.Name = name;
            this.ContentTypes = contentTypes;
            this.Before = before ?? Enumerable.Empty<string>();
            this.After = after ?? Enumerable.Empty<string>();
        }

        public IEnumerable<string> ContentTypes { get; }

        public string Name { get; }

        public IEnumerable<string> Before { get; }

        public IEnumerable<string> After { get; }
    }
#pragma warning restore 618
}
