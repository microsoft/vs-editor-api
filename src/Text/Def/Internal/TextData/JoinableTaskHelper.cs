//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
// This file contain internal APIs that are subject to change without notice.
// Use at your own risk.
//
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// A helper for managing JoinableTasks.
    /// </summary>
    public class JoinableTaskHelper
    {
        public readonly JoinableTaskContext Context;
        public readonly JoinableTaskCollection Collection;
        public readonly JoinableTaskFactory Factory;

        public JoinableTaskHelper(JoinableTaskContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            this.Context = context;
            this.Collection = context.CreateCollection();
            this.Factory = context.CreateFactory(this.Collection);
        }

        public JoinableTask RunOnUIThread(Action action, bool forceTaskSwitch = true)
        {
            using (this.Context.SuppressRelevance())
            {
                return this.Factory.RunAsync(async delegate
                                    {
                                        if (forceTaskSwitch && this.Context.IsOnMainThread)
                                        {
                                            await Task.Yield();
                                        }

                                        await this.Factory.SwitchToMainThreadAsync();
                                        action();
                                    });
            }
        }

        public JoinableTask<T> RunOnUIThread<T>(Func<T> function, bool forceTaskSwitch = true)
        {
            using (this.Context.SuppressRelevance())
            {
                return this.Factory.RunAsync(async delegate
                                    {
                                        if (forceTaskSwitch && this.Context.IsOnMainThread)
                                        {
                                            await Task.Yield();
                                        }

                                        await this.Factory.SwitchToMainThreadAsync();
                                        return function();
                                    });
            }
        }

        public async Task DisposeAsync()
        {
            await this.Collection.JoinTillEmptyAsync();
        }

        public void Dispose()
        {
            this.Context.Factory.Run(async delegate {   // Not this.Factory
                await this.DisposeAsync();
            });
        }
    }
}

