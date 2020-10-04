using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using UnityEngine;

using UnityObject = UnityEngine.Object;
using UnityResources = UnityEngine.Resources;
using UnityResourceRequest = UnityEngine.ResourceRequest;

namespace Asteroids.Utils
{
    /// <summary>
    /// Represents a proxy for <see cref="Resources"/> implementing a caching layer.
    /// It's almost a drop-in replacement for <see cref="Resources"/>.
    /// </summary>
    public static partial class Resources
    {
        /// <summary>
        /// Store the params array.
        /// </summary>
        private static Type[] parameters = new Type[1];

        private static Action unloadCache;

        private static Action<AsyncOperation> unloadCacheAsync = (_) => unloadCache();

        // Don't store all tables in the same dictionary because the common case would require an additional math offset at assembly level to load the correct tuple element..

        /// <summary>
        /// Stores a delegate of <see cref="LookupTable{T}.Load(string)"/> for each <see cref="Type"/>.<br/>
        /// This is used for <see cref="Load(string, Type)"/>.
        /// </summary>
        private static Dictionary<Type, Func<string, UnityObject>> erasedLoadTable = new Dictionary<Type, Func<string, UnityObject>>();

        /// <summary>
        /// Stores a delegate of <see cref="LookupTable{T}.LoadAsync(string)"/> for each <see cref="Type"/>.<br/>
        /// This is used for <see cref="LoadAsync(string, Type)"/>.
        /// </summary>
        private static Dictionary<Type, Func<string, Task>> erasedLoadAsyncTable = new Dictionary<Type, Func<string, Task>>();

        /// <summary>
        /// Stores a delegate of <see cref="LookupTable{T}.LoadAll(string)"/> for each <see cref="Type"/>.<br/>
        /// This is used for <see cref="LoadAll(string, Type)"/>.
        /// </summary>
        private static Dictionary<Type, Func<string, IReadOnlyList<UnityObject>>> erasedLoadAllTable = new Dictionary<Type, Func<string, IReadOnlyList<UnityObject>>>();

        /// <summary>
        /// Stores a delegate of <see cref="LookupTable{T}.FindAllObjects"/> for each <see cref="Type"/>.<br/>
        /// This is used for <see cref="FindObjectsOfTypeAll(Type)(string, Type)"/>.
        /// </summary>
        private static Dictionary<Type, Func<IReadOnlyList<UnityObject>>> erasedFindAllTable = new Dictionary<Type, Func<IReadOnlyList<UnityObject>>>();

        /// <summary>
        /// Method used to inspect <see cref="UnityResourceRequest"/>.
        /// </summary>
        private static Func<object, object> GetPathFromResourceRequest = typeof(UnityResourceRequest).GetField("m_Path", BindingFlags.Instance | BindingFlags.NonPublic).GetValue;

        /// <summary>
        /// Method used to inspect <see cref="UnityResourceRequest"/>.
        /// </summary>
        private static Func<object, object> GetTypeFromResourceRequest = typeof(UnityResourceRequest).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic).GetValue;

        /// <inheritdoc cref="ResourceRequest.asset"/>
        public static UnityObject GetAsset(this ResourceRequest request) => Load((string)GetPathFromResourceRequest(request), (Type)GetTypeFromResourceRequest(request));

        /// <inheritdoc cref="UnityResources.Load{T}(string)"/>
        public static T Load<T>(string path) where T : UnityObject => LookupTable<T>.Load(path);

        /// <inheritdoc cref="UnityResources.Load(string)"/>
        public static UnityObject Load(string path) => LookupTable<UnityObject>.Load(path);

        /// <inheritdoc cref="UnityResources.Load(string, Type)"/>
        /// <remarks>The lookup table was optimized to use the generic method <see cref="Load{T}(string)"/>.</remarks>
        public static UnityObject Load(string path, Type type) => GetLoader(type)(path);

        /// <inheritdoc cref="UnityResources.LoadAsync{T}(string)"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "We are mirroring a UnityEngine.Resource type.")]
        public static ResourceRequest<T> LoadAsync<T>(string path) where T : UnityObject => LookupTable<T>.LoadAsync(path);

        /// <inheritdoc cref="UnityResources.LoadAsync(string)"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "We are mirroring a UnityEngine.Resource type.")]
        public static ResourceRequest<UnityObject> LoadAsync(string path) => LookupTable<UnityObject>.LoadAsync(path);

