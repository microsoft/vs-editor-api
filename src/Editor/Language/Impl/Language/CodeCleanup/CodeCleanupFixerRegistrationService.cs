using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;

namespace Microsoft.VisualStudio.Language.CodeCleanUp
{
    /// <summary>
    /// Fixer registration service. This contains a set of fixers exported by components which
    /// are called during one click code clean up
    /// </summary>
    [Export(typeof(ICodeCleanUpFixerRegistrationService))]
    public class CodeCleanUpFixerRegistrationService : ICodeCleanUpFixerRegistrationService
    {
        private ImmutableHashSet<ICodeCleanUpFixerProvider> registeredFixerProviders = ImmutableHashSet.Create<ICodeCleanUpFixerProvider>();

        /// <summary>
        /// Fixer identifiers enabled by the client appliication
        /// </summary>
        private FixIdContainer enabledFixerIds;

        /// <inheritdoc/>
        public IReadOnlyCollection<ICodeCleanUpFixerProvider> RegisteredFixerProviders => registeredFixerProviders;

        /// <summary>
        /// Set of fixer identitifers exported by fixers indicating which fixes they can process
        /// </summary>
        [ImportMany]
        private readonly List<Lazy<FixIdDefinition, IFixIdDefinitionMetadata>> fixerCodeDefinitions;

        /// <inheritdoc/>
        public FixIdContainer EnabledFixIds
        {
            get
            {
                if (this.enabledFixerIds == null)
                {
                    var codes = this.fixerCodeDefinitions.Select<Lazy<FixIdDefinition, IFixIdDefinitionMetadata>, IFixInformation>((definition) => new FixerCodeInfo(definition));
                    this.enabledFixerIds = new FixIdContainer(codes.Any() ? codes.ToImmutableList() : ImmutableList<IFixInformation>.Empty);
                }

                return this.enabledFixerIds;
            }
        }

          /// <inheritdoc/>
        public bool TryRegisterFixerProvider(ICodeCleanUpFixerProvider fixerProvider)
        {
            Requires.NotNull(fixerProvider, nameof(fixerProvider));
            return ImmutableInterlocked.Update(ref this.registeredFixerProviders,
                            (collection, item) => collection.Add(item),
                            fixerProvider);
        }

        /// <inheritdoc/>
        public bool TryUnRegisterFixerProvider(ICodeCleanUpFixerProvider fixerProvider)
        {
            Requires.NotNull(fixerProvider, nameof(fixerProvider));

            return ImmutableInterlocked.Update(ref this.registeredFixerProviders,
                            (collection, item) => collection.Remove(item),
                            fixerProvider);
        }
    }
}
