using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Particles.Rendering;
using Stride.Rendering;
using Stride.Rendering.Background;
using Stride.Rendering.Compositing;
using Stride.Rendering.Images;
using Stride.Rendering.Images.Dither;
using Stride.Rendering.LightProbes;
using Stride.Rendering.Lights;
using Stride.Rendering.Materials;
using Stride.Rendering.Shadows;
using Stride.Rendering.Sprites;
using Stride.Rendering.SubsurfaceScattering;
using System;
using System.Collections.Generic;
using System.Reflection;
using VL.Core;

namespace VL.Stride.Rendering.Compositing
{
    static class CompositingNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(StrideNodeFactory nodeFactory)
        {
            var renderingCategory = "Stride.Rendering";
            var renderingCategoryAdvanced = $"{renderingCategory}.Advanced";

            var compositionCategory = $"{renderingCategoryAdvanced}.Compositing";
            yield return nodeFactory.NewGraphicsRendererNode<GraphicsCompositor>(category: compositionCategory)
                .AddInput(nameof(GraphicsCompositor.Game), x => x.Game, (x, v) => x.Game = v)
                .AddInput(nameof(GraphicsCompositor.SingleView), x => x.SingleView, (x, v) => x.SingleView = v)
                .AddListInput(nameof(GraphicsCompositor.RenderStages), x => x.RenderStages)
                .AddListInput(nameof(GraphicsCompositor.RenderFeatures), x => x.RenderFeatures)
                .AddEnabledPin();

            yield return nodeFactory.NewNode<RenderStage>(category: compositionCategory, fragmented: true)
                .AddInput(nameof(RenderStage.Name), x => x.Name, (x, v) => x.Name = v, defaultValue: "RenderStage")
                .AddInput(nameof(RenderStage.EffectSlotName), x => x.EffectSlotName, (x, v) => x.EffectSlotName = v, defaultValue: "Main")
                .AddInput(nameof(RenderStage.SortMode), x => x.SortMode.ToPredefinedSortMode(), (x, v) => x.SortMode = v.ToSortMode())
                .AddInput(nameof(RenderStage.Filter), x => x.Filter, (x, v) => x.Filter = v);

            yield return nodeFactory.NewNode<CustomRenderStageFilter>(category: compositionCategory, fragmented: true)
                .AddInput(nameof(CustomRenderStageFilter.IsVisibleFunc), x => x.IsVisibleFunc, (x, v) => x.IsVisibleFunc = v);

            // Render stage selectors
            string renderStageSelectorCategory = $"{compositionCategory}.RenderStageSelector";
            yield return nodeFactory.NewNode<MeshTransparentRenderStageSelector>(category: renderStageSelectorCategory, fragmented: true)
                .AddInput(nameof(MeshTransparentRenderStageSelector.EffectName), x => x.EffectName, (x, v) => x.EffectName = v, defaultValue: "StrideForwardShadingEffect")
                .AddInput(nameof(MeshTransparentRenderStageSelector.RenderGroup), x => x.RenderGroup, (x, v) => x.RenderGroup = v, RenderGroupMask.All)
                .AddInput(nameof(MeshTransparentRenderStageSelector.OpaqueRenderStage), x => x.OpaqueRenderStage, (x, v) => x.OpaqueRenderStage = v)
                .AddInput(nameof(MeshTransparentRenderStageSelector.TransparentRenderStage), x => x.TransparentRenderStage, (x, v) => x.TransparentRenderStage = v);

            yield return nodeFactory.NewNode<ShadowMapRenderStageSelector>(category: renderStageSelectorCategory, fragmented: true)
                .AddInput(nameof(ShadowMapRenderStageSelector.EffectName), x => x.EffectName, (x, v) => x.EffectName = v, defaultValue: "StrideForwardShadingEffect.ShadowMapCaster")
                .AddInput(nameof(ShadowMapRenderStageSelector.RenderGroup), x => x.RenderGroup, (x, v) => x.RenderGroup = v, RenderGroupMask.All)
                .AddInput(nameof(ShadowMapRenderStageSelector.ShadowMapRenderStage), x => x.ShadowMapRenderStage, (x, v) => x.ShadowMapRenderStage = v);

