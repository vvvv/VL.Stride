using Stride.Core.Mathematics;
using Stride.Graphics.GeometricPrimitives;
using Stride.Rendering;
using Stride.Rendering.ProceduralModels;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using VL.Core;
using VL.Lib.Basics.Resources;
using VL.Lib.Mathematics;
using VL.Stride.Rendering.ComputeEffect;

namespace VL.Stride.Rendering
{
    using Model = global::Stride.Rendering.Model;

    static class RenderingNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            var renderingCategory = "Stride.Rendering";
            var renderingAdvancedCategory = $"{renderingCategory}.Advanced";

            yield return NewInputRenderBaseNode<WithRenderTargetAndViewPort>(factory, category: renderingCategory)
                .AddInput(nameof(WithRenderTargetAndViewPort.RenderTarget), x => x.RenderTarget, (x, v) => x.RenderTarget = v)
                .AddInput(nameof(WithRenderTargetAndViewPort.DepthBuffer), x => x.DepthBuffer, (x, v) => x.DepthBuffer = v)
                ;

            yield return NewInputRenderBaseNode<RenderContextModifierRenderer>(factory, category: renderingCategory)
                .AddInput(nameof(RenderContextModifierRenderer.Modifier), x => x.Modifier, (x, v) => x.Modifier = v)
                ;

            yield return factory.NewNode<ParentTransformationModifier>(category: renderingCategory, copyOnWrite: false)
               .AddInput(nameof(ParentTransformationModifier.Transformation), x => x.Transformation, (x, v) => x.Transformation = v)
               .AddInput(nameof(ParentTransformationModifier.ExistingTransformUsage), x => x.ExistingTransformUsage, (x, v) => x.ExistingTransformUsage = v)
               ;

            yield return NewInputRenderBaseNode<WithinCommonSpace>(factory, category: renderingAdvancedCategory)
                .AddInput(nameof(WithinCommonSpace.CommonScreenSpace), x => x.CommonScreenSpace, (x, v) => x.CommonScreenSpace = v, CommonSpace.DIPTopLeft)
                ;

            yield return NewInputRenderBaseNode<WithinPhysicalScreenSpace>(factory, category: renderingAdvancedCategory)
                .AddInput(nameof(WithinPhysicalScreenSpace.Units), x => x.Units, (x, v) => x.Units = v, ScreenSpaceUnits.DIP)
                .AddInput(nameof(WithinPhysicalScreenSpace.Anchor), x => x.Anchor, (x, v) => x.Anchor = v, Lib.Mathematics.RectangleAnchor.Center)
                .AddInput(nameof(WithinPhysicalScreenSpace.Offset), x => x.Offset, (x, v) => x.Offset = v)
                .AddInput(nameof(WithinPhysicalScreenSpace.Scale), x => x.Scale, (x, v) => x.Scale = v, 1f)
                .AddInput(nameof(WithinPhysicalScreenSpace.IgnoreExistingView), x => x.IgnoreExistingView, (x, v) => x.IgnoreExistingView = v, true)
                .AddInput(nameof(WithinPhysicalScreenSpace.IgnoreExistingProjection), x => x.IgnoreExistingProjection, (x, v) => x.IgnoreExistingProjection = v, true)
                ;


            yield return NewInputRenderBaseNode<WithinVirtualScreenSpace>(factory, category: renderingAdvancedCategory)
                .AddInput(nameof(WithinVirtualScreenSpace.Bounds), x => x.Bounds, (x, v) => x.Bounds = v, new RectangleF(-0.5f, -0.5f, 1, 1))
                .AddInput(nameof(WithinVirtualScreenSpace.AspectRatioCorrectionMode), x => x.AspectRatioCorrectionMode, (x, v) => x.AspectRatioCorrectionMode = v, SizeMode.FitOut)
                .AddInput(nameof(WithinVirtualScreenSpace.Anchor), x => x.Anchor, (x, v) => x.Anchor = v, Lib.Mathematics.RectangleAnchor.Center)
                .AddInput(nameof(WithinVirtualScreenSpace.IgnoreExistingView), x => x.IgnoreExistingView, (x, v) => x.IgnoreExistingView = v, true)
                .AddInput(nameof(WithinVirtualScreenSpace.IgnoreExistingProjection), x => x.IgnoreExistingProjection, (x, v) => x.IgnoreExistingProjection = v, true)
                ;

