//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
// This file contain implementations details that are subject to change without notice.
// Use at your own risk.
//
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.Text.Utilities;

namespace Microsoft.VisualStudio.Text.Implementation
{
    internal class PageManager
    {
        // this class inherits from page so that it participates in the MRU list, of which it is the sentinel node.
        private ImmutableList<Page> _mru = ImmutableList<Page>.Empty;
        private readonly int _maxPages;

        public PageManager()
        {
            _maxPages = TextModelOptions.CompressedStorageMaxLoadedPages;
        }

        public void UpdateMRU(Page page)
        {
            var oldMRU = Volatile.Read(ref _mru);
            while (true)
            {
                ImmutableList<Page> newMRU;

                int index = oldMRU.IndexOf(page);
                if (index >= 0)
                {
                    if (index == (oldMRU.Count - 1))
                    {
                        // Page is already at the top of the MRU so nothing needs to be done.
                        return;
                    }

                    // Was in the list, but not at the top. Remove it in preparation for adding it later.
                    newMRU = oldMRU.RemoveAt(index);
                }
                else if (oldMRU.Count >= _maxPages)
                {
                    // Wasn't in the list and the list is full. Remove the oldest in preparation for adding it later.
                    newMRU = oldMRU.RemoveAt(0);
                }
                else
                {
                    newMRU = oldMRU;
                }

                newMRU = newMRU.Add(page);

                var result = Interlocked.CompareExchange(ref _mru, newMRU, oldMRU);
                if (result == oldMRU)
                    return;

                oldMRU = result;
            }
        }
    }
}
