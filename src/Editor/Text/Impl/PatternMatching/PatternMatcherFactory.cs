using System;
using System.ComponentModel.Composition;
using System.Linq;
using static Microsoft.VisualStudio.Text.PatternMatching.PatternMatcherCreationFlags;

namespace Microsoft.VisualStudio.Text.PatternMatching.Implementation
{
    [Export(typeof(IPatternMatcherFactory))]
    internal class PatternMatcherFactory : IPatternMatcherFactory
    {
        public IPatternMatcher CreatePatternMatcher(string pattern, PatternMatcherCreationOptions creationOptions)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("A non-empty pattern is required to create a pattern matcher", nameof(pattern));
            }

            if (creationOptions == null)
            {
                throw new ArgumentNullException(nameof(creationOptions));
            }

            if (creationOptions.ContainerSplitCharacters == null)
            {
                return PatternMatcher.CreateSimplePatternMatcher(
                    pattern,
                    creationOptions.CultureInfo,
                    creationOptions.Flags.HasFlag(IncludeMatchedSpans),
                    creationOptions.Flags.HasFlag(AllowFuzzyMatching),
                    creationOptions.Flags.HasFlag(AllowSimpleSubstringMatching));
            }
            else
            {
                return PatternMatcher.CreateContainerPatternMatcher(
                    pattern.Split(creationOptions.ContainerSplitCharacters.ToArray()),
                    creationOptions.ContainerSplitCharacters,
                    creationOptions.CultureInfo,
                    creationOptions.Flags.HasFlag(AllowFuzzyMatching),
                    creationOptions.Flags.HasFlag(AllowSimpleSubstringMatching),
                    creationOptions.Flags.HasFlag(IncludeMatchedSpans));
            }
        }
    }
}
