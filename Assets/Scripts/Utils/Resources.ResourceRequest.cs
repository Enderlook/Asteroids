using System;

using UnityEngine;

using UnityObject = UnityEngine.Object;
using UnityResourceRequest = UnityEngine.ResourceRequest;

namespace Asteroids.Utils
{
    public static partial class Resources
    {
        /// <summary>
        /// Represent a proxy for <see cref="UnityResourceRequest"/>.
        /// </summary>
        /// <typeparam name="T">Type of asset.</typeparam>
        public class ResourceRequest<T> : CustomYieldInstruction
            where T : UnityObject
        {
            // TODO: If C# 10 discriminated unions are released, use them.

            private T assetInner;

            private UnityResourceRequest request;

            private Action<ResourceRequest<T>> completedInner;

            internal ResourceRequest(T asset) => assetInner = asset;

            internal ResourceRequest(UnityResourceRequest request)
            {
                this.request = request;
                request.completed += (_) =>
                {
                    LookupTable<T>.MoveFromAsyncToSync((string)GetPathFromResourceRequest(this.request), asset);
                    completedInner(this);
                };
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "We are mirroring a UnityEngine.ResourceRequest type.")]
            public T asset {
                get {
                    if (assetInner is null)
                    {
                        if (request is null)
                            return null;
                        else
                        {
                            assetInner = Load<T>((string)GetPathFromResourceRequest(request));
                            return assetInner;
                        }
                    }
                    return assetInner;
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "We are mirroring a UnityEngine.ResourceRequest type.")]
            public bool isDone => !(assetInner is null) || !(request is null) || request.isDone;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "We are mirroring a UnityEngine.ResourceRequest type.")]
            public int priority {
                get => assetInner is null ? 0 : ((request?.priority) ?? 0);
                set {
                    if (!(request is null))
                        request.priority = value;
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "We are mirroring a UnityEngine.ResourceRequest type.")]
            public float progress => assetInner is null ? (request?.progress ?? 1) : 1;

            public override bool keepWaiting => !isDone;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "We are mirroring a UnityEngine.ResourceRequest type.")]
            public event Action<ResourceRequest<T>> completed {
                add {
                    if (assetInner is null)
                    {
                        if (request is null)
                            value(this);
                        else
                        {
                            if (completedInner is null)
                            {
                                completedInner = value;
                                request.completed += (_) => completedInner(this);
                            }
                            else
                                completedInner += value;
                        }
                    }
                    value(this);
                }
                remove => completedInner -= value;
            }
        }
    }
}