            yield return nodeFactory.NewNode<SimpleGroupToRenderStageSelector>(category: renderStageSelectorCategory, fragmented: true)
                .AddInput(nameof(SimpleGroupToRenderStageSelector.EffectName), x => x.EffectName, (x, v) => x.EffectName = v, defaultValue: "Test")
                .AddInput(nameof(SimpleGroupToRenderStageSelector.RenderGroup), x => x.RenderGroup, (x, v) => x.RenderGroup = v, RenderGroupMask.All)
                .AddInput(nameof(SimpleGroupToRenderStageSelector.RenderStage), x => x.RenderStage, (x, v) => x.RenderStage = v);

            yield return nodeFactory.NewNode<ParticleEmitterTransparentRenderStageSelector>(category: renderStageSelectorCategory, fragmented: true)
                .AddInput(nameof(ParticleEmitterTransparentRenderStageSelector.EffectName), x => x.EffectName, (x, v) => x.EffectName = v, defaultValue: "Particles")
                .AddInput(nameof(ParticleEmitterTransparentRenderStageSelector.RenderGroup), x => x.RenderGroup, (x, v) => x.RenderGroup = v, RenderGroupMask.All)
                .AddInput(nameof(ParticleEmitterTransparentRenderStageSelector.OpaqueRenderStage), x => x.OpaqueRenderStage, (x, v) => x.OpaqueRenderStage = v)
                .AddInput(nameof(ParticleEmitterTransparentRenderStageSelector.TransparentRenderStage), x => x.TransparentRenderStage, (x, v) => x.TransparentRenderStage = v);

            yield return nodeFactory.NewNode<SpriteTransparentRenderStageSelector>(category: renderStageSelectorCategory, fragmented: true)
                .AddInput(nameof(SpriteTransparentRenderStageSelector.EffectName), x => x.EffectName, (x, v) => x.EffectName = v, defaultValue: "Test")
                .AddInput(nameof(SpriteTransparentRenderStageSelector.RenderGroup), x => x.RenderGroup, (x, v) => x.RenderGroup = v, RenderGroupMask.All)
                .AddInput(nameof(SpriteTransparentRenderStageSelector.OpaqueRenderStage), x => x.OpaqueRenderStage, (x, v) => x.OpaqueRenderStage = v)
                .AddInput(nameof(SpriteTransparentRenderStageSelector.TransparentRenderStage), x => x.TransparentRenderStage, (x, v) => x.TransparentRenderStage = v);

            yield return nodeFactory.NewNode<EntityRendererStageSelector>(category: renderStageSelectorCategory, fragmented: true)
                .AddInput(nameof(EntityRendererStageSelector.RenderGroup), x => x.RenderGroup, (x, v) => x.RenderGroup = v, RenderGroupMask.All)
                .AddInput(nameof(EntityRendererStageSelector.BeforeScene), x => x.BeforeScene, (x, v) => x.BeforeScene = v)
                .AddInput(nameof(EntityRendererStageSelector.InSceneOpaque), x => x.InSceneOpaque, (x, v) => x.InSceneOpaque = v)
                .AddInput(nameof(EntityRendererStageSelector.InSceneTransparent), x => x.InSceneTransparent, (x, v) => x.InSceneTransparent = v)
                .AddInput(nameof(EntityRendererStageSelector.AfterScene), x => x.AfterScene, (x, v) => x.AfterScene = v);

            // Renderers
            yield return nodeFactory.NewGraphicsRendererNode<ClearRenderer>(category: compositionCategory)
                .AddInput(nameof(ClearRenderer.ClearFlags), x => x.ClearFlags, (x, v) => x.ClearFlags = v)
                .AddInput(nameof(ClearRenderer.Color), x => x.Color, (x, v) => x.Color = v, Color4.Black)
                .AddInput(nameof(ClearRenderer.Depth), x => x.Depth, (x, v) => x.Depth = v, 1f)
                .AddInput(nameof(ClearRenderer.Stencil), x => x.Stencil, (x, v) => x.Stencil = v)
                .AddEnabledPin();

