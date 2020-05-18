using Stride.Core;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using VL.Core;

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
            string materialCategory = "Stride.Rendering.Materials";
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
        }
    }
}
