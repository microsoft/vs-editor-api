using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    /// <summary>
    /// Lightweight stack-like view over a readonly ordered list of command handlers.
    /// </summary>
    internal class ImportBucket<T>
    {
        private int _currentImportSetting;
        private readonly IReadOnlyList<Lazy<T, IOrderableContentTypeMetadata>> _imports;

        public ImportBucket(IReadOnlyList<Lazy<T, IOrderableContentTypeMetadata>> imports)
        {
            _imports = imports ?? throw new ArgumentNullException(nameof(imports));
        }

        public bool IsEmpty => _currentImportSetting >= _imports.Count;

        public Lazy<T, IOrderableContentTypeMetadata> Peek()
        {
            if (!IsEmpty)
            {
                return _imports[_currentImportSetting];
            }

            throw new InvalidOperationException($"{nameof(ImportBucket<T>)} is empty.");
        }

        internal Lazy<T, IOrderableContentTypeMetadata> Pop()
        {
            if (!IsEmpty)
            {
                return _imports[_currentImportSetting++];
            }

            throw new InvalidOperationException($"{nameof(ImportBucket<T>)} is empty.");
        }
    }
}