            yield return nodeFactory.NewGraphicsRendererNode<DebugRenderer>(category: compositionCategory)
                .AddListInput(nameof(DebugRenderer.DebugRenderStages), x => x.DebugRenderStages)
                .AddEnabledPin();

            yield return nodeFactory.NewGraphicsRendererNode<SingleStageRenderer>(category: compositionCategory)
                .AddInput(nameof(SingleStageRenderer.RenderStage), x => x.RenderStage, (x, v) => x.RenderStage = v)
                .AddEnabledPin();

            yield return nodeFactory.NewGraphicsRendererNode<ForceAspectRatioSceneRenderer>(category: compositionCategory)
                .AddInput(nameof(ForceAspectRatioSceneRenderer.FixedAspectRatio), x => x.FixedAspectRatio, (x, v) => x.FixedAspectRatio = v)
                .AddInput(nameof(ForceAspectRatioSceneRenderer.ForceAspectRatio), x => x.ForceAspectRatio, (x, v) => x.ForceAspectRatio = v)
                .AddInput(nameof(ForceAspectRatioSceneRenderer.Child), x => x.Child, (x, v) => x.Child = v)
                .AddEnabledPin();

            var cameraComponent = new CameraComponent() { ViewMatrix = Matrix.Translation(0, 0, -2), UseCustomViewMatrix = true };
            yield return nodeFactory.NewGraphicsRendererNode<SceneExternalCameraRenderer>(name: "CameraRenderer", category: compositionCategory)
                .AddInput("Camera", x => x.ExternalCamera ?? cameraComponent, (x, v) => x.ExternalCamera = v)
                .AddInput(nameof(SceneExternalCameraRenderer.Child), x => x.Child, (x, v) => x.Child = v)
                .AddEnabledPin();

            yield return nodeFactory.NewGraphicsRendererNode<RenderTextureSceneRenderer>(category: compositionCategory)
                .AddInput(nameof(RenderTextureSceneRenderer.RenderTexture), x => x.RenderTexture, (x, v) => x.RenderTexture = v)
                .AddInput(nameof(RenderTextureSceneRenderer.Child), x => x.Child, (x, v) => x.Child = v)
                .AddEnabledPin();

            yield return nodeFactory.NewGraphicsRendererNode<SceneRendererCollection>(category: compositionCategory)
                .AddListInput(nameof(SceneRendererCollection.Children), x => x.Children)
                .AddEnabledPin();

            var defaultResolver = new MSAAResolver();
            yield return nodeFactory.NewGraphicsRendererNode<ForwardRenderer>(category: compositionCategory)
                .AddInput(nameof(ForwardRenderer.Clear), x => x.Clear, (x, v) => x.Clear = v, defaultValue: null /* We want null as default */)
                .AddInput(nameof(ForwardRenderer.OpaqueRenderStage), x => x.OpaqueRenderStage, (x, v) => x.OpaqueRenderStage = v)
                .AddInput(nameof(ForwardRenderer.TransparentRenderStage), x => x.TransparentRenderStage, (x, v) => x.TransparentRenderStage = v)
                .AddListInput(nameof(ForwardRenderer.ShadowMapRenderStages), x => x.ShadowMapRenderStages)
                .AddInput(nameof(ForwardRenderer.GBufferRenderStage), x => x.GBufferRenderStage, (x, v) => x.GBufferRenderStage = v)
                .AddInput(nameof(ForwardRenderer.PostEffects), x => x.PostEffects, (x, v) => x.PostEffects = v)
                .AddInput(nameof(ForwardRenderer.LightShafts), x => x.LightShafts, (x, v) => x.LightShafts = v)
                .AddInput(nameof(ForwardRenderer.VRSettings), x => x.VRSettings, (x, v) => x.VRSettings = v)
                .AddInput(nameof(ForwardRenderer.SubsurfaceScatteringBlurEffect), x => x.SubsurfaceScatteringBlurEffect, (x, v) => x.SubsurfaceScatteringBlurEffect = v)
                .AddInput(nameof(ForwardRenderer.MSAALevel), x => x.MSAALevel, (x, v) => x.MSAALevel = v)
                .AddInput(nameof(ForwardRenderer.MSAAResolver), x => x.MSAAResolver, (x, v) =>
                {
                    var s = x.MSAAResolver;
                    var y = v ?? defaultResolver;
                    s.FilterType = y.FilterType;
                    s.FilterRadius = y.FilterRadius;
                })
                .AddInput(nameof(ForwardRenderer.BindDepthAsResourceDuringTransparentRendering), x => x.BindDepthAsResourceDuringTransparentRendering, (x, v) => x.BindDepthAsResourceDuringTransparentRendering = v)
                .AddEnabledPin();


