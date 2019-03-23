// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace System.Windows.Threading
{
    sealed class DispatcherOperationTaskSource
    {
        readonly TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

        public Task GetTask()
            => taskCompletionSource.Task;

        public void SetCanceled()
            => taskCompletionSource.SetCanceled();

        public void SetResult()
            => taskCompletionSource.SetResult(true);

        public void SetException(Exception exception)
            => taskCompletionSource.SetException(exception);
    }
}