            yield return NewInputRenderBaseNode<WithRenderView>(factory, category: renderingAdvancedCategory)
                .AddInput(nameof(WithRenderView.RenderView), x => x.RenderView, (x, v) => x.RenderView = v)
                .AddInput(nameof(WithRenderView.AspectRatioCorrectionMode), x => x.AspectRatioCorrectionMode, (x, v) => x.AspectRatioCorrectionMode = v)
                ;

            yield return NewInputRenderBaseNode<WithWindowInputSource>(factory, category: renderingAdvancedCategory)
                .AddInput(nameof(WithWindowInputSource.InputSource), x => x.InputSource, (x, v) => x.InputSource = v)
                ;

            yield return factory.NewNode<GetWindowInputSource>(name: nameof(GetWindowInputSource), category: renderingAdvancedCategory, copyOnWrite: false)
                .AddInput(nameof(RendererBase.Input), x => x.Input, (x, v) => x.Input = v)
                .AddOutput(nameof(GetWindowInputSource.InputSource), x => x.InputSource)
            ;

            // Compute effect dispatchers
            var dispatchersCategory = $"{renderingAdvancedCategory}.ComputeEffect";
            yield return factory.NewNode<DirectComputeEffectDispatcher>(name: "DirectDispatcher", category: renderingAdvancedCategory, copyOnWrite: false, hasStateOutput: false)
                .AddCachedInput(nameof(DirectComputeEffectDispatcher.ThreadGroupCount), x => x.ThreadGroupCount, (x, v) => x.ThreadGroupCount = v, Int3.One)
                .AddOutput<IComputeEffectDispatcher>("Output", x => x);

            yield return factory.NewNode<IndirectComputeEffectDispatcher>(name: "IndirectDispatcher", category: renderingAdvancedCategory, copyOnWrite: false, hasStateOutput: false)
                .AddCachedInput(nameof(IndirectComputeEffectDispatcher.ArgumentBuffer), x => x.ArgumentBuffer, (x, v) => x.ArgumentBuffer = v)
                .AddCachedInput(nameof(IndirectComputeEffectDispatcher.OffsetInBytes), x => x.OffsetInBytes, (x, v) => x.OffsetInBytes = v)
                .AddOutput<IComputeEffectDispatcher>("Output", x => x);

            yield return factory.NewNode<CustomComputeEffectDispatcher>(name: "CustomDispatcher", category: renderingAdvancedCategory, copyOnWrite: false, hasStateOutput: false)
                .AddCachedInput(nameof(CustomComputeEffectDispatcher.ThreadGroupCountsSelector), x => x.ThreadGroupCountsSelector, (x, v) => x.ThreadGroupCountsSelector = v)
                .AddOutput<IComputeEffectDispatcher>("Output", x => x);

            //yield return factory.NewNode<RenderView>(name: "RenderView", category: renderingAdvancedCategory, copyOnWrite: false)
            //    .AddInput(nameof(RenderView.View), x => x.View, (x, v) => x.View = v)
            //    .AddInput(nameof(RenderView.Projection), x => x.Projection, (x, v) => x.Projection = v)
            //    .AddInput(nameof(RenderView.NearClipPlane), x => x.NearClipPlane, (x, v) => x.NearClipPlane = v)
            //    .AddInput(nameof(RenderView.FarClipPlane), x => x.FarClipPlane, (x, v) => x.FarClipPlane = v)
            //    // TODO: add more
            //    ;

