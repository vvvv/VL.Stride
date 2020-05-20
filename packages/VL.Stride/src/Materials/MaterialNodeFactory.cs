using Stride.Core;
using Stride.Engine;
using Stride.Particles.Rendering;
using Stride.Rendering;
using Stride.Rendering.Background;
using Stride.Rendering.Compositing;
using Stride.Rendering.Images;
using Stride.Rendering.Lights;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Rendering.Shadows;
using Stride.Rendering.Sprites;
using Stride.Rendering.SubsurfaceScattering;
using Stride.Rendering.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using VL.Core;
using VL.Lib.Collections;
using VL.Stride.Rendering;

[assembly: NodeFactory(typeof(VL.Stride.Materials.MaterialNodeFactory))]

namespace VL.Stride.Materials
{
    public class MaterialNodeFactory : IVLNodeDescriptionFactory
    {
        public MaterialNodeFactory()
        {
            NodeDescriptions = new ReadOnlyObservableCollection<IVLNodeDescription>(new ObservableCollection<IVLNodeDescription>(GetNodeDescriptions()));
        }

        public ReadOnlyObservableCollection<IVLNodeDescription> NodeDescriptions { get; }

        IEnumerable<IVLNodeDescription> GetNodeDescriptions()
        {
            string renderingCategory = "Stride.Rendering";
            string materialCategory = $"{renderingCategory}.Materials";
            string geometryCategory = $"{materialCategory}.Geometry";
            string shadingCategory = $"{materialCategory}.Shading";
            string miscCategory = $"{materialCategory}.Misc";
            string transparencyCategory = $"{miscCategory}.Transparency";
            string layersCategory = $"{materialCategory}.Layers";
            string diffuseModelCategory = $"{shadingCategory}.DiffuseModel";
            string specularModelCategory = $"{shadingCategory}.SpecularModel";

            // Geometry
            yield return new MaterialNodeDescription<MaterialTessellationFlatFeature>(this, "FlatTesselation", geometryCategory);
            yield return new MaterialNodeDescription<MaterialTessellationPNFeature>(this, "PointNormalTessellation", geometryCategory);
            yield return new MaterialNodeDescription<MaterialDisplacementMapFeature>(this, "DisplacementMap", geometryCategory);
            yield return new MaterialNodeDescription<MaterialNormalMapFeature>(this, "NormalMap", geometryCategory);
            yield return new MaterialNodeDescription<MaterialGlossinessMapFeature>(this, "GlossMap", geometryCategory);

            // Shading
            yield return new MaterialNodeDescription<MaterialDiffuseMapFeature>(this, "DiffuseMap", shadingCategory);

            yield return new MaterialNodeDescription<MaterialDiffuseCelShadingModelFeature>(this, "CelShading", diffuseModelCategory);
            yield return new MaterialNodeDescription<MaterialDiffuseHairModelFeature>(this, "Hair", diffuseModelCategory);
            yield return new MaterialNodeDescription<MaterialDiffuseLambertModelFeature>(this, "Lambert", diffuseModelCategory);

            yield return new MaterialNodeDescription<MaterialMetalnessMapFeature>(this, "MetalnessMap", shadingCategory);
            yield return new MaterialNodeDescription<MaterialSpecularMapFeature>(this, "SpecularMap", shadingCategory);

            yield return new MaterialNodeDescription<MaterialSpecularCelShadingModelFeatureUsingEnums>(this, "CelShading", specularModelCategory);
            yield return new MaterialNodeDescription<MaterialCelShadingLightDefault>(this, "DefaultLightFunction", $"{specularModelCategory}.CelShading");
            yield return new MaterialNodeDescription<MaterialCelShadingLightRamp>(this, "RampLightFunction", $"{specularModelCategory}.CelShading");

            yield return new MaterialNodeDescription<MaterialSpecularHairModelFeature>(this, "Hair", specularModelCategory);
            yield return new MaterialNodeDescription<MaterialSpecularMicrofacetModelFeatureUsingEnums>(this, "Microfacet", specularModelCategory);
            yield return new MaterialNodeDescription<MaterialSpecularThinGlassModelFeatureUsingEnums>(this, "Glass", specularModelCategory);

            yield return new MaterialNodeDescription<MaterialEmissiveMapFeature>(this, "EmissiveMap", shadingCategory);
            yield return new MaterialNodeDescription<MaterialSubsurfaceScatteringFeature>(this, "SubsurfaceScattering", shadingCategory);

            // Misc
            yield return new MaterialNodeDescription<MaterialOcclusionMapFeature>(this, "OcclusionMap", miscCategory);
            yield return new MaterialNodeDescription<MaterialTransparencyAdditiveFeature>(this, "Additive", transparencyCategory);
            yield return new MaterialNodeDescription<MaterialTransparencyBlendFeature>(this, "Blend", transparencyCategory);
            yield return new MaterialNodeDescription<MaterialTransparencyCutoffFeature>(this, "Cutoff", transparencyCategory);
            yield return new MaterialNodeDescription<MaterialClearCoatFeature>(this, "ClearCoat", miscCategory);

            // Layers
            yield return new MaterialNodeDescription<MaterialBlendLayer>(this, "MaterialLayer", layersCategory);
            yield return new MaterialNodeDescription<MaterialOverrides>(this, "LayerOverrides", layersCategory);

            // Top level
            // TODO: The Overrides property is a getter only - we need to expose the inner properties
            yield return new MaterialNodeDescription<MaterialAttributes>(this, "MaterialAttributes", materialCategory);
            yield return new MaterialNodeDescription<MaterialDescriptor>(this, "MaterialDescriptor", materialCategory);

            // Not so sure about these - they work for now
            yield return new MaterialNodeDescription<ComputeColor>(this, "ComputeColor", materialCategory);
            yield return new MaterialNodeDescription<ComputeFloat>(this, "ComputeFloat", materialCategory);
            yield return new MaterialNodeDescription<ComputeTextureColor>(this, "ComputeTextureColor", materialCategory);
            yield return new MaterialNodeDescription<ComputeTextureScalar>(this, "ComputeTextureScalar", materialCategory);


            string compositionCategory = $"{renderingCategory}.Composition";
            yield return new CustomNodeDesc<GraphicsCompositor>(this, category: compositionCategory)
                .AddInput(nameof(GraphicsCompositor.Game), x => x.Game, (x, v) => x.Game = v)
                .AddListInput(nameof(GraphicsCompositor.RenderStages), x => x.RenderStages)
                .AddListInput(nameof(GraphicsCompositor.RenderFeatures), x => x.RenderFeatures);

            yield return new CustomNodeDesc<RenderStage>(this, category: compositionCategory)
                .AddInput(nameof(RenderStage.Name), x => x.Name, (x, v) => x.Name = v, defaultValue: "RenderStage")
                .AddInput(nameof(RenderStage.EffectSlotName), x => x.EffectSlotName, (x, v) => x.EffectSlotName = v, defaultValue: "Main")
                .AddInput(nameof(RenderStage.SortMode), x => x.SortMode.ToPredefinedSortMode(), (x, v) => x.SortMode = v.ToSortMode());

            // Render stage selectors
            string renderStageSelectorCategory = $"{compositionCategory}.RenderStageSelector";
            yield return new CustomNodeDesc<MeshTransparentRenderStageSelector>(this, category: renderStageSelectorCategory)
                .AddInput(nameof(MeshTransparentRenderStageSelector.EffectName), x => x.EffectName, (x, v) => x.EffectName = v, defaultValue: "StrideForwardShadingEffect")
                .AddInput(nameof(MeshTransparentRenderStageSelector.RenderGroup), x => x.RenderGroup, (x, v) => x.RenderGroup = v, RenderGroupMask.All)
                .AddInput(nameof(MeshTransparentRenderStageSelector.OpaqueRenderStage), x => x.OpaqueRenderStage, (x, v) => x.OpaqueRenderStage = v)
                .AddInput(nameof(MeshTransparentRenderStageSelector.TransparentRenderStage), x => x.TransparentRenderStage, (x, v) => x.TransparentRenderStage = v);

            yield return new CustomNodeDesc<ShadowMapRenderStageSelector>(this, category: renderStageSelectorCategory)
                .AddInput(nameof(ShadowMapRenderStageSelector.EffectName), x => x.EffectName, (x, v) => x.EffectName = v, defaultValue: "StrideForwardShadingEffect.ShadowMapCaster")
                .AddInput(nameof(ShadowMapRenderStageSelector.RenderGroup), x => x.RenderGroup, (x, v) => x.RenderGroup = v, RenderGroupMask.All)
                .AddInput(nameof(ShadowMapRenderStageSelector.ShadowMapRenderStage), x => x.ShadowMapRenderStage, (x, v) => x.ShadowMapRenderStage = v);

            yield return new CustomNodeDesc<SimpleGroupToRenderStageSelector>(this, category: renderStageSelectorCategory)
                .AddInput(nameof(SimpleGroupToRenderStageSelector.EffectName), x => x.EffectName, (x, v) => x.EffectName = v, defaultValue: "Test")
                .AddInput(nameof(SimpleGroupToRenderStageSelector.RenderGroup), x => x.RenderGroup, (x, v) => x.RenderGroup = v, RenderGroupMask.All)
                .AddInput(nameof(SimpleGroupToRenderStageSelector.RenderStage), x => x.RenderStage, (x, v) => x.RenderStage = v);

            yield return new CustomNodeDesc<ParticleEmitterTransparentRenderStageSelector>(this, category: renderStageSelectorCategory)
                .AddInput(nameof(ParticleEmitterTransparentRenderStageSelector.EffectName), x => x.EffectName, (x, v) => x.EffectName = v, defaultValue: "Particles")
                .AddInput(nameof(ParticleEmitterTransparentRenderStageSelector.RenderGroup), x => x.RenderGroup, (x, v) => x.RenderGroup = v, RenderGroupMask.All)
                .AddInput(nameof(ParticleEmitterTransparentRenderStageSelector.OpaqueRenderStage), x => x.OpaqueRenderStage, (x, v) => x.OpaqueRenderStage = v)
                .AddInput(nameof(ParticleEmitterTransparentRenderStageSelector.TransparentRenderStage), x => x.TransparentRenderStage, (x, v) => x.TransparentRenderStage = v);

            yield return new CustomNodeDesc<SpriteTransparentRenderStageSelector>(this, category: renderStageSelectorCategory)
                .AddInput(nameof(SpriteTransparentRenderStageSelector.EffectName), x => x.EffectName, (x, v) => x.EffectName = v, defaultValue: "Test")
                .AddInput(nameof(SpriteTransparentRenderStageSelector.RenderGroup), x => x.RenderGroup, (x, v) => x.RenderGroup = v, RenderGroupMask.All)
                .AddInput(nameof(SpriteTransparentRenderStageSelector.OpaqueRenderStage), x => x.OpaqueRenderStage, (x, v) => x.OpaqueRenderStage = v)
                .AddInput(nameof(SpriteTransparentRenderStageSelector.TransparentRenderStage), x => x.TransparentRenderStage, (x, v) => x.TransparentRenderStage = v);

            yield return new CustomNodeDesc<SceneExternalCameraRenderer>(this, name: "CameraRenderer", category: compositionCategory)
                .AddInput(nameof(SceneExternalCameraRenderer.Child), x => x.Child, (x, v) => x.Child = v)
                .AddInput("Camera", x => x.ExternalCamera, (x, v) => x.ExternalCamera = v);

            yield return new CustomNodeDesc<ForwardRenderer>(this, category: compositionCategory, copyOnWrite: false)
                .AddInput(nameof(ForwardRenderer.Clear), x => x.Clear, (x, v) => x.Clear = v)
                .AddInput(nameof(ForwardRenderer.OpaqueRenderStage), x => x.OpaqueRenderStage, (x, v) => x.OpaqueRenderStage = v)
                .AddInput(nameof(ForwardRenderer.TransparentRenderStage), x => x.TransparentRenderStage, (x, v) => x.TransparentRenderStage = v)
                .AddListInput(nameof(ForwardRenderer.ShadowMapRenderStages), x => x.ShadowMapRenderStages)
                .AddInput(nameof(ForwardRenderer.PostEffects), x => x.PostEffects, (x, v) => x.PostEffects = v);

            yield return new MaterialNodeDescription<ClearRenderer>(this, category: compositionCategory) { CopyOnWrite = false };
            yield return new MaterialNodeDescription<SubsurfaceScatteringBlur>(this, category: compositionCategory) { CopyOnWrite = false };
            yield return new MaterialNodeDescription<VRRendererSettings>(this, category: compositionCategory) { CopyOnWrite = false };
            yield return new MaterialNodeDescription<LightShafts>(this, category: compositionCategory) { CopyOnWrite = false };

            // Post processing
            var postProcessingCategory = $"{renderingCategory}.PostProcessing";
            yield return new CustomNodeDesc<PostProcessingEffects>(this, category: postProcessingCategory, copyOnWrite: false)
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
                })
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
                });
            yield return new MaterialNodeDescription<LocalReflections>(this, category: postProcessingCategory);
            yield return new MaterialNodeDescription<DepthOfField>(this, category: postProcessingCategory);
            yield return new MaterialNodeDescription<LuminanceEffect>(this, category: postProcessingCategory);
            yield return new MaterialNodeDescription<BrightFilter>(this, category: postProcessingCategory);
            yield return new MaterialNodeDescription<Bloom>(this, category: postProcessingCategory);
            yield return new MaterialNodeDescription<LightStreak>(this, category: postProcessingCategory);
            yield return new MaterialNodeDescription<LensFlare>(this, category: postProcessingCategory);
            // AA
            yield return new MaterialNodeDescription<FXAAEffect>(this, category: postProcessingCategory);
            yield return new MaterialNodeDescription<TemporalAntiAliasEffect>(this, category: postProcessingCategory);

            // Root render features
            yield return new CustomNodeDesc<MeshRenderFeature>(this, category: renderingCategory)
                .AddListInput(nameof(MeshRenderFeature.RenderFeatures), x => x.RenderFeatures)
                .AddListInput(nameof(MeshRenderFeature.RenderStageSelectors), x => x.RenderStageSelectors)
                .AddListInput(nameof(MeshRenderFeature.PipelineProcessors), x => x.PipelineProcessors);

            yield return new CustomNodeDesc<BackgroundRenderFeature>(this, category: renderingCategory)
                .AddListInput(nameof(BackgroundRenderFeature.RenderStageSelectors), x => x.RenderStageSelectors);

            yield return new CustomNodeDesc<SpriteRenderFeature>(this, category: renderingCategory)
                .AddListInput(nameof(SpriteRenderFeature.RenderStageSelectors), x => x.RenderStageSelectors);

            // Sub render features for mesh render feature
            var renderFeaturesCategory = $"{renderingCategory}.RenderFeatures";
            yield return new MaterialNodeDescription<TransformRenderFeature>(this, category: renderFeaturesCategory);
            yield return new MaterialNodeDescription<SkinningRenderFeature>(this, category: renderFeaturesCategory);
            yield return new MaterialNodeDescription<MaterialRenderFeature>(this, category: renderFeaturesCategory);
            yield return new MaterialNodeDescription<ShadowCasterRenderFeature>(this, category: renderFeaturesCategory);

            yield return new CustomNodeDesc<ForwardLightingRenderFeature>(this, category: renderFeaturesCategory)
                .AddListInput(nameof(ForwardLightingRenderFeature.LightRenderers), x => x.LightRenderers)
                .AddInput(nameof(ForwardLightingRenderFeature.ShadowMapRenderer), x => x.ShadowMapRenderer, (x, v) => x.ShadowMapRenderer = v);

            var pipelineProcessorsCategory = $"{renderingCategory}.PipelineProcessors";
            yield return new CustomNodeDesc<MeshPipelineProcessor>(this, category: pipelineProcessorsCategory)
                .AddInput(nameof(MeshPipelineProcessor.TransparentRenderStage), x => x.TransparentRenderStage, (x, v) => x.TransparentRenderStage = v);

            yield return new CustomNodeDesc<ShadowMeshPipelineProcessor>(this, category: pipelineProcessorsCategory)
                .AddInput(nameof(ShadowMeshPipelineProcessor.ShadowMapRenderStage), x => x.ShadowMapRenderStage, (x, v) => x.ShadowMapRenderStage = v)
                .AddInput(nameof(ShadowMeshPipelineProcessor.DepthClipping), x => x.DepthClipping, (x, v) => x.DepthClipping = v);

            yield return new CustomNodeDesc<WireframePipelineProcessor>(this, category: pipelineProcessorsCategory)
                .AddInput(nameof(WireframePipelineProcessor.RenderStage), x => x.RenderStage, (x, v) => x.RenderStage = v);

            // Light renderers - make enum
            var lightsCategory = $"{renderingCategory}.Lights";
            yield return new MaterialNodeDescription<LightAmbientRenderer>(this, category: lightsCategory);
            yield return new MaterialNodeDescription<LightSkyboxRenderer>(this, category: lightsCategory);
            yield return new MaterialNodeDescription<LightDirectionalGroupRenderer>(this, category: lightsCategory);
            yield return new MaterialNodeDescription<LightPointGroupRenderer>(this, category: lightsCategory);
            yield return new MaterialNodeDescription<LightSpotGroupRenderer>(this, category: lightsCategory);
            yield return new MaterialNodeDescription<LightClusteredPointSpotGroupRenderer>(this, category: lightsCategory);

            // Shadow map renderers
            var shadowsCategory = $"{renderingCategory}.Shadows";
            yield return new CustomNodeDesc<ShadowMapRenderer>(this, category: shadowsCategory)
                .AddListInput(nameof(ShadowMapRenderer.Renderers), x => x.Renderers);

            yield return new MaterialNodeDescription<LightDirectionalShadowMapRenderer>(this, category: shadowsCategory);
            yield return new MaterialNodeDescription<LightSpotShadowMapRenderer>(this, category: shadowsCategory);
            yield return new MaterialNodeDescription<LightPointShadowMapRendererParaboloid>(this, category: shadowsCategory);
            yield return new MaterialNodeDescription<LightPointShadowMapRendererCubeMap>(this, category: shadowsCategory);
        }
    }
}
