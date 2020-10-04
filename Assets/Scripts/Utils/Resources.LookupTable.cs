using System;
using System.Collections.Generic;

using UnityObject = UnityEngine.Object;
using UnityResources = UnityEngine.Resources;

namespace Asteroids.Utils
{
    public static partial class Resources
    {
        private static class LookupTable<T>
            where T : UnityObject
        {
            private static WeakReference<T[]> all = new WeakReference<T[]>(null);

            /*
             * Don't store all tables in the same dictionary because the common case would require an additional math offset at assembly level to load the correct tuple element.
             * This method moves the additional cost from the average case to the worst one.
             */

            private static Dictionary<string, WeakReference<T>> table = new Dictionary<string, WeakReference<T>>();

            private static Dictionary<string, ResourceRequest<T>> tableAsync = new Dictionary<string, ResourceRequest<T>>();

            private static Dictionary<string, WeakReference<T[]>> tableAll = new Dictionary<string, WeakReference<T[]>>();

            static LookupTable() => unloadCache += RemoveDeathReferences;

            public static void MoveFromAsyncToSync(string path, T asset)
            {
                tableAsync.Remove(path);
                if (table.TryGetValue(path, out WeakReference<T> reference))
                    reference.SetTarget(asset);
                else
                    table[path] = new WeakReference<T>(asset);
            }

            public static T Load(string path)
            {
                if (table.TryGetValue(path, out WeakReference<T> reference))
                {
                    if (!reference.TryGetTarget(out T value))
                    {
                        value = UnityResources.Load<T>(path);
                        reference.SetTarget(value);
                    }
                    return value;
                }
                else
                {
                    T value = UnityResources.Load<T>(path);
                    table[path] = new WeakReference<T>(value);
                    return value;
                }
            }

            public static ResourceRequest<T> LoadAsync(string path)
            {
                if (table.TryGetValue(path, out WeakReference<T> reference))
                {
                    if (reference.TryGetTarget(out T value))
                        return new ResourceRequest<T>(value);
                    else
                    {
                        if (tableAsync.TryGetValue(path, out ResourceRequest<T> request))
                            return request;
                        else
                        {
                            request = new ResourceRequest<T>(UnityResources.LoadAsync<T>(path));
                            tableAsync[path] = request;
                            return request;
                        }
                    }
                }
                else
                {
                    if (tableAsync.TryGetValue(path, out ResourceRequest<T> request))
                        return request;
                    else
                    {
                        request = new ResourceRequest<T>(UnityResources.LoadAsync<T>(path));
                        tableAsync[path] = request;
                        return request;
                    }
                }
            }

            public static IReadOnlyList<T> LoadAll(string path)
            {
                if (!tableAll.TryGetValue(path, out WeakReference<T[]> reference))
                {
                    if (!reference.TryGetTarget(out T[] value))
                    {
                        value = UnityResources.LoadAll<T>(path);
                        reference.SetTarget(value);
                    }
                    return value;
                }
                else
                {
                    T[] value = UnityResources.LoadAll<T>(path);
                    tableAll[path] = new WeakReference<T[]>(value);
                    return value;
                }
            }

            public static IReadOnlyList<T> FindAllObjects()
            {
                if (!all.TryGetTarget(out T[] value))
                {
                    value = UnityResources.FindObjectsOfTypeAll<T>();
                    all.SetTarget(value);
                }
                return value;
            }

            public static void RemoveDeathReferences()
            {
                foreach ((string path, WeakReference<T> value) in table)
                {
                    if (!value.TryGetTarget(out T _))
                        table.Remove(path);
                }

                foreach ((string path, WeakReference<T[]> value) in tableAll)
                {
                    if (!value.TryGetTarget(out T[] _))
                        tableAll.Remove(path);
                }
            }
        }
    }
}