            // Meshes
            yield return factory.NewMeshNode((CapsuleProceduralModel x) => (x.Length, x.Radius, x.Tessellation))
                .AddCachedInput(nameof(CapsuleProceduralModel.Length), x => x.Length, (x, v) => x.Length = v, 0.5f)
                .AddCachedInput(nameof(CapsuleProceduralModel.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddCachedInput(nameof(CapsuleProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((ConeProceduralModel x) => (x.Height, x.Radius, x.Tessellation))
                .AddCachedInput(nameof(ConeProceduralModel.Height), x => x.Height, (x, v) => x.Height = v, 1f)
                .AddCachedInput(nameof(ConeProceduralModel.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddCachedInput(nameof(ConeProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((CubeProceduralModel x) => x.Size, name: "BoxMesh")
                .AddCachedInput(nameof(CubeProceduralModel.Size), x => x.Size, (x, v) => x.Size = v, Vector3.One)
                .AddDefaultPins();

            yield return factory.NewMeshNode((CylinderProceduralModel x) => (x.Height, x.Radius, x.Tessellation))
                .AddCachedInput(nameof(CylinderProceduralModel.Height), x => x.Height, (x, v) => x.Height = v, 1f)
                .AddCachedInput(nameof(CylinderProceduralModel.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddCachedInput(nameof(CylinderProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((GeoSphereProceduralModel x) => (x.Radius, x.Tessellation))
                .AddCachedInput(nameof(GeoSphereProceduralModel.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddCachedInput(nameof(GeoSphereProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = VLMath.Clamp(v, 1, 5), 3)
                .AddDefaultPins();

            yield return factory.NewMeshNode((PlaneProceduralModel x) => (x.Size, x.Tessellation, x.Normal, x.GenerateBackFace))
                .AddCachedInput(nameof(PlaneProceduralModel.Size), x => x.Size, (x, v) => x.Size = v, Vector2.One)
                .AddCachedInput(nameof(PlaneProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v, Int2.One)
                .AddCachedInput(nameof(PlaneProceduralModel.Normal), x => x.Normal, (x, v) => x.Normal = v, NormalDirection.UpZ)
                .AddCachedInput(nameof(PlaneProceduralModel.GenerateBackFace), x => x.GenerateBackFace, (x, v) => x.GenerateBackFace = v, true)
                .AddDefaultPins();

            yield return factory.NewMeshNode((SphereProceduralModel x) => (x.Radius, x.Tessellation))
                .AddCachedInput(nameof(SphereProceduralModel.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddCachedInput(nameof(SphereProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((TeapotProceduralModel x) => (x.Size, x.Tessellation))
                .AddCachedInput(nameof(TeapotProceduralModel.Size), x => x.Size, (x, v) => x.Size = v, 1f)
                .AddCachedInput(nameof(TeapotProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((TorusProceduralModel x) => (x.Radius, x.Thickness, x.Tessellation))
                .AddCachedInput(nameof(TorusProceduralModel.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddCachedInput(nameof(TorusProceduralModel.Thickness), x => x.Thickness, (x, v) => x.Thickness = v, 0.25f)
                .AddCachedInput(nameof(TorusProceduralModel.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v, 16)
                .AddDefaultPins();

            //g3 primitives
            yield return factory.NewMeshNode((Models.ConeModel x) => (x.BaseRadius, x.Clockwise, x.FromAngle, x.ToAngle, x.Height, x.SharedVertices, x.Slices))
                .AddCachedInput(nameof(Models.ConeModel.BaseRadius), x => x.BaseRadius, (x, v) => x.BaseRadius = v, 0.5f)
                .AddCachedInput(nameof(Models.ConeModel.Clockwise), x => x.Clockwise, (x, v) => x.Clockwise = v, false)
                .AddCachedInput(nameof(Models.ConeModel.FromAngle), x => x.FromAngle, (x, v) => x.FromAngle = v, 0f)
                .AddCachedInput(nameof(Models.ConeModel.ToAngle), x => x.ToAngle, (x, v) => x.ToAngle = v, 1f)
                .AddCachedInput(nameof(Models.ConeModel.Height), x => x.Height, (x, v) => x.Height = v, 1f)
                .AddCachedInput(nameof(Models.ConeModel.SharedVertices), x => x.SharedVertices, (x, v) => x.SharedVertices = v, false)
                .AddCachedInput(nameof(Models.ConeModel.Slices), x => x.Slices, (x, v) => x.Slices = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((Models.CappedCylinderModel x) => (x.BaseRadius, x.TopRadius, x.Clockwise, x.FromAngle, x.ToAngle, x.Height, x.SharedVertices, x.Slices))
                .AddCachedInput(nameof(Models.CappedCylinderModel.BaseRadius), x => x.BaseRadius, (x, v) => x.BaseRadius = v, 0.5f)
                .AddCachedInput(nameof(Models.CappedCylinderModel.TopRadius), x => x.TopRadius, (x, v) => x.TopRadius = v, 0.75f)
                .AddCachedInput(nameof(Models.CappedCylinderModel.Clockwise), x => x.Clockwise, (x, v) => x.Clockwise = v, false)
                .AddCachedInput(nameof(Models.CappedCylinderModel.FromAngle), x => x.FromAngle, (x, v) => x.FromAngle = v, 0f)
                .AddCachedInput(nameof(Models.CappedCylinderModel.ToAngle), x => x.ToAngle, (x, v) => x.ToAngle = v, 1f)
                .AddCachedInput(nameof(Models.CappedCylinderModel.Height), x => x.Height, (x, v) => x.Height = v, 1f)
                .AddCachedInput(nameof(Models.CappedCylinderModel.SharedVertices), x => x.SharedVertices, (x, v) => x.SharedVertices = v, false)
                .AddCachedInput(nameof(Models.CappedCylinderModel.Slices), x => x.Slices, (x, v) => x.Slices = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((Models.OpenCylinderModel x) => (x.BaseRadius, x.TopRadius, x.Clockwise, x.FromAngle, x.ToAngle, x.Height, x.SharedVertices, x.Slices))
                .AddCachedInput(nameof(Models.OpenCylinderModel.BaseRadius), x => x.BaseRadius, (x, v) => x.BaseRadius = v, 0.5f)
                .AddCachedInput(nameof(Models.OpenCylinderModel.TopRadius), x => x.TopRadius, (x, v) => x.TopRadius = v, 0.75f)
                .AddCachedInput(nameof(Models.OpenCylinderModel.Clockwise), x => x.Clockwise, (x, v) => x.Clockwise = v, false)
                .AddCachedInput(nameof(Models.OpenCylinderModel.FromAngle), x => x.FromAngle, (x, v) => x.FromAngle = v, 0f)
                .AddCachedInput(nameof(Models.OpenCylinderModel.ToAngle), x => x.ToAngle, (x, v) => x.ToAngle = v, 1f)
                .AddCachedInput(nameof(Models.OpenCylinderModel.Height), x => x.Height, (x, v) => x.Height = v, 1f)
                .AddCachedInput(nameof(Models.OpenCylinderModel.SharedVertices), x => x.SharedVertices, (x, v) => x.SharedVertices = v, false)
                .AddCachedInput(nameof(Models.OpenCylinderModel.Slices), x => x.Slices, (x, v) => x.Slices = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((Models.VerticalGeneralizedCylinderModel x) => (x.Capped, x.Sections, x.Clockwise, x.SharedVertices, x.Slices))
                .AddCachedInput(nameof(Models.VerticalGeneralizedCylinderModel.Capped), x => x.Capped, (x, v) => x.Capped = v, true)
                .AddCachedInput(nameof(Models.VerticalGeneralizedCylinderModel.Sections), x => x.Sections, (x, v) => x.Sections = v)
                .AddCachedInput(nameof(Models.VerticalGeneralizedCylinderModel.Clockwise), x => x.Clockwise, (x, v) => x.Clockwise = v, false)
                .AddCachedInput(nameof(Models.VerticalGeneralizedCylinderModel.SharedVertices), x => x.SharedVertices, (x, v) => x.SharedVertices = v, false)
                .AddCachedInput(nameof(Models.VerticalGeneralizedCylinderModel.Slices), x => x.Slices, (x, v) => x.Slices = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((Models.TubeModel x) => (x.Path, x.Closed, x.Shape, x.Capped, x.SharedVertices))
                .AddCachedInput(nameof(Models.TubeModel.Path), x => x.Path, (x, v) => x.Path = v)
                .AddCachedInput(nameof(Models.TubeModel.Closed), x => x.Closed, (x, v) => x.Closed = v,false)
                .AddCachedInput(nameof(Models.TubeModel.Shape), x => x.Shape, (x, v) => x.Shape = v)
                .AddCachedInput(nameof(Models.TubeModel.Capped), x => x.Capped, (x, v) => x.Capped = v, true)
                .AddCachedInput(nameof(Models.TubeModel.Clockwise), x => x.Clockwise, (x, v) => x.Clockwise = v, false)
                .AddCachedInput(nameof(Models.TubeModel.SharedVertices), x => x.SharedVertices, (x, v) => x.SharedVertices = v, false)
                .AddDefaultPins();

            yield return factory.NewMeshNode((Models.DiscModel x) => (x.OuterRadius, x.InnerRadius, x.Clockwise, x.FromAngle, x.ToAngle, x.Slices))
                .AddCachedInput(nameof(Models.DiscModel.OuterRadius), x => x.OuterRadius, (x, v) => x.OuterRadius = v, 1f)
                .AddCachedInput(nameof(Models.DiscModel.InnerRadius), x => x.InnerRadius, (x, v) => x.InnerRadius = v, 0.5f)
                .AddCachedInput(nameof(Models.DiscModel.Clockwise), x => x.Clockwise, (x, v) => x.Clockwise = v, true)
                .AddCachedInput(nameof(Models.DiscModel.FromAngle), x => x.FromAngle, (x, v) => x.FromAngle = v, 0f)
                .AddCachedInput(nameof(Models.DiscModel.ToAngle), x => x.ToAngle, (x, v) => x.ToAngle = v, 1f)
                .AddCachedInput(nameof(Models.DiscModel.Slices), x => x.Slices, (x, v) => x.Slices = v, 16)
                .AddDefaultPins();

            yield return factory.NewMeshNode((Models.GriddedRectModel x) => (x.EdgeVertices, x.Width, x.Height, x.Clockwise))
                .AddCachedInput(nameof(Models.GriddedRectModel.EdgeVertices), x => x.EdgeVertices, (x, v) => x.EdgeVertices = v, 2)
                .AddCachedInput(nameof(Models.GriddedRectModel.Width), x => x.Width, (x, v) => x.Width = v, 1f)
                .AddCachedInput(nameof(Models.GriddedRectModel.Height), x => x.Height, (x, v) => x.Height = v, 0.5f)
                .AddCachedInput(nameof(Models.GriddedRectModel.Clockwise), x => x.Clockwise, (x, v) => x.Clockwise = v, true)
                .AddDefaultPins();

            yield return factory.NewMeshNode((Models.RoundRectModel x) => (x.CornerSteps, x.Width, x.Height, x.Radius, x.SharpCorners, x.Clockwise))
                .AddCachedInput(nameof(Models.RoundRectModel.CornerSteps), x => x.CornerSteps, (x, v) => x.CornerSteps = v, 4)
                .AddCachedInput(nameof(Models.RoundRectModel.Width), x => x.Width, (x, v) => x.Width = v, 2f)
                .AddCachedInput(nameof(Models.RoundRectModel.Height), x => x.Height, (x, v) => x.Height = v, 1f)
                .AddCachedInput(nameof(Models.RoundRectModel.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.25f)
                .AddCachedInput(nameof(Models.RoundRectModel.SharpCorners), x => x.SharpCorners, (x, v) => x.SharpCorners = v, Models.RoundRectModel.Corner.None)
                .AddCachedInput(nameof(Models.RoundRectModel.Clockwise), x => x.Clockwise, (x, v) => x.Clockwise = v, true)
                .AddDefaultPins();

            yield return factory.NewMeshNode((Models.GridBoxModel x) => (x.Center, x.EdgeVertices, x.Extent, x.Clockwise, x.SharedVertices))
                .AddCachedInput(nameof(Models.GridBoxModel.Center), x => x.Center, (x, v) => x.Center = v, Vector3.Zero)
                .AddCachedInput(nameof(Models.GridBoxModel.EdgeVertices), x => x.EdgeVertices, (x, v) => x.EdgeVertices = v, 2)
                .AddCachedInput(nameof(Models.GridBoxModel.Extent), x => x.Extent, (x, v) => x.Extent = v, Vector3.One)
                .AddCachedInput(nameof(Models.GridBoxModel.Clockwise), x => x.Clockwise, (x, v) => x.Clockwise = v, true)
                .AddCachedInput(nameof(Models.GridBoxModel.SharedVertices), x => x.SharedVertices, (x, v) => x.SharedVertices = v, false)
                .AddDefaultPins();

            yield return factory.NewMeshNode((Models.SphereModel x) => (x.EdgeVertices, x.Radius, x.SharedVertices))
                .AddCachedInput(nameof(Models.SphereModel.EdgeVertices), x => x.EdgeVertices, (x, v) => x.EdgeVertices = v, 8)
                .AddCachedInput(nameof(Models.SphereModel.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddCachedInput(nameof(Models.SphereModel.SharedVertices), x => x.SharedVertices, (x, v) => x.SharedVertices = v, false)
                .AddDefaultPins();

            yield return factory.NewMeshNode((Models.Radial3DArrowModel x) => (x.HeadBaseRadius, x.HeadLength, x.StickLength, x.StickRadius, x.TipRadius, x.Clockwise, x.SharedVertices, x.Slices))
                .AddCachedInput(nameof(Models.Radial3DArrowModel.HeadBaseRadius), x => x.HeadBaseRadius, (x, v) => x.HeadBaseRadius = v, 0.3333f)
                .AddCachedInput(nameof(Models.Radial3DArrowModel.HeadLength), x => x.HeadLength, (x, v) => x.HeadLength = v, 0.5f)
                .AddCachedInput(nameof(Models.Radial3DArrowModel.StickLength), x => x.StickLength, (x, v) => x.StickLength = v, 1f)
                .AddCachedInput(nameof(Models.Radial3DArrowModel.StickRadius), x => x.StickRadius, (x, v) => x.StickRadius = v, 0.125f)
                .AddCachedInput(nameof(Models.Radial3DArrowModel.TipRadius), x => x.TipRadius, (x, v) => x.TipRadius = v, 0f)
                .AddCachedInput(nameof(Models.Radial3DArrowModel.Clockwise), x => x.Clockwise, (x, v) => x.Clockwise = v, false)
                .AddCachedInput(nameof(Models.Radial3DArrowModel.SharedVertices), x => x.SharedVertices, (x, v) => x.SharedVertices = v, false)
                .AddCachedInput(nameof(Models.Radial3DArrowModel.Slices), x => x.Slices, (x, v) => x.Slices = v, 16)
                .AddDefaultPins();

            // TextureFX
            yield return factory.NewNode(c => new MipMapGenerator(c), name: "MipMap", category: "Stride.Textures.Experimental.Utils", copyOnWrite: false, hasStateOutput: false)
                .AddInput("Input", x => x.InputTexture, (x, v) => x.InputTexture = v)
                .AddInput(nameof(MipMapGenerator.MaxMipMapCount), x => x.MaxMipMapCount, (x, v) => x.MaxMipMapCount = v)
                .AddOutput("Output", x => { x.ScheduleForRendering(); return x.OutputTexture; });
        }

        static CustomNodeDesc<TInputRenderBase> NewInputRenderBaseNode<TInputRenderBase>(IVLNodeDescriptionFactory factory, string category, string name = null)
            where TInputRenderBase : RendererBase, new()
        {
            return factory.NewNode<TInputRenderBase>(name: name, category: category, copyOnWrite: false)
                .AddInput(nameof(RendererBase.Input), x => x.Input, (x, v) => x.Input = v);
        }

        static CustomNodeDesc<TProceduralModel> NewMeshNode<TProceduralModel, TKey>(this IVLNodeDescriptionFactory factory, Func<TProceduralModel, TKey> getKey, string name = null)
           where TProceduralModel : PrimitiveProceduralModelBase, new()
        {
            return new CustomNodeDesc<TProceduralModel>(factory,
                name: name ?? typeof(TProceduralModel).Name.Replace("ProceduralModel", "Mesh"),
                category: "Stride.Models.Meshes",
                copyOnWrite: false,
                hasStateOutput: false,
                ctor: nodeContext =>
                {
                    var generator = new TProceduralModel();
                    return (generator, default);
                })
                .AddCachedOutput<Mesh>("Output", lifetime =>
                {
                    var disposable = new SerialDisposable();
                    lifetime.Add(disposable);

                    var gameProvider = ServiceRegistry.Current.GetGameProvider();
                    return generator =>
                    {
                        var key = (gameProvider, typeof(TProceduralModel), generator.Scale, generator.UvScale, generator.LocalOffset, generator.NumberOfTextureCoordinates, getKey(generator));
                        var provider = ResourceProvider.NewPooledSystemWide(key, _ =>
                        {
                            return gameProvider.Bind(
                                game =>
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
                });
        }

        static CustomNodeDesc<TProceduralModel> AddDefaultPins<TProceduralModel>(this CustomNodeDesc<TProceduralModel> node)
            where TProceduralModel : PrimitiveProceduralModelBase, new()
        {
            return node
                .AddCachedInput(nameof(PrimitiveProceduralModelBase.Scale), x => x.Scale, (x, v) => x.Scale = v, Vector3.One)
                .AddCachedInput(nameof(PrimitiveProceduralModelBase.UvScale), x => x.UvScale, (x, v) => x.UvScale = v, Vector2.One)
                .AddCachedInput(nameof(PrimitiveProceduralModelBase.LocalOffset), x => x.LocalOffset, (x, v) => x.LocalOffset = v, Vector3.Zero)
                .AddCachedInput(nameof(PrimitiveProceduralModelBase.NumberOfTextureCoordinates), x => x.NumberOfTextureCoordinates, (x, v) => x.NumberOfTextureCoordinates = v, 1);
        }
    }
}
