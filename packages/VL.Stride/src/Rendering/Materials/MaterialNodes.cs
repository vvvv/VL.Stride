using Stride.Core.Mathematics;
using Stride.Core.Serialization;
using Stride.Core.Serialization.Contents;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using VL.Core;
using VL.Stride.Shaders.ShaderFX;

namespace VL.Stride.Rendering.Materials
{
    static class MaterialNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(StrideNodeFactory nodeFactory)
        {
            string renderingCategory = "Stride.Rendering";
            string materialCategory = $"{renderingCategory}.Materials";
            string geometryCategory = $"{materialCategory}.{nameof(GeometryAttributes)}";
            string shadingCategory = $"{materialCategory}.{nameof(ShadingAttributes)}";
            string miscCategory = $"{materialCategory}.{nameof(MiscAttributes)}";
            string transparencyCategory = $"{miscCategory}.Transparency";
            string layersCategory = $"{materialCategory}.Layers";
            string diffuseModelCategory = $"{shadingCategory}.DiffuseModel";
            string specularModelCategory = $"{shadingCategory}.SpecularModel";

            // Geometry
            yield return nodeFactory.Create<GeometryAttributes>(category: materialCategory)
                .AddInput(nameof(GeometryAttributes.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v)
                .AddInput(nameof(GeometryAttributes.Displacement), x => x.Displacement, (x, v) => x.Displacement = v)
                .AddInput(nameof(GeometryAttributes.Surface), x => x.Surface, (x, v) => x.Surface = v)
                .AddInput(nameof(GeometryAttributes.MicroSurface), x => x.MicroSurface, (x, v) => x.MicroSurface = v);

            yield return new StrideNodeDesc<MaterialTessellationFlatFeature>(nodeFactory, "FlatTesselation", geometryCategory);
            yield return new StrideNodeDesc<MaterialTessellationPNFeature>(nodeFactory, "PointNormalTessellation", geometryCategory);
            yield return new StrideNodeDesc<MaterialDisplacementMapFeature>(nodeFactory, "DisplacementMap", geometryCategory);
            yield return new StrideNodeDesc<MaterialNormalMapFeature>(nodeFactory, "NormalMap", geometryCategory);
            yield return new StrideNodeDesc<MaterialGlossinessMapFeature>(nodeFactory, "GlossMap", geometryCategory);

            // Shading
            yield return nodeFactory.Create<ShadingAttributes>(category: materialCategory)
                .AddInput(nameof(ShadingAttributes.Diffuse), x => x.Diffuse, (x, v) => x.Diffuse = v)
                .AddInput(nameof(ShadingAttributes.DiffuseModel), x => x.DiffuseModel, (x, v) => x.DiffuseModel = v)
                .AddInput(nameof(ShadingAttributes.Specular), x => x.Specular, (x, v) => x.Specular = v)
                // Hack to workaround equality bug (https://github.com/stride3d/stride/issues/735)
                .AddInputWithRefEquality(nameof(ShadingAttributes.SpecularModel), x => x.SpecularModel, (x, v) => x.SpecularModel = v)
                .AddInput(nameof(ShadingAttributes.Emissive), x => x.Emissive, (x, v) => x.Emissive = v)
                .AddInput(nameof(ShadingAttributes.SubsurfaceScattering), x => x.SubsurfaceScattering, (x, v) => x.SubsurfaceScattering = v);

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
            yield return nodeFactory.Create<MiscAttributes>(category: materialCategory)
                .AddInput(nameof(MiscAttributes.Occlusion), x => x.Occlusion, (x, v) => x.Occlusion = v)
                .AddInput(nameof(MiscAttributes.Transparency), x => x.Transparency, (x, v) => x.Transparency = v)
                .AddInput(nameof(MiscAttributes.Overrides), x => x.Overrides, (x, v) => x.Overrides = v)
                .AddInput(nameof(MiscAttributes.CullMode), x => x.CullMode, (x, v) => x.CullMode = v, CullMode.Back)
                .AddInput(nameof(MiscAttributes.ClearCoat), x => x.ClearCoat, (x, v) => x.ClearCoat = v);
            yield return new StrideNodeDesc<MaterialOcclusionMapFeature>(nodeFactory, "OcclusionMap", miscCategory);
            yield return new StrideNodeDesc<MaterialTransparencyAdditiveFeature>(nodeFactory, "Additive", transparencyCategory);
            yield return new StrideNodeDesc<MaterialTransparencyBlendFeature>(nodeFactory, "Blend", transparencyCategory);
            yield return new StrideNodeDesc<MaterialTransparencyCutoffFeature>(nodeFactory, "Cutoff", transparencyCategory);
            yield return new StrideNodeDesc<MaterialClearCoatFeature>(nodeFactory, "ClearCoat", miscCategory);

            // Layers
            yield return new StrideNodeDesc<MaterialBlendLayer>(nodeFactory, "MaterialLayer", layersCategory);
            yield return new StrideNodeDesc<MaterialOverrides>(nodeFactory, "LayerOverrides", layersCategory);

            // Top level
            yield return nodeFactory.Create<GroupedMaterialAttributes>(name: "MaterialAttributes", category: materialCategory, hasStateOutput: false)
                .AddInput(nameof(GroupedMaterialAttributes.Geometry), x => x.Geometry, (x, v) => x.Geometry = v)
                .AddInput(nameof(GroupedMaterialAttributes.Shading), x => x.Shading, (x, v) => x.Shading = v)
                .AddInput(nameof(GroupedMaterialAttributes.Misc), x => x.Misc, (x, v) => x.Misc = v)
                .AddOutput("Output", x => x.ToMaterialAttributes());

            yield return nodeFactory.Create<MaterialDescriptor>(nameof(MaterialDescriptor), materialCategory)
                .AddInput(nameof(MaterialDescriptor.Attributes), x => x.Attributes, (x, v) => x.Attributes = v)
                .AddListInput(nameof(MaterialDescriptor.Layers), x => x.Layers);

            // Not so sure about these - they work for now
            yield return nodeFactory.Create<LiveComputeColor>(
                name: nameof(ComputeColor), 
                category: materialCategory, 
                copyOnWrite: false,
                hasStateOutput: false,
                update: (nc, x) => x.Parameters?.Set(x.Accessor, x.Value))
                .AddInput(nameof(ComputeColor.Value), x => x.Value, (x, v) => x.Value = v, Color4.White)
                .AddOutput<ComputeColor>("Output", x => x);

            yield return nodeFactory.Create<LiveComputeFloat>(
                name: nameof(ComputeFloat), 
                category: materialCategory, 
                copyOnWrite: false,
                hasStateOutput: false,
                update: (nc, x) => x.Parameters?.Set(x.Accessor, x.Value))
                .AddInput(nameof(ComputeFloat.Value), x => x.Value, (x, v) => x.Value = v, 1f)
                .AddOutput<ComputeFloat>("Output", x => x);
            //yield return nodeFactory.Create<ComputeFloat>("ComputeFloat", materialCategory, copyOnWrite: false, hasStateOutput: false)
            //    .AddInput("Input", x => x.Input, (x, v) => x.Input = v)
            //    .AddOutput<IComputeScalar>("Output", x => x);

            yield return new StrideNodeDesc<ComputeTextureColor>(nodeFactory, category: materialCategory);
            yield return new StrideNodeDesc<ComputeTextureScalar>(nodeFactory, category: materialCategory);
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

        static CustomNodeDesc<T> AddStateOutputWithRefEquality<T>(this CustomNodeDesc<T> node)
            where T : MaterialSpecularMicrofacetModelFeature, new()
        {
            var i = new T();
            return node
                .AddOutput("Output", x => x)
                .AddInput(nameof(MaterialSpecularMicrofacetModelFeature.Fresnel), x => x.Fresnel.ToEnum(), (x, v) => x.Fresnel = v.ToFunction(), i.Fresnel.ToEnum())
                .AddInput(nameof(MaterialSpecularMicrofacetModelFeature.Visibility), x => x.Visibility.ToEnum(), (x, v) => x.Visibility = v.ToFunction(), i.Visibility.ToEnum())
                .AddInput(nameof(MaterialSpecularMicrofacetModelFeature.NormalDistribution), x => x.NormalDistribution.ToEnum(), (x, v) => x.NormalDistribution = v.ToFunction(), i.NormalDistribution.ToEnum())
                .AddInput(nameof(MaterialSpecularMicrofacetModelFeature.Environment), x => x.Environment.ToEnum(), (x, v) => x.Environment = v.ToFunction(), i.Environment.ToEnum());
        }

        class LiveComputeFloat : ComputeFloat
        {
            public ParameterCollection Parameters { get; private set; }
            public ValueParameter<float> Accessor { get; private set; }

            public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
            {
                var shaderSource = base.GenerateShaderSource(context, baseKeys);
                Parameters = context.Parameters;
                Accessor = context.Parameters.GetAccessor(UsedKey as ValueParameterKey<float>);
                return shaderSource;
            }
        }

        class LiveComputeColor : ComputeColor
        {
            public ParameterCollection Parameters { get; private set; }
            public ValueParameter<Color4> Accessor { get; private set; }

            public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
            {
                var shaderSource = base.GenerateShaderSource(context, baseKeys);
                Parameters = context.Parameters;
                Accessor = context.Parameters.GetAccessor(UsedKey as ValueParameterKey<Color4>);
                return shaderSource;
            }
        }
    }

