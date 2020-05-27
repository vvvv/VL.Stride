using Stride.Graphics;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using System;
using System.Collections.Generic;
using VL.Core;

namespace VL.Stride.Rendering.Materials
{
    static class MaterialNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(StrideNodeFactory nodeFactory)
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
            yield return new StrideNodeDesc<MaterialTessellationFlatFeature>(nodeFactory, "FlatTesselation", geometryCategory);
            yield return new StrideNodeDesc<MaterialTessellationPNFeature>(nodeFactory, "PointNormalTessellation", geometryCategory);
            yield return new StrideNodeDesc<MaterialDisplacementMapFeature>(nodeFactory, "DisplacementMap", geometryCategory);
            yield return new StrideNodeDesc<MaterialNormalMapFeature>(nodeFactory, "NormalMap", geometryCategory);
            yield return new StrideNodeDesc<MaterialGlossinessMapFeature>(nodeFactory, "GlossMap", geometryCategory);

            // Shading
            yield return new StrideNodeDesc<MaterialDiffuseMapFeature>(nodeFactory, "DiffuseMap", shadingCategory);

            yield return new StrideNodeDesc<MaterialDiffuseCelShadingModelFeature>(nodeFactory, "CelShading", diffuseModelCategory);
            yield return new StrideNodeDesc<MaterialDiffuseHairModelFeature>(nodeFactory, "Hair", diffuseModelCategory);
            yield return new StrideNodeDesc<MaterialDiffuseLambertModelFeature>(nodeFactory, "Lambert", diffuseModelCategory);

            yield return new StrideNodeDesc<MaterialMetalnessMapFeature>(nodeFactory, "MetalnessMap", shadingCategory);
            yield return new StrideNodeDesc<MaterialSpecularMapFeature>(nodeFactory, "SpecularMap", shadingCategory);

            yield return nodeFactory.Create<MaterialSpecularCelShadingModelFeature>("CelShading", specularModelCategory)
                .AddInputs()
                .AddInput(nameof(MaterialSpecularCelShadingModelFeature.RampFunction), x => x.RampFunction, (x, v) => x.RampFunction = v);

            yield return new StrideNodeDesc<MaterialCelShadingLightDefault>(nodeFactory, "DefaultLightFunction", $"{specularModelCategory}.CelShading");
            yield return new StrideNodeDesc<MaterialCelShadingLightRamp>(nodeFactory, "RampLightFunction", $"{specularModelCategory}.CelShading");

            yield return new StrideNodeDesc<MaterialSpecularHairModelFeature>(nodeFactory, "Hair", specularModelCategory);

            yield return nodeFactory.Create<MaterialSpecularMicrofacetModelFeature>("Microfacet", specularModelCategory)
                .AddInputs();

            var defaultGlass = new MaterialSpecularThinGlassModelFeature();
            yield return nodeFactory.Create<MaterialSpecularThinGlassModelFeature>("Glass", specularModelCategory)
                .AddInputs()
                .AddInput(nameof(MaterialSpecularThinGlassModelFeature.RefractiveIndex), x => x.RefractiveIndex, (x, v) => x.RefractiveIndex = v, defaultGlass.RefractiveIndex);

            yield return new StrideNodeDesc<MaterialEmissiveMapFeature>(nodeFactory, "EmissiveMap", shadingCategory);
            yield return new StrideNodeDesc<MaterialSubsurfaceScatteringFeature>(nodeFactory, "SubsurfaceScattering", shadingCategory);

            // Misc
            yield return new StrideNodeDesc<MaterialOcclusionMapFeature>(nodeFactory, "OcclusionMap", miscCategory);
            yield return new StrideNodeDesc<MaterialTransparencyAdditiveFeature>(nodeFactory, "Additive", transparencyCategory);
            yield return new StrideNodeDesc<MaterialTransparencyBlendFeature>(nodeFactory, "Blend", transparencyCategory);
            yield return new StrideNodeDesc<MaterialTransparencyCutoffFeature>(nodeFactory, "Cutoff", transparencyCategory);
            yield return new StrideNodeDesc<MaterialClearCoatFeature>(nodeFactory, "ClearCoat", miscCategory);

            // Layers
            yield return new StrideNodeDesc<MaterialBlendLayer>(nodeFactory, "MaterialLayer", layersCategory);
            yield return new StrideNodeDesc<MaterialOverrides>(nodeFactory, "LayerOverrides", layersCategory);

            // Top level
            // TODO: The Overrides property is a getter only - we need to expose the inner properties
            yield return new StrideNodeDesc<MaterialAttributes>(nodeFactory, "MaterialAttributes", materialCategory);
            yield return nodeFactory.Create<MaterialDescriptor>(nameof(MaterialDescriptor), materialCategory)
                .AddInput(nameof(MaterialDescriptor.Attributes), x => x.Attributes, (x, v) => x.Attributes = v)
                .AddListInput(nameof(MaterialDescriptor.Layers), x => x.Layers);

            // Not so sure about these - they work for now
            yield return new StrideNodeDesc<ComputeColor>(nodeFactory, "ComputeColor", materialCategory);
            yield return new StrideNodeDesc<ComputeFloat>(nodeFactory, "ComputeFloat", materialCategory);
            yield return new StrideNodeDesc<ComputeTextureColor>(nodeFactory, "ComputeTextureColor", materialCategory);
            yield return new StrideNodeDesc<ComputeTextureScalar>(nodeFactory, "ComputeTextureScalar", materialCategory);
        }

        static CustomNodeDesc<T> AddInputs<T>(this CustomNodeDesc<T> node)
            where T : MaterialSpecularMicrofacetModelFeature, new()
        {
            var i = new T();
            return node
                .AddInput(nameof(MaterialSpecularMicrofacetModelFeature.Fresnel), x => x.Fresnel.ToEnum(), (x, v) => x.Fresnel = v.ToFunction(), i.Fresnel.ToEnum())
                .AddInput(nameof(MaterialSpecularMicrofacetModelFeature.Visibility), x => x.Visibility.ToEnum(), (x, v) => x.Visibility = v.ToFunction(), i.Visibility.ToEnum())
                .AddInput(nameof(MaterialSpecularMicrofacetModelFeature.NormalDistribution), x => x.NormalDistribution.ToEnum(), (x, v) => x.NormalDistribution = v.ToFunction(), i.NormalDistribution.ToEnum())
                .AddInput(nameof(MaterialSpecularMicrofacetModelFeature.Environment), x => x.Environment.ToEnum(), (x, v) => x.Environment = v.ToFunction(), i.Environment.ToEnum());
        }
    }
}
