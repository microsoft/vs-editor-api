//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Microsoft.VisualStudio.Text.Utilities
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal sealed class PerformanceBlockMarker
    {
        [ImportMany]
        private List<Lazy<IPerformanceMarkerBlockProvider>> _performanceMarkerBlockProviders = null;

        internal IDisposable CreateBlock(string blockName)
        {
            // Unit tests case
            if (_performanceMarkerBlockProviders == null || _performanceMarkerBlockProviders.Count == 0)
            {
                return new Block();
            }

            // Optimize for the most common case
            if (_performanceMarkerBlockProviders.Count == 1)
            {
                IDisposable block = _performanceMarkerBlockProviders[0].Value?.CreateBlock(blockName);
                if (block != null)
                {
                    return block;
                }
            }

            var providedBlocks = new FrugalList<IDisposable>();
            for (int i = 0; i < _performanceMarkerBlockProviders.Count; i++)
            {
                providedBlocks.Add(_performanceMarkerBlockProviders[i].Value?.CreateBlock(blockName));
            }
            
            return new Block(providedBlocks);
        }

        private class Block : IDisposable
        {
            private readonly FrugalList<IDisposable> _markers;

            public Block(FrugalList<IDisposable> markers)
            {
                _markers = markers;
            }

            public Block()
            {
            }

            public void Dispose()
            {
                if (_markers == null)
                {
                    return;
                }

                foreach (var marker in _markers)
                {
                    marker?.Dispose();
                }
            }
        }
    }
}
