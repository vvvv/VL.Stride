using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics.GeometricPrimitives;
using Stride.Rendering;
using Stride.Rendering.Compositing;
using Stride.Rendering.ProceduralModels;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using VL.Core;
using VL.Lib.Basics.Resources;
using VL.Stride.Input;
using VL.Stride.Rendering.Compositing;
using VL.Stride.Scripts;

namespace VL.Stride.Engine
{
    static class EngineNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            var strideCategory = "Stride";
            var strideCategoryAdvanced = "Stride.Advanced";

            yield return new CustomNodeDesc<SceneInstanceSystem>(factory,
                ctor: nodeContext =>
                {
                    var gameHandle = nodeContext.GetGameHandle();
                    var game = gameHandle.Resource;
                    var instance = new SceneInstanceSystem(game.Services);
                    return (instance, () => gameHandle.Dispose());
                },
                category: strideCategoryAdvanced,
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
                category: strideCategoryAdvanced,
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

            // Camera components
            var camerasCategory = "Stride.Cameras";

            yield return factory.NewComponentNode<CameraComponent>(camerasCategory, cam => cam.Slot = new SceneCameraSlotId(new Guid()))
                .AddInput(nameof(CameraComponent.UseCustomViewMatrix), x => x.UseCustomViewMatrix, (x, v) => x.UseCustomViewMatrix = v)
                .AddInput(nameof(CameraComponent.ViewMatrix), x => x.ViewMatrix, (x, v) => x.ViewMatrix = v)
                .AddInput(nameof(CameraComponent.Projection), x => x.Projection, (x, v) => x.Projection = v)
                .AddInput(nameof(CameraComponent.VerticalFieldOfView), x => (double)x.VerticalFieldOfView / 360, (x, v) => x.VerticalFieldOfView = (float)(v * 360), 45d / 360, summary: "Gets or sets the vertical field of view in cycles.", remarks: "The vertical field of view (in cycles)")
                .AddInput(nameof(CameraComponent.UseCustomAspectRatio), x => x.UseCustomAspectRatio, (x, v) => x.UseCustomAspectRatio = v)
                .AddInput(nameof(CameraComponent.AspectRatio), x => x.AspectRatio, (x, v) => x.AspectRatio = v, 1f)
                .AddInput(nameof(CameraComponent.NearClipPlane), x => x.NearClipPlane, (x, v) => x.NearClipPlane = v)
                .AddInput(nameof(CameraComponent.FarClipPlane), x => x.FarClipPlane, (x, v) => x.FarClipPlane = v)
                .AddInput(nameof(CameraComponent.UseCustomProjectionMatrix), x => x.UseCustomProjectionMatrix, (x, v) => x.UseCustomProjectionMatrix = v)
                .AddInput(nameof(CameraComponent.ProjectionMatrix), x => x.ProjectionMatrix, (x, v) => x.ProjectionMatrix = v)
                .WithEnabledPin();

            // The CameraRenderer needs to be here due to dependency on CameraComponent
            var renderingCategory = "Stride.Rendering";
            var renderingCategoryAdvanced = $"{renderingCategory}.Advanced";
            var compositionCategory = $"{renderingCategoryAdvanced}.Compositing";
            var cameraComponent = new CameraComponent() { ViewMatrix = Matrix.Translation(0, 0, -2), UseCustomViewMatrix = true };
            yield return factory.NewGraphicsRendererNode<SceneExternalCameraRenderer>(name: "CameraRenderer", category: compositionCategory)
                .AddInput("Camera", x => x.ExternalCamera ?? cameraComponent, (x, v) => x.ExternalCamera = v)
                .AddInput(nameof(SceneExternalCameraRenderer.Child), x => x.Child, (x, v) => x.Child = v)
                .AddEnabledPin();

            // Model components
            var modelsCategory = "Stride.Models";

            yield return factory.NewComponentNode<ModelComponent>(modelsCategory)
                .AddInput(nameof(ModelComponent.Model), x => x.Model, (x, v) => x.Model = v)
                .AddInput(nameof(ModelComponent.RenderGroup), x => x.RenderGroup, (x, v) => x.RenderGroup = v)
                .AddInput(nameof(ModelComponent.IsShadowCaster), x => x.IsShadowCaster, (x, v) => x.IsShadowCaster = v, true)
                .AddListInput(nameof(ModelComponent.Materials), x => x.Materials)
                .WithEnabledPin();


            // Input components
            var inputCategory = "Stride.Input";

            yield return factory.NewComponentNode<InputSourceComponent>(inputCategory)
                .AddInput(nameof(InputSourceComponent.Enabled), x => x.Enabled, (x, v) => x.Enabled = v, true)
                .AddOutput(nameof(InputSourceComponent.InputSource), c => c.InputSource);

            yield return factory.NewComponentNode<CameraInputSourceComponent>(inputCategory)
                .AddInput(nameof(CameraInputSourceComponent.Enabled), x => x.Enabled, (x, v) => x.Enabled = v, true)
                .AddOutput(nameof(CameraInputSourceComponent.InputSource), c => c.InputSource);

            yield return factory.NewNode<CameraInputSourceSceneRenderer>(name: nameof(CameraInputSourceSceneRenderer), category: inputCategory)
                .AddInput(nameof(CameraInputSourceSceneRenderer.CameraInputSourceComponent), x => x.CameraInputSourceComponent, (x, v) => x.CameraInputSourceComponent = v);

            
            // Patchable script
            yield return factory.NewComponentNode<InterfaceSyncScript>(strideCategory, name: "PatchScriptComponent")
                .AddInput(nameof(InterfaceSyncScript.PatchScript), x => x.PatchScript, (x, v) => x.PatchScript = v)
                ;

        }
    }
}