        /// <inheritdoc cref="UnityResources.LoadAsync(string, Type)"/>
        /// <remarks>The lookup table was optimized to use the generic method <see cref="LoadAsync{T}(string)"/>.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "We are mirroring a UnityEngine.Resource type.")]
        public static Task LoadAsync(string path, Type type) => GetLoaderAsync(type)(path);

        /// <inheritdoc cref="UnityResources.FindObjectsOfTypeAll{T}"/>
        public static IReadOnlyList<T> FindObjectsOfTypeAll<T>() where T : UnityObject => LookupTable<T>.FindAllObjects();

        /// <inheritdoc cref="UnityResources.FindObjectsOfTypeAll(Type)"/>
        /// <remarks>The lookup table was optimized to use the generic method instead <see cref="FindObjectsOfTypeAll{T})"/>.</remarks>
        public static IReadOnlyList<UnityObject> FindObjectsOfTypeAll(Type type) => GetFindAll(type)();

        /// <inheritdoc cref="UnityResources.LoadAll{T}(string)"/>
        public static IReadOnlyList<T> LoadAll<T>(string path) where T : UnityObject => LookupTable<T>.LoadAll(path);

        /// <inheritdoc cref="UnityResources.LoadAll(string)"/>
        public static IReadOnlyList<UnityObject> LoadAll(string path) => LookupTable<UnityObject>.LoadAll(path);

        /// <inheritdoc cref="UnityResources.LoadAll(string, Type)"/>
        /// <remarks>The lookup table was optimized to use the generic method <see cref="LoadAll{T}(string)"/>.</remarks>
        public static IReadOnlyList<UnityObject> LoadAll(string path, Type type) => GetLoaderAll(type)(path);

        /// <inheritdoc cref="UnityResources.UnloadAsset(UnityObject)"/>
        public static void UnloadAsset(UnityObject assetToUnload) => UnityResources.UnloadAsset(assetToUnload);

        /// <inheritdoc cref="UnityResources.UnloadUnusedAssets()"/>
        public static AsyncOperation UnloadUnusedAssets()
        {
            AsyncOperation operation = UnityResources.UnloadUnusedAssets();
            operation.completed += unloadCacheAsync;
            return operation;
        }

        private static Func<string, UnityObject> GetLoader(Type type)
        {
            if (!erasedLoadTable.TryGetValue(type, out Func<string, UnityObject> @delegate))
            {
                parameters[0] = type;
                erasedLoadTable[type] = (Func<string, UnityObject>)typeof(LookupTable<>).MakeGenericType(parameters).GetMethod(nameof(LookupTable<UnityObject>.Load), BindingFlags.Static | BindingFlags.Public).CreateDelegate(typeof(Func<string, UnityObject>));
            }
            return @delegate;
        }

        private static Func<string, Task> GetLoaderAsync(Type type)
        {
            if (!erasedLoadAsyncTable.TryGetValue(type, out Func<string, Task> @delegate))
            {
                parameters[0] = type;
                erasedLoadAsyncTable[type] = (Func<string, Task>)typeof(LookupTable<>).MakeGenericType(parameters).GetMethod(nameof(LookupTable<UnityObject>.LoadAsync), BindingFlags.Static | BindingFlags.Public).CreateDelegate(typeof(Func<string, Task>));
            }
            return @delegate;
        }

        private static Func<string, IReadOnlyList<UnityObject>> GetLoaderAll(Type type)
        {
            if (!erasedLoadAllTable.TryGetValue(type, out Func<string, IReadOnlyList<UnityObject>> @delegate))
            {
                parameters[0] = type;
                erasedLoadAllTable[type] = (Func<string, IReadOnlyList<UnityObject>>)typeof(LookupTable<>).MakeGenericType(parameters).GetMethod(nameof(LookupTable<UnityObject>.LoadAll), BindingFlags.Static | BindingFlags.Public).CreateDelegate(typeof(Func<string, IReadOnlyList<UnityObject>>));
            }
            return @delegate;
        }

        private static Func<IReadOnlyList<UnityObject>> GetFindAll(Type type)
        {
            if (!erasedFindAllTable.TryGetValue(type, out Func<IReadOnlyList<UnityObject>> @delegate))
            {
                parameters[0] = type;
                erasedFindAllTable[type] = (Func<IReadOnlyList<UnityObject>>)typeof(LookupTable<>).MakeGenericType(parameters).GetMethod(nameof(LookupTable<UnityObject>.FindAllObjects), BindingFlags.Static | BindingFlags.Public).CreateDelegate(typeof(Func<IReadOnlyList<UnityObject>>));
            }
            return @delegate;
        }
    }
}