    internal class GroupedMaterialAttributes
    {
        readonly MaterialAttributes @default = new MaterialAttributes();

        public GeometryAttributes Geometry { get; set; }
        public ShadingAttributes Shading { get; set; }
        public MiscAttributes Misc { get; set; }

        public MaterialAttributes ToMaterialAttributes()
        {
            return new MaterialAttributes()
            {
                Tessellation = Geometry?.Tessellation,
                Displacement = Geometry?.Displacement,
                Surface = Geometry?.Surface,
                MicroSurface = Geometry?.MicroSurface,
                Diffuse = Shading?.Diffuse,
                DiffuseModel = Shading?.DiffuseModel,
                Specular = Shading?.Specular,
                SpecularModel = Shading?.SpecularModel,
                Emissive = Shading?.Emissive,
                SubsurfaceScattering = Shading?.SubsurfaceScattering,
                Occlusion = Misc?.Occlusion,
                Transparency = Misc?.Transparency,
                Overrides = { UVScale = Misc?.Overrides?.UVScale ?? @default.Overrides.UVScale },
                CullMode = Misc?.CullMode ?? @default.CullMode,
                ClearCoat = Misc?.ClearCoat
            };
        }
    }

    public class GeometryAttributes
    {
        public IMaterialTessellationFeature Tessellation { get; set; }
        public IMaterialDisplacementFeature Displacement { get; set; }
        public IMaterialSurfaceFeature Surface { get; set; }
        public IMaterialMicroSurfaceFeature MicroSurface { get; set; }
    }

    public class ShadingAttributes
    {
        public IMaterialDiffuseFeature Diffuse { get; set; }
        public IMaterialDiffuseModelFeature DiffuseModel { get; set; }
        public IMaterialSpecularFeature Specular { get; set; }
        public IMaterialSpecularModelFeature SpecularModel { get; set; }
        public IMaterialEmissiveFeature Emissive { get; set; }
        public IMaterialSubsurfaceScatteringFeature SubsurfaceScattering { get; set; }
    }

    public class MiscAttributes
    {
        public IMaterialOcclusionFeature Occlusion { get; set; }
        public IMaterialTransparencyFeature Transparency { get; set; }
        public MaterialOverrides Overrides { get; set; }
        public CullMode CullMode { get; set; }
        public IMaterialClearCoatFeature ClearCoat { get; set; }
    }
}
