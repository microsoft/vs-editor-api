using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// Abstract base implementation of the <see cref="IUIThreadOperationContext"/> interface.
    /// </summary>
    public abstract class AbstractUIThreadOperationContext : IUIThreadOperationContext
    {
        private List<IUIThreadOperationScope> _scopes;
        private bool _allowCancellation;
        private PropertyCollection _properties;
        private readonly string _contextDescription;
        private int _completedItems;
        private int _totalItems;

        /// <summary>
        /// Creates a new instance of the <see cref="AbstractUIThreadOperationContext"/>.
        /// </summary>
        /// <param name="allowCancellation">Initial value of the <see cref="IUIThreadOperationContext.AllowCancellation"/>
        /// property, which can change as new scopes are added to the context.</param>
        /// <param name="description">Initial value of the <see cref="IUIThreadOperationContext.Description"/>
        /// property, which can change as new scopes are added to the context.</param>
        public AbstractUIThreadOperationContext(bool allowCancellation, string description)
        {
            _contextDescription = description ?? throw new ArgumentNullException(nameof(description));
            _allowCancellation = allowCancellation;
        }

        /// <summary>
        /// Cancellation token for cancelling the operation.
        /// </summary>
        public virtual CancellationToken UserCancellationToken => CancellationToken.None;

        /// <summary>
        /// Gets whether the operation can be cancelled.
        /// </summary>
        /// <remarks>This value is composed of initial AllowCancellation value and
        /// <see cref="IUIThreadOperationScope.AllowCancellation"/> values of all currently added scopes.
        /// The value composition logic takes into acount disposed scopes too - if any of added scopes
        /// were disposed while its <see cref="IUIThreadOperationScope.AllowCancellation"/> was false,
        /// this property will stay false regardless of all other scopes' <see cref="IUIThreadOperationScope.AllowCancellation"/>
        /// values.
        /// </remarks>
        public virtual bool AllowCancellation
        {
            get
            {
                if (!_allowCancellation)
                {
                    return false;
                }

                if (_scopes == null || _scopes.Count == 0)
                {
                    return _allowCancellation;
                }

                return _scopes.All((s) => s.AllowCancellation);
            }
        }

        /// <summary>
        /// Gets user readable operation description, composed of initial context description and
        /// descriptions of all currently added scopes.
        /// </summary>
        public virtual string Description
        {
            get
            {
                if (_scopes == null || _scopes.Count == 0)
                {
                    return _contextDescription;
                }

                // Combine context description with descriptions of all current scopes
                return _contextDescription + Environment.NewLine + string.Join(Environment.NewLine, _scopes.Select((s) => s.Description));
            }
        }

        protected int CompletedItems => _completedItems;
        protected int TotalItems => _totalItems;

        private IList<IUIThreadOperationScope> LazyScopes => _scopes ?? (_scopes = new List<IUIThreadOperationScope>());

        /// <summary>
        /// Gets current list of <see cref="IUIThreadOperationScope"/>s in this context.
        /// </summary>
        public virtual IEnumerable<IUIThreadOperationScope> Scopes => this.LazyScopes;

        /// <summary>
        /// A collection of properties.
        /// </summary>
        public virtual PropertyCollection Properties => _properties ?? (_properties = new PropertyCollection());

        /// <summary>
        /// Adds an UI thread operation scope with its own cancellability, description and progress tracker.
        /// The scope is removed from the context on dispose.
        /// </summary>
        public virtual IUIThreadOperationScope AddScope(bool allowCancellation, string description)
        {
            var scope = new UIThreadOperationScope(allowCancellation, description, this);
            this.LazyScopes.Add(scope);
            this.OnScopesChanged();
            return scope;
        }

        protected virtual void OnScopeProgressChanged(IUIThreadOperationScope changedScope)
        {
            int completed = 0;
            int total = 0;
            foreach (UIThreadOperationScope scope in this.LazyScopes)
            {
                completed += scope.CompletedItems;
                total += scope.TotalItems;
            }

            Interlocked.Exchange(ref _completedItems, completed);
            Interlocked.Exchange(ref _totalItems, total);
        }

        /// <summary>
        /// Invoked when new <see cref="IUIThreadOperationScope"/>s are added or disposed.
        /// </summary>
        protected virtual void OnScopesChanged() { }

        protected virtual void OnScopeChanged(IUIThreadOperationScope uiThreadOperationScope)
        {
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Allows a component to take full ownership over this UI thread operation, for example
        /// when it shows its own modal UI dialog and handles cancellability through that dialog instead.
        /// </summary>
        public virtual void TakeOwnership()
        {
        }

        protected virtual void OnScopeDisposed(IUIThreadOperationScope scope)
        {
            _allowCancellation &= scope.AllowCancellation;
            _scopes.Remove(scope);
            OnScopesChanged();
        }

        private class UIThreadOperationScope : IUIThreadOperationScope
        {
            private bool _allowCancellation;
            private string _description;
            private IProgress<ProgressInfo> _progress;
            private readonly AbstractUIThreadOperationContext _context;
            private int _completedItems;
            private int _totalItems;

            public UIThreadOperationScope(bool allowCancellation, string description, AbstractUIThreadOperationContext context)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                this.AllowCancellation = allowCancellation;
                this.Description = description ?? "";
            }

            public bool AllowCancellation
            {
                get { return _allowCancellation; }
                set
                {
                    if (_allowCancellation != value)
                    {
                        _allowCancellation = value;
                        _context.OnScopeChanged(this);
                    }
                }
            }

            public string Description
            {
                get { return _description; }
                set
                {
                    if (_description != value)
                    {
                        _description = value;
                        _context.OnScopeChanged(this);
                    }
                }
            }

            public IUIThreadOperationContext Context => _context;

            public IProgress<ProgressInfo> Progress => _progress ?? (_progress = new Progress<ProgressInfo>((progressInfo) => OnProgressChanged(progressInfo)));

            public int CompletedItems => _completedItems;

            public int TotalItems => _totalItems;

            private void OnProgressChanged(ProgressInfo progressInfo)
            {
                Interlocked.Exchange(ref _completedItems, progressInfo.CompletedItems);
                Interlocked.Exchange(ref _totalItems, progressInfo.TotalItems);
                _context.OnScopeProgressChanged(this);
            }

            public void Dispose()
            {
                _context.OnScopeDisposed(this);
            }
        }
    }
}
