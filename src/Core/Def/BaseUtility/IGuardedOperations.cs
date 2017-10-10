//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// Operations that guard calls to extensions code and log errors.
    /// </summary>
    /// <remarks>This class supports the Visual Studio 
    /// infrastructure and in general is not intended to be used directly from your code.</remarks>
    public interface IGuardedOperations
    {
        /// <summary>
        /// Makes a guarded call to an extension point.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        void CallExtensionPoint(Action call);

        /// <summary>
        /// Makes a guarded call to an extension point.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        void CallExtensionPoint(object errorSource, Action call);

        /// <summary>
        /// Makes a guarded call to an extension point.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        T CallExtensionPoint<T>(Func<T> call, T valueOnThrow);

        /// <summary>
        /// Makes a guarded call to an extension point.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        T CallExtensionPoint<T>(object errorSource, Func<T> call, T valueOnThrow);

        /// <summary>
        /// Makes a guarded call to an async extension point.
        /// </summary>
        /// <param name="asyncAction">The extension point to be called.</param>
        /// <returns>A <see cref="Task"/> that asynchronously executes the <paramref name="asyncAction"/>.</returns>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        Task CallExtensionPointAsync(Func<Task> asyncAction);

        /// <summary>
        /// Makes a guarded call to an async extension point.
        /// </summary>
        /// <param name="asyncAction">The extension point to be called.</param>
        /// <returns>A <see cref="Task"/> that asynchronously executes the <paramref name="asyncAction"/>.</returns>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        Task CallExtensionPointAsync(object errorSource, Func<Task> asyncAction);

        /// <summary>
        /// Makes a guarded call to an async extension point.
        /// </summary>
        /// <typeparam name="T">The type of the value returned from the <paramref name="asyncCall"/>.</typeparam>
        /// <param name="asyncCall">The extension point to be called.</param>
        /// <param name="valueOnThrow">The value returned if call failed.</param>
        /// <returns>A <see cref="Task{T}"/> that asynchronously executes the <paramref name="asyncCall"/>.</returns>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        Task<T> CallExtensionPointAsync<T>(Func<Task<T>> asyncCall, T valueOnThrow);

        /// <summary>
        /// Makes a guarded call to an async extension point.
        /// </summary>
        /// <typeparam name="T">The type of the value returned from the <paramref name="asyncCall"/>.</typeparam>
        /// <param name="asyncCall">The extension point to be called.</param>
        /// <param name="valueOnThrow">The value returned if call failed.</param>
        /// <returns>A <see cref="Task{T}"/> that asynchronously executes the <paramref name="asyncCall"/>.</returns>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        Task<T> CallExtensionPointAsync<T>(object errorSource, Func<Task<T>> asyncCall, T valueOnThrow);

        /// <summary>
        /// Selects eligible extension factories.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        IEnumerable<Lazy<TExtensionFactory, TMetadataView>> FindEligibleFactories<TExtensionFactory, TMetadataView>(IEnumerable<Lazy<TExtensionFactory, TMetadataView>> lazyFactories, IContentType dataContentType, IContentTypeRegistryService contentTypeRegistryService)
            where TExtensionFactory : class
            where TMetadataView : INamedContentTypeMetadata;

        /// <summary>
        /// Handles an exception occured in a call to an extension point.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        void HandleException(object errorSource, Exception e);

        /// <summary>
        /// Safely instantiates an extension point.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        TExtension InstantiateExtension<TExtension>(object errorSource, Lazy<TExtension> provider);

        /// <summary>
        /// Safely instantiates an extension point.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        TExtension InstantiateExtension<TExtension, TMetadata>(object errorSource, Lazy<TExtension, TMetadata> provider);

        /// <summary>
        /// Safely instantiates an extension point.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        TExtensionInstance InstantiateExtension<TExtension, TMetadata, TExtensionInstance>(object errorSource, Lazy<TExtension, TMetadata> provider, Func<TExtension, TExtensionInstance> getter);

        /// <summary>
        /// Safely invokes best matching extension factory.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        TExtension InvokeBestMatchingFactory<TExtension, TMetadataView>(IList<Lazy<TExtension, TMetadataView>> providerHandles, IContentType dataContentType, IContentTypeRegistryService contentTypeRegistryService, object errorSource) where TMetadataView : IContentTypeMetadata;

        /// <summary>
        /// Safely invokes best matching extension factory.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        TExtensionInstance InvokeBestMatchingFactory<TExtensionFactory, TExtensionInstance, TMetadataView>(IList<Lazy<TExtensionFactory, TMetadataView>> providerHandles, IContentType dataContentType, Func<TExtensionFactory, TExtensionInstance> getter, IContentTypeRegistryService contentTypeRegistryService, object errorSource)
            where TExtensionFactory : class
            where TMetadataView : IContentTypeMetadata;

        /// <summary>
        /// Safely invokes all eligible extension factories.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        List<TExtensionInstance> InvokeEligibleFactories<TExtensionInstance, TExtensionFactory, TMetadataView>(IEnumerable<Lazy<TExtensionFactory, TMetadataView>> lazyFactories, Func<TExtensionFactory, TExtensionInstance> getter, IContentType dataContentType, IContentTypeRegistryService contentTypeRegistryService, object errorSource)
            where TExtensionInstance : class
            where TExtensionFactory : class
            where TMetadataView : INamedContentTypeMetadata;

        /// <summary>
        /// Safely invokes all matching extension factories.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        List<TExtensionInstance> InvokeMatchingFactories<TExtensionInstance, TExtensionFactory, TMetadataView>(IEnumerable<Lazy<TExtensionFactory, TMetadataView>> lazyFactories, Func<TExtensionFactory, TExtensionInstance> getter, IContentType dataContentType, object errorSource)
            where TExtensionInstance : class
            where TExtensionFactory : class
            where TMetadataView : IContentTypeMetadata;

        /// <summary>
        /// Safely raises an event.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        void RaiseEvent(object sender, EventHandler eventHandlers);

        /// <summary>
        /// Safely raises an event.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        void RaiseEvent<TArgs>(object sender, EventHandler<TArgs> eventHandlers, TArgs args) where TArgs : EventArgs;

        /// <summary>
        /// Safely raises an event on a background thread.
        /// </summary>
        /// <remarks>This class supports the Visual Studio 
        /// infrastructure and in general is not intended to be used directly from your code.</remarks>
        Task RaiseEventOnBackgroundAsync<TArgs>(object sender, AsyncEventHandler<TArgs> eventHandlers, TArgs args) where TArgs : EventArgs;
    }
}
