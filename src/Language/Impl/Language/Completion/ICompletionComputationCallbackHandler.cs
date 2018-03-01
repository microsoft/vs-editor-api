using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    internal interface ICompletionComputationCallbackHandler<TModel>
    {
        Task UpdateUi(TModel model);
        void Dismiss();
    }
}
