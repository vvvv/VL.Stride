using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Shaders;
using System.Collections.Generic;
using VL.Core;

namespace VL.Stride.Rendering.Materials
{
    static class MaterialNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(StrideNodeFactory nodeFactory)
        {
            string materialCategory = "Stride.Materials";
            string geometryCategory = $"{materialCategory}.{nameof(GeometryAttributes)}";
            string shadingCategory = $"{materialCategory}.{nameof(ShadingAttributes)}";
            string miscCategory = $"{materialCategory}.{nameof(MiscAttributes)}";
            string transparencyCategory = $"{miscCategory}.Transparency";
            string layersCategory = $"{materialCategory}.Layers";
            string diffuseModelCategory = $"{shadingCategory}.DiffuseModel";
            string specularModelCategory = $"{shadingCategory}.SpecularModel";

            // Geometry
            yield return nodeFactory.NewNode<GeometryAttributes>(category: materialCategory, fragmented: true)
                .AddInput(nameof(GeometryAttributes.Tessellation), x => x.Tessellation, (x, v) => x.Tessellation = v)
                .AddInput(nameof(GeometryAttributes.Displacement), x => x.Displacement, (x, v) => x.Displacement = v)
                .AddInput(nameof(GeometryAttributes.Surface), x => x.Surface, (x, v) => x.Surface = v)
                .AddInput(nameof(GeometryAttributes.MicroSurface), x => x.MicroSurface, (x, v) => x.MicroSurface = v);

            yield return NewMaterialNode<MaterialTessellationFlatFeature>(nodeFactory, "FlatTesselation", geometryCategory);
            yield return NewMaterialNode<MaterialTessellationPNFeature>(nodeFactory, "PointNormalTessellation", geometryCategory);
            yield return NewMaterialNode<MaterialDisplacementMapFeature>(nodeFactory, "DisplacementMap", geometryCategory);
            yield return NewMaterialNode<MaterialNormalMapFeature>(nodeFactory, "NormalMap", geometryCategory);
            yield return NewMaterialNode<MaterialGlossinessMapFeature>(nodeFactory, "GlossMap", geometryCategory);

            // Shading
            yield return nodeFactory.NewNode<ShadingAttributes>(category: materialCategory, fragmented: true)
                .AddInput(nameof(ShadingAttributes.Diffuse), x => x.Diffuse, (x, v) => x.Diffuse = v)
                .AddInput(nameof(ShadingAttributes.DiffuseModel), x => x.DiffuseModel, (x, v) => x.DiffuseModel = v)
                .AddInput(nameof(ShadingAttributes.Specular), x => x.Specular, (x, v) => x.Specular = v)
                // Hack to workaround equality bug (https://github.com/stride3d/stride/issues/735)
                .AddInputWithRefEquality(nameof(ShadingAttributes.SpecularModel), x => x.SpecularModel, (x, v) => x.SpecularModel = v)
                .AddInput(nameof(ShadingAttributes.Emissive), x => x.Emissive, (x, v) => x.Emissive = v)
                .AddInput(nameof(ShadingAttributes.SubsurfaceScattering), x => x.SubsurfaceScattering, (x, v) => x.SubsurfaceScattering = v);

            yield return NewMaterialNode<MaterialDiffuseMapFeature>(nodeFactory, "DiffuseMap", shadingCategory);

            yield return NewMaterialNode<MaterialDiffuseCelShadingModelFeature>(nodeFactory, "CelShading", diffuseModelCategory);
            yield return NewMaterialNode<MaterialDiffuseHairModelFeature>(nodeFactory, "Hair", diffuseModelCategory);
            yield return NewMaterialNode<MaterialDiffuseLambertModelFeature>(nodeFactory, "Lambert", diffuseModelCategory);

            yield return NewMaterialNode<MaterialMetalnessMapFeature>(nodeFactory, "MetalnessMap", shadingCategory);
            yield return NewMaterialNode<MaterialSpecularMapFeature>(nodeFactory, "SpecularMap", shadingCategory);

            yield return nodeFactory.NewNode<MaterialSpecularCelShadingModelFeature>("CelShading", specularModelCategory, fragmented: true)
                .AddInputs()
                .AddInput(nameof(MaterialSpecularCelShadingModelFeature.RampFunction), x => x.RampFunction, (x, v) => x.RampFunction = v);

            yield return NewMaterialNode<MaterialCelShadingLightDefault>(nodeFactory, "DefaultLightFunction", $"{specularModelCategory}.CelShading");
            yield return NewMaterialNode<MaterialCelShadingLightRamp>(nodeFactory, "RampLightFunction", $"{specularModelCategory}.CelShading");

            yield return NewMaterialNode<MaterialSpecularHairModelFeature>(nodeFactory, "Hair", specularModelCategory);

            yield return nodeFactory.NewNode<MaterialSpecularMicrofacetModelFeature>("Microfacet", specularModelCategory, fragmented: true)
                .AddInputs();

