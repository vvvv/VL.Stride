using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics.GeometricPrimitives;
using Stride.Rendering;
using Stride.Rendering.ProceduralModels;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using VL.Core;
using VL.Lib.Basics.Resources;
using VL.Stride.Input;

namespace VL.Stride.Engine
{
    using Model = global::Stride.Rendering.Model;

    static class EngineNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            yield return new CustomNodeDesc<SceneInstanceSystem>(factory,
                ctor: nodeContext =>
                {
                    var gameHandle = nodeContext.GetGameHandle();
                    var game = gameHandle.Resource;
                    var instance = new SceneInstanceSystem(game.Services);
                    return (instance, () => gameHandle.Dispose());
                },
                category: "Stride",
                copyOnWrite: false)
                .AddInput(nameof(SceneInstanceSystem.RootScene), x => x.RootScene, (x, v) => x.RootScene = v)
                .AddOutput(nameof(SceneInstanceSystem.SceneInstance), x => x.SceneInstance);

            yield return new CustomNodeDesc<SceneInstanceRenderer>(factory,
                ctor: nodeContext =>
                {
                    var gameHandle = nodeContext.GetGameHandle();
                    var game = gameHandle.Resource;
                    var instance = new SceneInstanceRenderer();
                    return (instance, () => gameHandle.Dispose());
                },
                category: "Stride",
                copyOnWrite: false)
                .AddInput(nameof(SceneInstanceRenderer.SceneInstance), x => x.SceneInstance, (x, v) => x.SceneInstance = v)
                .AddInput(nameof(SceneInstanceRenderer.GraphicsCompositor), x => x.GraphicsCompositor, (x, v) => x.GraphicsCompositor = v);

            // Light components
            var lightsCategory = "Stride.Lights";

            yield return factory.NewComponentNode<LightComponent>(lightsCategory)
                .AddInput(nameof(LightComponent.Type), x => x.Type, (x, v) => x.Type = v)
                .AddInput(nameof(LightComponent.Intensity), x => x.Intensity, (x, v) => x.Intensity = v, 1f)
                .WithEnabledPin();

            yield return factory.NewComponentNode<LightShaftComponent>(lightsCategory)
                .AddInput(nameof(LightShaftComponent.DensityFactor), x => x.DensityFactor, (x, v) => x.DensityFactor = v, 0.002f)
                .AddInput(nameof(LightShaftComponent.SampleCount), x => x.SampleCount, (x, v) => x.SampleCount = v, 16)
                .AddInput(nameof(LightShaftComponent.SeparateBoundingVolumes), x => x.SeparateBoundingVolumes, (x, v) => x.SeparateBoundingVolumes = v, true)
                .WithEnabledPin();

            yield return factory.NewComponentNode<LightShaftBoundingVolumeComponent>(lightsCategory)
                .AddInput(nameof(LightShaftBoundingVolumeComponent.Model), x => x.Model, (x, v) => x.Model = v) // Ensure to check for change! Property throws event!
                .AddInput(nameof(LightShaftBoundingVolumeComponent.LightShaft), x => x.LightShaft, (x, v) => x.LightShaft = v) // Ensure to check for change! Property throws event!
                .WithEnabledPin();

            // Model components
            var modelsCategory = "Stride.Models";

            yield return factory.NewComponentNode<ModelComponent>(modelsCategory)
                .AddInput(nameof(ModelComponent.Model), x => x.Model, (x, v) => x.Model = v)
                .AddInput(nameof(ModelComponent.RenderGroup), x => x.RenderGroup, (x, v) => x.RenderGroup = v)
                .AddInput(nameof(ModelComponent.IsShadowCaster), x => x.IsShadowCaster, (x, v) => x.IsShadowCaster = v, true)
                .AddListInput(nameof(ModelComponent.Materials), x => x.Materials)
                .WithEnabledPin();

