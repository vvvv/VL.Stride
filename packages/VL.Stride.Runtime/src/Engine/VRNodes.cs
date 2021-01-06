using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
using Stride.Rendering.Compositing;
using Stride.VirtualReality;
using System;
using System.Collections.Generic;
using VL.Core;
using VL.Lib.Basics.Resources;
using VL.Lib.Collections;

namespace VL.Stride.Engine
{
    static class VRNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {

            var vrCategory = "Stride.Experimental.VirtualReality";
            var physicsColliderShapesCategory = $"{vrCategory}.ColliderShapes";

            yield return new StrideNodeDesc<VRRendererSettings>(factory, category: vrCategory);
            yield return new StrideNodeDesc<VRDeviceDescription>(factory, category: vrCategory);
            yield return new StrideNodeDesc<VROverlayRenderer>(factory, category: vrCategory);

            yield return factory.NewNode(name: "VRDevice", category: vrCategory, copyOnWrite: false, hasStateOutput: false, ctor: n => new VRDeviceSplitter(n))
                .AddOutput(nameof(VRDevice.State), x => x.v.Device?.State ?? DeviceState.Invalid)
                .AddOutput(nameof(VRDevice.LeftHand), x => x.v.Device?.LeftHand)
                .AddOutput(nameof(VRDevice.RightHand), x => x.v.Device?.RightHand)
                .AddOutput(nameof(VRDevice.HeadPosition), x => x.v.Device?.HeadPosition ?? Vector3.Zero)
                .AddOutput(nameof(VRDevice.HeadRotation), x => x.v.Device?.HeadRotation ?? Quaternion.Identity)
                .AddOutput(nameof(VRDevice.HeadLinearVelocity), x => x.v.Device?.HeadLinearVelocity ?? Vector3.Zero)
                .AddOutput(nameof(VRDevice.HeadAngularVelocity), x => x.v.Device?.HeadAngularVelocity ?? Vector3.Zero)
                .AddOutput(nameof(VRDevice.MirrorTexture), x => x.v.Device?.MirrorTexture)
                .AddOutput(nameof(VRDevice.VRApi), x => x.v.Device?.VRApi ?? VRApi.Dummy)
                .AddOutput(nameof(VRDevice.TrackedItems), x => x.TrackedItems)
                .AddOutput("VR Device System", x => x.v)
                ;

            yield return factory.NewSplitNode<TouchController>(vrCategory)
                .AddOutput(nameof(TouchController.State), x => x.v?.State ?? DeviceState.Invalid)
                .AddOutput(nameof(TouchController.Position), x => x.v?.Position ?? Vector3.Zero)
                .AddOutput(nameof(TouchController.Rotation), x => x.v?.Rotation ?? Quaternion.Identity)
                .AddOutput(nameof(TouchController.LinearVelocity), x => x.v?.LinearVelocity ?? Vector3.Zero)
                .AddOutput(nameof(TouchController.AngularVelocity), x => x.v?.AngularVelocity ?? Vector3.Zero)
                ;

            yield return factory.NewSplitNode<TrackedItem>(vrCategory)
                .AddOutput(nameof(TrackedItem.State), x => x.v?.State ?? DeviceState.Invalid)
                .AddOutput(nameof(TrackedItem.Position), x => x.v?.Position ?? Vector3.Zero)
                .AddOutput(nameof(TrackedItem.Rotation), x => x.v?.Rotation ?? Quaternion.Identity)
                ;
        }

        static CustomNodeDesc<Splitter<T>> NewSplitNode<T>(this IVLNodeDescriptionFactory factory, string category, string name = default)
        {
            return factory.NewNode(name: name ?? typeof(T).Name, category: category, copyOnWrite: false, hasStateOutput: false, ctor: _ => new Splitter<T>())
                .AddInput("Input", x => x.v, (x, v) => x.v = v);
        }

        class Splitter<T>
        {
            public T v;
        }

        class VRDeviceSplitter : IDisposable
        {
            private IResourceHandle<Game> gameHandle;
            private Spread<TrackedItem> trackedItems = Spread<TrackedItem>.Empty;
            private TrackedItem[] lastTrackedItems;
            public VRDeviceSystem v;

            public VRDeviceSplitter(NodeContext nodeContext)
            {
                gameHandle = nodeContext.GetGameHandle();
                v = gameHandle.Resource.Services.GetService<VRDeviceSystem>();
            }

            public Spread<TrackedItem> TrackedItems
            {
                get
                {
                    var items = v.Device?.TrackedItems;
                    if (items != lastTrackedItems)
                    {
                        trackedItems = Spread.Create(items);
                        lastTrackedItems = items;
                    }

                    return trackedItems;
                }
            }

            public void Dispose()
            {
                gameHandle?.Dispose();
            }
        }

    }
}