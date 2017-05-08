using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// A base class for attributes that can appear only once on a single component part.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false)]
    public abstract class SingletonBaseMetadataAttribute : Attribute
    {
    }
}