            // Meshes
            yield return factory.NewMeshNode((CapsuleProceduralModel x) => (x.Length, x.Radius, x.Tessellation))
                .AddInput(nameof(CapsuleProceduralModel.Length), x => x.Length, (x, v) => x.Length = v, 0.5f)
                .AddInput(nameof(CapsuleProceduralModel.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddInput(nameof(CapsuleProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((ConeProceduralModel x) => (x.Height, x.Radius, x.Tessellation))
                .AddInput(nameof(ConeProceduralModel.Height), x => x.Height, (x, v) => x.Height = v, 1f)
                .AddInput(nameof(ConeProceduralModel.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddInput(nameof(ConeProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((CubeProceduralModel x) => x.Size)
                .AddInput(nameof(CubeProceduralModel.Size), x => x.Size, (x, v) => x.Size = v, Vector3.One)
                .AddDefaultPins();

            yield return factory.NewMeshNode((CylinderProceduralModel x) => (x.Height, x.Radius, x.Tessellation))
                .AddInput(nameof(CylinderProceduralModel.Height), x => x.Height, (x, v) => x.Height = v, 1f)
                .AddInput(nameof(CylinderProceduralModel.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddInput(nameof(CylinderProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((GeoSphereProceduralModel x) => (x.Radius, x.Tessellation))
                .AddInput(nameof(GeoSphereProceduralModel.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddInput(nameof(GeoSphereProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = VLMath.Clamp(v, 1, 5), 3)
                .AddDefaultPins();

            yield return factory.NewMeshNode((PlaneProceduralModel x) => (x.Size, x.Tessellation, x.Normal, x.GenerateBackFace))
                .AddInput(nameof(PlaneProceduralModel.Size), x => x.Size, (x, v) => x.Size = v, Vector2.One)
                .AddInput(nameof(PlaneProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v, Int2.One)
                .AddInput(nameof(PlaneProceduralModel.Normal), x => x.Normal, (x, v) => x.Normal = v, NormalDirection.UpZ)
                .AddInput(nameof(PlaneProceduralModel.GenerateBackFace), x => x.GenerateBackFace, (x, v) => x.GenerateBackFace = v, true)
                .AddDefaultPins();

            yield return factory.NewMeshNode((SphereProceduralModel x) => (x.Radius, x.Tessellation))
                .AddInput(nameof(SphereProceduralModel.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddInput(nameof(SphereProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((TeapotProceduralModel x) => (x.Size, x.Tessellation))
                .AddInput(nameof(TeapotProceduralModel.Size), x => x.Size, (x, v) => x.Size = v, 1f)
                .AddInput(nameof(TeapotProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((TorusProceduralModel x) => (x.Radius, x.Thickness, x.Tessellation))
                .AddInput(nameof(TorusProceduralModel.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddInput(nameof(TorusProceduralModel.Thickness), x => x.Thickness, (x, v) => x.Thickness = v, 0.25f)
                .AddInput(nameof(TorusProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v, 16)
                .AddDefaultPins();


            // Input components
            var inputCategory = "Stride.Input";

            yield return factory.NewComponentNode<InputSourceComponent>(inputCategory)
                .AddOutput(nameof(InputSourceComponent.InputSource), c => c.InputSource);
        }

        public static CustomNodeDesc<TProceduralModel> NewMeshNode<TProceduralModel, TKey>(this IVLNodeDescriptionFactory factory, Func<TProceduralModel, TKey> getKey)
            where TProceduralModel : PrimitiveProceduralModelBase, new()
        {
            return new CustomNodeDesc<TProceduralModel>(factory,
                name: typeof(TProceduralModel).Name.Replace("ProceduralModel", "Mesh"),
                category: "Stride.Models.Meshes",
                copyOnWrite: false,
                hasStateOutput: false,
                fragmented: true,
                ctor: nodeContext =>
                {
                    var generator = new TProceduralModel();
                    return (generator, default);
                })
                .AddCachedOutput("Output", nodeContext =>
                {
                    var disposable = new SerialDisposable();
                    Func<TProceduralModel, Mesh> getter = generator =>
                    {
                        var key = (typeof(TProceduralModel), generator.Scale, generator.UvScale, generator.LocalOffset, generator.NumberOfTextureCoordinates, getKey(generator));
                        var provider = ResourceProvider.NewPooledPerApp(nodeContext, key, _ =>
                        {
                            return nodeContext.GetGameProvider()
                                .Bind(game =>
                                {
                                    var model = new Model();
                                    generator.Generate(game.Services, model);
                                    return ResourceProvider.Return(model.Meshes[0], m =>
                                    {
                                        if (m.Draw != null)
                                        {
                                            m.Draw.IndexBuffer?.Buffer?.Dispose();
                                            foreach (var b in m.Draw.VertexBuffers)
                                                b.Buffer?.Dispose();
                                        }
                                    });
                                });
                        });
                        var meshHandle = provider.GetHandle();
                        disposable.Disposable = meshHandle;
                        return meshHandle.Resource;
                    };
                    return (getter, disposable);
                });
        }

        public static CustomNodeDesc<TProceduralModel> AddDefaultPins<TProceduralModel>(this CustomNodeDesc<TProceduralModel> node)
            where TProceduralModel : PrimitiveProceduralModelBase, new()
        {
            return node
                .AddInput(nameof(PrimitiveProceduralModelBase.Scale), x => x.Scale, (x, v) => x.Scale = v, Vector3.One)
                .AddInput(nameof(PrimitiveProceduralModelBase.UvScale), x => x.UvScale, (x, v) => x.UvScale = v, Vector2.One)
                .AddInput(nameof(PrimitiveProceduralModelBase.LocalOffset), x => x.LocalOffset, (x, v) => x.LocalOffset = v, Vector3.Zero)
                .AddInput(nameof(PrimitiveProceduralModelBase.NumberOfTextureCoordinates), x => x.NumberOfTextureCoordinates, (x, v) => x.NumberOfTextureCoordinates = v, 1);
        }
    }
}