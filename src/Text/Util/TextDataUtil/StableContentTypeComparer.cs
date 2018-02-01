using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// Custom comparer for sorting lists of content types that preserves original order of unrelated content types.
    /// </summary>
    internal class StableContentTypeComparer : IComparer<IEnumerable<string>>
    {
        private readonly IContentTypeRegistryService _contentTypeRegistryService;

        public StableContentTypeComparer(IContentTypeRegistryService contentTypeRegistryService)
        {
            _contentTypeRegistryService = contentTypeRegistryService ?? throw new ArgumentNullException(nameof(contentTypeRegistryService));
        }

        public int Compare(IEnumerable<string> x, IEnumerable<string> y)
        {
            if (x.SequenceEqual(y))
            {
                return 0;
            }

            foreach (var contentTypeXStr in x)
            {
                var contentTypeX = _contentTypeRegistryService.GetContentType(contentTypeXStr);
                if (contentTypeX != null)
                {
                    foreach (var contentTypeYStr in y)
                    {
                        if (contentTypeX.IsOfType(contentTypeYStr))
                        {
                            return -1;
                        }
                    }
                }
            }

            foreach (var contentTypeYStr in y)
            {
                var contentTypeY = _contentTypeRegistryService.GetContentType(contentTypeYStr);
                if (contentTypeY != null)
                {
                    foreach (var contentTypeXStr in x)
                    {
                        if (contentTypeY.IsOfType(contentTypeXStr))
                        {
                            return 1;
                        }
                    }
                }
            }

            // Content types are unrelated, there is no way resolve the tie so
            // let's consider them equal to preserve original order.
            return 0;
        }
    }
}