            var defaultGlass = new MaterialSpecularThinGlassModelFeature();
            yield return nodeFactory.NewNode<MaterialSpecularThinGlassModelFeature>("Glass", specularModelCategory, fragmented: true)
                .AddInputs()
                .AddInput(nameof(MaterialSpecularThinGlassModelFeature.RefractiveIndex), x => x.RefractiveIndex, (x, v) => x.RefractiveIndex = v, defaultGlass.RefractiveIndex);

            yield return NewMaterialNode<MaterialEmissiveMapFeature>(nodeFactory, "EmissiveMap", shadingCategory);
            yield return NewMaterialNode<MaterialSubsurfaceScatteringFeature>(nodeFactory, "SubsurfaceScattering", shadingCategory);

            // Misc
            yield return nodeFactory.NewNode<MiscAttributes>(category: materialCategory, fragmented: true)
                .AddInput(nameof(MiscAttributes.Occlusion), x => x.Occlusion, (x, v) => x.Occlusion = v)
                .AddInput(nameof(MiscAttributes.Transparency), x => x.Transparency, (x, v) => x.Transparency = v)
                .AddInput(nameof(MiscAttributes.Overrides), x => x.Overrides, (x, v) => x.Overrides = v)
                .AddInput(nameof(MiscAttributes.CullMode), x => x.CullMode, (x, v) => x.CullMode = v, CullMode.Back)
                .AddInput(nameof(MiscAttributes.ClearCoat), x => x.ClearCoat, (x, v) => x.ClearCoat = v);
            yield return NewMaterialNode<MaterialOcclusionMapFeature>(nodeFactory, "OcclusionMap", miscCategory);
            yield return NewMaterialNode<MaterialTransparencyAdditiveFeature>(nodeFactory, "Additive", transparencyCategory);
            yield return NewMaterialNode<MaterialTransparencyBlendFeature>(nodeFactory, "Blend", transparencyCategory);
            yield return NewMaterialNode<MaterialTransparencyCutoffFeature>(nodeFactory, "Cutoff", transparencyCategory);
            yield return NewMaterialNode<MaterialClearCoatFeature>(nodeFactory, "ClearCoat", miscCategory);

            // Layers
            yield return NewMaterialNode<MaterialBlendLayer>(nodeFactory, "MaterialLayer", layersCategory);
            yield return NewMaterialNode<MaterialOverrides>(nodeFactory, "LayerOverrides", layersCategory);

            // Top level
            yield return nodeFactory.NewNode<GroupedMaterialAttributes>(name: "MaterialAttributes", category: materialCategory, hasStateOutput: false, fragmented: true)
                .AddInput(nameof(GroupedMaterialAttributes.Geometry), x => x.Geometry, (x, v) => x.Geometry = v)
                .AddInput(nameof(GroupedMaterialAttributes.Shading), x => x.Shading, (x, v) => x.Shading = v)
                .AddInput(nameof(GroupedMaterialAttributes.Misc), x => x.Misc, (x, v) => x.Misc = v)
                .AddCachedOutput("Output", x => x.ToMaterialAttributes());

            yield return nodeFactory.NewNode<MaterialDescriptor>(nameof(MaterialDescriptor), materialCategory, fragmented: true)
                .AddInput(nameof(MaterialDescriptor.Attributes), x => x.Attributes, (x, v) => x.Attributes = v)
                .AddListInput(nameof(MaterialDescriptor.Layers), x => x.Layers);

            // Not so sure about these - they work for now
            yield return nodeFactory.NewNode<LiveComputeColor>(
                name: nameof(ComputeColor), 
                category: materialCategory, 
                copyOnWrite: false,
                hasStateOutput: false,
                fragmented: true)
                .AddInput(
                    name: nameof(ComputeColor.Value),
                    getter: x => x.Value,
                    setter: (x, v) =>
                    {
                        x.Value = v;
                        x.Parameters?.Set(x.Accessor, v);
                    },
                    defaultValue: Color4.White)
                .AddOutput<ComputeColor>("Output", x => x);

            yield return nodeFactory.NewNode<LiveComputeFloat>(
                name: nameof(ComputeFloat), 
                category: materialCategory, 
                copyOnWrite: false,
                hasStateOutput: false,
                fragmented: true)
                .AddInput(
                    name: nameof(ComputeFloat.Value),
                    getter: x => x.Value,
                    setter: (x, v) =>
                    {
                        x.Value = v;
                        x.Parameters?.Set(x.Accessor, x.Value);
                    },
                    defaultValue: 1f)
                .AddOutput<ComputeFloat>("Output", x => x);

            yield return NewMaterialNode<ComputeTextureColor>(nodeFactory, nameof(ComputeTextureColor), materialCategory);
            yield return NewMaterialNode<ComputeTextureScalar>(nodeFactory, nameof(ComputeTextureScalar), materialCategory);
        }

        static StrideNodeDesc<T> NewMaterialNode<T>(this IVLNodeDescriptionFactory nodeFactory, string name, string category)
            where T : new()
        {
            return new StrideNodeDesc<T>(nodeFactory, name, category, isFragmented: true);
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
