using Stride.Core;
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
            yield return new MaterialNodeDescription<GraphicsCompositor>(this, category: compositionCategory)
            {
                nameof(GraphicsCompositor.RenderStages), nameof(GraphicsCompositor.RenderFeatures), nameof(GraphicsCompositor.Game)
            };
            yield return new MaterialNodeDescription<RenderStage>(this, category: compositionCategory) 
            { 
                nameof(RenderStage.Name), nameof(RenderStage.EffectSlotName), nameof(RenderStage.SortMode)
            };

            // Render stage selectors
            string renderStageSelectorCategory = $"{compositionCategory}.RenderStageSelector";
            //yield return new MaterialNodeDescription<MeshTransparentRenderStageSelector>(this, category: renderStageSelectorCategory);
            yield return new MaterialNodeDescription<ShadowMapRenderStageSelector>(this, category: renderStageSelectorCategory);
            yield return new MaterialNodeDescription<SimpleGroupToRenderStageSelector>(this, category: renderStageSelectorCategory);
            yield return new MaterialNodeDescription<ParticleEmitterTransparentRenderStageSelector>(this, category: renderStageSelectorCategory);
            yield return new MaterialNodeDescription<SpriteTransparentRenderStageSelector>(this, category: renderStageSelectorCategory);

            // TODO: make enum
            yield return new MaterialNodeDescription<SceneExternalCameraRenderer>(this, category: compositionCategory)
            {
                nameof(SceneExternalCameraRenderer.Child), nameof(SceneExternalCameraRenderer.ExternalCamera)
            };

            yield return new CustomNodeDesc<ForwardRenderer>(this, category: compositionCategory)
                .AddInput<ClearRenderer>(nameof(ForwardRenderer.Clear), (x, v) => x.Clear = v)
                .AddInput<RenderStage>(nameof(ForwardRenderer.OpaqueRenderStage), (x, v) => x.OpaqueRenderStage = v)
                .AddInput<RenderStage>(nameof(ForwardRenderer.TransparentRenderStage), (x, v) => x.TransparentRenderStage = v)
                .AddInput<IReadOnlyList<RenderStage>>(nameof(ForwardRenderer.ShadowMapRenderStages), (x, v) =>
                {
                    if (!x.ShadowMapRenderStages.SequenceEqual(v))
                    {
                        x.ShadowMapRenderStages.Clear();
                        x.ShadowMapRenderStages.AddRange(v);
                    }
                })
                .AddInput<IPostProcessingEffects>(nameof(ForwardRenderer.PostEffects), (x, v) => x.PostEffects = v)
                .AddOutput("Output", x => x);

            yield return new MaterialNodeDescription<ClearRenderer>(this, category: compositionCategory);
            yield return new MaterialNodeDescription<SubsurfaceScatteringBlur>(this, category: compositionCategory);
            yield return new MaterialNodeDescription<VRRendererSettings>(this, category: compositionCategory);
            yield return new MaterialNodeDescription<LightShafts>(this, category: compositionCategory);

            // Post processing
            var postProcessingCategory = $"{renderingCategory}.PostProcessing";
            yield return new MaterialNodeDescription<PostProcessingEffects>(this, category: postProcessingCategory);
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
            //yield return new MaterialNodeDescription<MeshRenderFeature>(this, category: renderingCategory)
            //{
            //    nameof(MeshRenderFeature.RenderFeatures), nameof(MeshRenderFeature.RenderStageSelectors), nameof(MeshRenderFeature.PipelineProcessors)
            //};
            yield return new MaterialNodeDescription<BackgroundRenderFeature>(this, category: renderingCategory);
            yield return new MaterialNodeDescription<SpriteRenderFeature>(this, category: renderingCategory);

            yield return new MaterialNodeDescription<MeshPipelineProcessor>(this, category: renderingCategory);
            yield return new MaterialNodeDescription<ShadowMeshPipelineProcessor>(this, category: renderingCategory);
            yield return new MaterialNodeDescription<WireframePipelineProcessor>(this, category: renderingCategory);

            // Sub render features - make enum
            var subRenderFeaturesCategory = $"{renderingCategory}.SubRenderFeatures";
            yield return new MaterialNodeDescription<TransformRenderFeature>(this, category: subRenderFeaturesCategory);
            yield return new MaterialNodeDescription<SkinningRenderFeature>(this, category: subRenderFeaturesCategory);
            yield return new MaterialNodeDescription<MaterialRenderFeature>(this, category: subRenderFeaturesCategory);
            yield return new MaterialNodeDescription<ShadowCasterRenderFeature>(this, category: subRenderFeaturesCategory);
            yield return new MaterialNodeDescription<ForwardLightingRenderFeature>(this, category: subRenderFeaturesCategory);

            // Light renderers - make enum
            var lightsCategory = $"{subRenderFeaturesCategory}.Lights";
            yield return new MaterialNodeDescription<LightAmbientRenderer>(this, category: lightsCategory);
            yield return new MaterialNodeDescription<LightSkyboxRenderer>(this, category: lightsCategory);
            yield return new MaterialNodeDescription<LightDirectionalGroupRenderer>(this, category: lightsCategory);
            yield return new MaterialNodeDescription<LightPointGroupRenderer>(this, category: lightsCategory);
            yield return new MaterialNodeDescription<LightSpotGroupRenderer>(this, category: lightsCategory);
            yield return new MaterialNodeDescription<LightClusteredPointSpotGroupRenderer>(this, category: lightsCategory);

            // Shadow map renderers
            var shadowsCategory = $"{subRenderFeaturesCategory}.Shadows";
            yield return new MaterialNodeDescription<ShadowMapRenderer>(this, category: shadowsCategory);
            yield return new MaterialNodeDescription<LightDirectionalShadowMapRenderer>(this, category: shadowsCategory);
            yield return new MaterialNodeDescription<LightSpotShadowMapRenderer>(this, category: shadowsCategory);
            yield return new MaterialNodeDescription<LightPointShadowMapRendererParaboloid>(this, category: shadowsCategory);
            yield return new MaterialNodeDescription<LightPointShadowMapRendererCubeMap>(this, category: shadowsCategory);

            yield return new CustomNodeDesc<MeshRenderFeature>(this)
                .AddInput<IReadOnlyList<SubRenderFeature>>(nameof(MeshRenderFeature.RenderFeatures), (x, v) =>
                {
                    if (!x.RenderFeatures.SequenceEqual(v))
                    {
                        x.RenderFeatures.Clear();
                        x.RenderFeatures.AddRange(v);
                    }
                })
                .AddInput<IReadOnlyList<RenderStageSelector>>(nameof(MeshRenderFeature.RenderStageSelectors), (x, v) =>
                {
                    if (!x.RenderStageSelectors.SequenceEqual(v))
                    {
                        x.RenderStageSelectors.Clear();
                        x.RenderStageSelectors.AddRange(v);
                    }
                })
                .AddInput<IReadOnlyList<PipelineProcessor>>(nameof(MeshRenderFeature.PipelineProcessors), (x, v) =>
                {
                    if (!x.PipelineProcessors.SequenceEqual(v))
                    {
                        x.PipelineProcessors.Clear();
                        x.PipelineProcessors.AddRange(v);
                    }
                })
                .AddOutput("Output", x => x);

            yield return new CustomNodeDesc<MeshTransparentRenderStageSelector>(this, category: renderStageSelectorCategory)
                .AddInput(nameof(MeshTransparentRenderStageSelector.EffectName), (x, v) => x.EffectName = v, defaultValue: "StrideForwardShadingEffect")
                .AddInput<RenderStage>(nameof(MeshTransparentRenderStageSelector.OpaqueRenderStage), (x, v) => x.OpaqueRenderStage= v)
                .AddInput<RenderStage>(nameof(MeshTransparentRenderStageSelector.TransparentRenderStage), (x, v) => x.TransparentRenderStage = v)
                .AddInput<RenderGroupMask>(nameof(MeshTransparentRenderStageSelector.RenderGroup), (x, v) => x.RenderGroup = v)
                .AddOutput("Output", x => x);
        }
    }
}