            yield return new StrideNodeDesc<SubsurfaceScatteringBlur>(nodeFactory, category: compositionCategory);
            yield return new StrideNodeDesc<VRRendererSettings>(nodeFactory, category: compositionCategory);
            yield return new StrideNodeDesc<LightShafts>(nodeFactory, category: compositionCategory);
            yield return new StrideNodeDesc<MSAAResolver>(nodeFactory, category: compositionCategory);

            // Post processing
            var postFxCategory = $"{renderingCategory}.PostFX";
            yield return CreatePostEffectsNode();

            yield return new StrideNodeDesc<AmbientOcclusion>(nodeFactory, category: postFxCategory);
            yield return new StrideNodeDesc<LocalReflections>(nodeFactory, category: postFxCategory);
            yield return new StrideNodeDesc<DepthOfField>(nodeFactory, category: postFxCategory);
            yield return new StrideNodeDesc<BrightFilter>(nodeFactory, category: postFxCategory);
            yield return new StrideNodeDesc<Bloom>(nodeFactory, category: postFxCategory);
            yield return new StrideNodeDesc<LightStreak>(nodeFactory, category: postFxCategory);
            yield return new StrideNodeDesc<LensFlare>(nodeFactory, category: postFxCategory);
            // AA
            yield return new StrideNodeDesc<FXAAEffect>(nodeFactory, category: postFxCategory);
            yield return new StrideNodeDesc<TemporalAntiAliasEffect>(nodeFactory, category: postFxCategory);
            // Color transforms
            var colorTransformsCategory = $"{postFxCategory}.ColorTransforms";
            yield return new StrideNodeDesc<LuminanceToChannelTransform>(nodeFactory, category: colorTransformsCategory) { CopyOnWrite = false };
            yield return new StrideNodeDesc<FilmGrain>(nodeFactory, category: colorTransformsCategory) { CopyOnWrite = false };
            yield return nodeFactory.NewNode<Vignetting>(category: colorTransformsCategory, copyOnWrite: false)
                .AddInput(nameof(Vignetting.Amount), x => x.Amount, (x, v) => x.Amount = v, 0.8f)
                .AddInput(nameof(Vignetting.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.7f)
                .AddInput(nameof(Vignetting.Color), x => x.Color.ToColor4(), (x, v) => x.Color = v.ToColor3(), Color4.Black)
                .AddInput(nameof(Vignetting.Enabled), x => x.Enabled, (x, v) => x.Enabled = v, true);
            yield return new StrideNodeDesc<Dither>(nodeFactory, category: colorTransformsCategory) { CopyOnWrite = false };

            yield return nodeFactory.NewNode<ToneMap>(category: colorTransformsCategory, copyOnWrite: false)
                .AddInput(nameof(ToneMap.Operator), x => x.Operator, (x, v) =>
                {
                    if (v != null && v != x.Operator && x.Group != null)
                    {
                        // For same operator types the shader permutation of the whole color transform group stays the same
                        // and therefor Stride will not update the parameter copiers of the transforms.
                        // We therefor invalidate the shader manually here.
                        var transformGroupEffectField = typeof(ColorTransformGroup).GetField("transformGroupEffect", BindingFlags.Instance | BindingFlags.NonPublic);
                        var transformGroupEffect = (ImageEffectShader)transformGroupEffectField.GetValue(x.Group);
                        var descriptorReflectionField = typeof(EffectInstance).GetField("descriptorReflection", BindingFlags.Instance | BindingFlags.NonPublic);
                        descriptorReflectionField.SetValue(transformGroupEffect.EffectInstance, null);
                    }
                    x.Operator = v;
                })
                .AddInput(nameof(ToneMap.AutoKeyValue), x => x.AutoKeyValue, (x, v) => x.AutoKeyValue = v, true)
                .AddInput(nameof(ToneMap.KeyValue), x => x.KeyValue, (x, v) => x.KeyValue = v, 0.18f)
                .AddInput(nameof(ToneMap.AutoExposure), x => x.AutoExposure, (x, v) => x.AutoExposure = v, true)
                .AddInput(nameof(ToneMap.Exposure), x => x.Exposure, (x, v) => x.Exposure = v, 0f)
                .AddInput(nameof(ToneMap.TemporalAdaptation), x => x.TemporalAdaptation, (x, v) => x.TemporalAdaptation = v, true)
                .AddInput(nameof(ToneMap.AdaptationRate), x => x.AdaptationRate, (x, v) => x.AdaptationRate = v, 1.0f)
                .AddInput(nameof(ToneMap.UseLocalLuminance), x => x.UseLocalLuminance, (x, v) => x.UseLocalLuminance = v, false)
                .AddInput(nameof(ToneMap.LuminanceLocalFactor), x => x.LuminanceLocalFactor, (x, v) => x.LuminanceLocalFactor = v, 0.0f)
                .AddInput(nameof(ToneMap.Contrast), x => x.Contrast, (x, v) => x.Contrast = v, 0f)
                .AddInput(nameof(ToneMap.Brightness), x => x.Brightness, (x, v) => x.Brightness = v, 0f)
                .AddInput(nameof(ToneMap.Enabled), x => x.Enabled, (x, v) => x.Enabled = v, true);

            //yield return new StrideNodeDesc<ToneMap>(nodeFactory, category: colorTransformsCategory) { CopyOnWrite = false };

            var operatorsCategory = $"{colorTransformsCategory}.ToneMapOperators";
            yield return new StrideNodeDesc<ToneMapHejl2Operator>(nodeFactory, "Hejl2", category: operatorsCategory) { CopyOnWrite = false };
            yield return new StrideNodeDesc<ToneMapHejlDawsonOperator>(nodeFactory, "HejlDawson", category: operatorsCategory) { CopyOnWrite = false };
            yield return new StrideNodeDesc<ToneMapMikeDayOperator>(nodeFactory, "MikeDay", category: operatorsCategory) { CopyOnWrite = false };
            yield return new StrideNodeDesc<ToneMapU2FilmicOperator>(nodeFactory, "U2Filmic", category: operatorsCategory) { CopyOnWrite = false };
            yield return new StrideNodeDesc<ToneMapDragoOperator>(nodeFactory, "Drago", category: operatorsCategory) { CopyOnWrite = false };
            yield return new StrideNodeDesc<ToneMapExponentialOperator>(nodeFactory, "Exponential", category: operatorsCategory) { CopyOnWrite = false };
            yield return new StrideNodeDesc<ToneMapLogarithmicOperator>(nodeFactory, "Logarithmic", category: operatorsCategory) { CopyOnWrite = false };
            yield return new StrideNodeDesc<ToneMapReinhardOperator>(nodeFactory, "Reinhard", category: operatorsCategory) { CopyOnWrite = false };

            // Root render features
            yield return nodeFactory.NewNode<MeshRenderFeature>(category: renderingCategoryAdvanced)
                .AddListInput(nameof(MeshRenderFeature.RenderFeatures), x => x.RenderFeatures)
                .AddListInput(nameof(MeshRenderFeature.RenderStageSelectors), x => x.RenderStageSelectors)
                .AddListInput(nameof(MeshRenderFeature.PipelineProcessors), x => x.PipelineProcessors);

            yield return nodeFactory.NewNode<BackgroundRenderFeature>(category: renderingCategoryAdvanced)
                .AddListInput(nameof(BackgroundRenderFeature.RenderStageSelectors), x => x.RenderStageSelectors);

            yield return nodeFactory.NewNode<SpriteRenderFeature>(category: renderingCategoryAdvanced)
                .AddListInput(nameof(SpriteRenderFeature.RenderStageSelectors), x => x.RenderStageSelectors);

            yield return nodeFactory.NewNode<EntityRendererRenderFeature>(category: renderingCategoryAdvanced)
                .AddListInput(nameof(EntityRendererRenderFeature.RenderStageSelectors), x => x.RenderStageSelectors)
                .AddInput(nameof(EntityRendererRenderFeature.HelpersRenderStage), x => x.HelpersRenderStage, (x, v) => x.HelpersRenderStage = v)
                .AddInput(nameof(EntityRendererRenderFeature.HelpersRenderer), x => x.HelpersRenderer, (x, v) => x.HelpersRenderer = v);

            // Sub render features for mesh render feature
            var renderFeaturesCategory = $"{renderingCategoryAdvanced}.RenderFeatures";
            yield return new StrideNodeDesc<TransformRenderFeature>(nodeFactory, category: renderFeaturesCategory);
            yield return new StrideNodeDesc<SkinningRenderFeature>(nodeFactory, category: renderFeaturesCategory);
            yield return new StrideNodeDesc<MaterialRenderFeature>(nodeFactory, category: renderFeaturesCategory);
            yield return new StrideNodeDesc<ShadowCasterRenderFeature>(nodeFactory, category: renderFeaturesCategory);
            yield return new StrideNodeDesc<InstancingRenderFeature>(nodeFactory, category: renderFeaturesCategory);

            yield return nodeFactory.NewNode<ForwardLightingRenderFeature>(category: renderFeaturesCategory)
                .AddListInput(nameof(ForwardLightingRenderFeature.LightRenderers), x => x.LightRenderers)
                .AddInput(nameof(ForwardLightingRenderFeature.ShadowMapRenderer), x => x.ShadowMapRenderer, (x, v) => x.ShadowMapRenderer = v);

            var pipelineProcessorsCategory = $"{renderingCategoryAdvanced}.PipelineProcessors";
            yield return nodeFactory.NewNode<MeshPipelineProcessor>(category: pipelineProcessorsCategory)
                .AddInput(nameof(MeshPipelineProcessor.TransparentRenderStage), x => x.TransparentRenderStage, (x, v) => x.TransparentRenderStage = v);

            yield return nodeFactory.NewNode<ShadowMeshPipelineProcessor>(category: pipelineProcessorsCategory)
                .AddInput(nameof(ShadowMeshPipelineProcessor.ShadowMapRenderStage), x => x.ShadowMapRenderStage, (x, v) => x.ShadowMapRenderStage = v)
                .AddInput(nameof(ShadowMeshPipelineProcessor.DepthClipping), x => x.DepthClipping, (x, v) => x.DepthClipping = v);

            yield return nodeFactory.NewNode<WireframePipelineProcessor>(category: pipelineProcessorsCategory)
                .AddInput(nameof(WireframePipelineProcessor.RenderStage), x => x.RenderStage, (x, v) => x.RenderStage = v);

            // Light renderers - make enum
            var lightsCategory = $"{renderingCategoryAdvanced}.Light";
            yield return new StrideNodeDesc<LightAmbientRenderer>(nodeFactory, category: lightsCategory);
            yield return new StrideNodeDesc<LightSkyboxRenderer>(nodeFactory, category: lightsCategory);
            yield return new StrideNodeDesc<LightDirectionalGroupRenderer>(nodeFactory, category: lightsCategory);
            yield return new StrideNodeDesc<LightPointGroupRenderer>(nodeFactory, category: lightsCategory);
            yield return new StrideNodeDesc<LightSpotGroupRenderer>(nodeFactory, category: lightsCategory);
            yield return new StrideNodeDesc<LightClusteredPointSpotGroupRenderer>(nodeFactory, category: lightsCategory);
            yield return new StrideNodeDesc<LightProbeRenderer>(nodeFactory, category: lightsCategory);

            // Shadow map renderers
            var shadowsCategory = $"{renderingCategoryAdvanced}.Shadow";
            yield return nodeFactory.NewNode<ShadowMapRenderer>(category: shadowsCategory)
                .AddListInput(nameof(ShadowMapRenderer.Renderers), x => x.Renderers);

            yield return new StrideNodeDesc<LightDirectionalShadowMapRenderer>(nodeFactory, category: shadowsCategory);
            yield return new StrideNodeDesc<LightSpotShadowMapRenderer>(nodeFactory, category: shadowsCategory);
            yield return new StrideNodeDesc<LightPointShadowMapRendererParaboloid>(nodeFactory, category: shadowsCategory);
            yield return new StrideNodeDesc<LightPointShadowMapRendererCubeMap>(nodeFactory, category: shadowsCategory);

            IVLNodeDescription CreatePostEffectsNode()
            {
                return nodeFactory.NewNode<PostProcessingEffects>(name: "PostFX", category: renderingCategory, copyOnWrite: false, 
                    init: effects =>
                    {
                        // Can't use effects.DisableAll() - disables private effects used by AA
                        effects.AmbientOcclusion.Enabled = false;
                        effects.LocalReflections.Enabled = false;
                        effects.DepthOfField.Enabled = false;
                        effects.BrightFilter.Enabled = false;
                        effects.Bloom.Enabled = false;
                        effects.LightStreak.Enabled = false;
                        effects.LensFlare.Enabled = false;
                        // ColorTransforms delegates to an empty list, keep it enabled
                        effects.ColorTransforms.Enabled = true;
                        effects.Antialiasing.Enabled = false;
                    })
                    .AddInput(nameof(PostProcessingEffects.AmbientOcclusion), x => x.AmbientOcclusion, (x, v) =>
                    {
                        var s = x.AmbientOcclusion;
                        if (v != null)
                        {
                            s.Enabled = v.Enabled;
                            s.NumberOfSamples = v.NumberOfSamples;
                            s.ParamProjScale = v.ParamProjScale;
                            s.ParamIntensity = v.ParamIntensity;
                            s.ParamBias = v.ParamBias;
                            s.ParamRadius = v.ParamRadius;
                            s.NumberOfBounces = v.NumberOfBounces;
                            s.BlurScale = v.BlurScale;
                            s.EdgeSharpness = v.EdgeSharpness;
                            s.TempSize = v.TempSize;
                        }
                        else
                        {
                            s.Enabled = false;
                        }
                    }, defaultValue: null /* null is used to disable */)
                    .AddInput(nameof(PostProcessingEffects.LocalReflections), x => x.LocalReflections, (x, v) =>
                    {
                        var s = x.LocalReflections;
                        if (v != null)
                        {
                            s.Enabled = v.Enabled;
                            s.DepthResolution = v.DepthResolution;
                            s.RayTracePassResolution = v.RayTracePassResolution;
                            s.MaxSteps = v.MaxSteps;
                            s.BRDFBias = v.BRDFBias;
                            s.GlossinessThreshold = v.GlossinessThreshold;
                            s.WorldAntiSelfOcclusionBias = v.WorldAntiSelfOcclusionBias;
                            s.ResolvePassResolution = v.ResolvePassResolution;
                            s.ResolveSamples = v.ResolveSamples;
                            s.ReduceHighlights = v.ReduceHighlights;
                            s.EdgeFadeFactor = v.EdgeFadeFactor;
                            s.UseColorBufferMips = v.UseColorBufferMips;
                            s.TemporalEffect = v.TemporalEffect;
                            s.TemporalScale = v.TemporalScale;
                            s.TemporalResponse = v.TemporalResponse;
                        }
                        else
                        {
                            s.Enabled = false;
                        }
                    }, defaultValue: null /* null is used to disable */)
                    .AddInput(nameof(PostProcessingEffects.DepthOfField), x => x.DepthOfField, (x, v) =>
                    {
                        var s = x.DepthOfField;
                        if (v != null)
                        {
                            s.Enabled = v.Enabled;
                            s.MaxBokehSize = v.MaxBokehSize;
                            s.DOFAreas = v.DOFAreas;
                            s.QualityPreset = v.QualityPreset;
                            s.Technique = v.Technique;
                            s.AutoFocus = v.AutoFocus;
                        }
                        else
                        {
                            s.Enabled = false;
                        }
                    }, defaultValue: null /* null is used to disable */)
                    .AddInput(nameof(PostProcessingEffects.BrightFilter), x => x.BrightFilter, (x, v) =>
                    {
                        var s = x.BrightFilter;
                        if (v != null)
                        {
                            s.Enabled = v.Enabled;
                            s.Threshold = v.Threshold;
                            s.Steepness = v.Steepness;
                            s.Color = v.Color;
                        }
                        else
                        {
                            // Keep the bright filter enabled. Needed by Bloom, LightStreak and LensFlare. 
                            // Stride will only use it if one of those is enabled.
                            s.Enabled = true;
                        }
                    }, defaultValue: null /* null is used to disable */)
                    .AddInput(nameof(PostProcessingEffects.Bloom), x => x.Bloom, (x, v) =>
                    {
                        var s = x.Bloom;
                        if (v != null)
                        {
                            s.Enabled = v.Enabled;
                            s.Radius = v.Radius;
                            s.Amount = v.Amount;
                            s.DownScale = v.DownScale;
                            s.SigmaRatio = v.SigmaRatio;
                            s.Distortion = v.Distortion;
                            s.Afterimage.Enabled = v.Afterimage.Enabled;
                            s.Afterimage.FadeOutSpeed = v.Afterimage.FadeOutSpeed;
                            s.Afterimage.Sensitivity = v.Afterimage.Sensitivity;
                        }
                        else
                        {
                            s.Enabled = false;
                        }
                    }, defaultValue: null /* null is used to disable */)
                    .AddInput(nameof(PostProcessingEffects.LightStreak), x => x.LightStreak, (x, v) =>
                    {
                        var s = x.LightStreak;
                        if (v != null)
                        {
                            s.Enabled = v.Enabled;
                            s.Amount = v.Amount;
                            s.StreakCount = v.StreakCount;
                            s.Attenuation = v.Attenuation;
                            s.Phase = v.Phase;
                            s.ColorAberrationStrength = v.ColorAberrationStrength;
                            s.IsAnamorphic = v.IsAnamorphic;
                        }
                        else
                        {
                            s.Enabled = false;
                        }
                    }, defaultValue: null /* null is used to disable */)
                    .AddInput(nameof(PostProcessingEffects.LensFlare), x => x.LensFlare, (x, v) =>
                    {
                        var s = x.LensFlare;
                        if (v != null)
                        {
                            s.Enabled = v.Enabled;
                            s.Amount = v.Amount;
                            s.ColorAberrationStrength = v.ColorAberrationStrength;
                            s.HaloFactor = v.HaloFactor;
                        }
                        else
                        {
                            s.Enabled = false;
                        }
                    }, defaultValue: null /* null is used to disable */)
                    .AddListInput(nameof(PostProcessingEffects.ColorTransforms), x => x.ColorTransforms.Transforms)
                    .AddInput(nameof(PostProcessingEffects.Antialiasing), x => x.Antialiasing, (x, v) => x.Antialiasing = v);
            }
        }

        static CustomNodeDesc<TRenderer> NewGraphicsRendererNode<TRenderer>(this IVLNodeDescriptionFactory factory, string category, string name = null)
            where TRenderer : IGraphicsRenderer, new()
        {
            return factory.NewNode<TRenderer>(name: name, category: category, copyOnWrite: false, fragmented: true);
        }

        static CustomNodeDesc<TRenderer> AddEnabledPin<TRenderer>(this CustomNodeDesc<TRenderer> node)
            where TRenderer : IGraphicsRenderer
        {
            return node.AddInput(nameof(IGraphicsRenderer.Enabled), x => x.Enabled, (x, v) => x.Enabled = v, defaultValue: true);
        }
    }
}
