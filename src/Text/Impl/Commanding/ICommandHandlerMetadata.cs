using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.UI.Text.Commanding.Implementation
{
    public interface ICommandHandlerMetadata : IOrderable, IContentTypeMetadata
    {
        [DefaultValue(null)] // [TextViewRole] is optional
        IEnumerable<string> TextViewRoles { get; }
    }

}
