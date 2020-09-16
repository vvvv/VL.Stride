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
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory nodeFactory)
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
                .AddInput(nameof(ShadingAttributes.SpecularModel), x => x.SpecularModel, (x, v) => x.SpecularModel = v)
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
                    setter: (x, v) => x.SetValue(v),
                    defaultValue: Color4.White)
                .AddInput(nameof(ComputeColor.PremultiplyAlpha), x => x.PremultiplyAlpha, (x, v) => x.PremultiplyAlpha = v, true)
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
                    setter: (x, v) => x.SetValue(v),
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

            public void SetValue(float value)
            {
                Value = value;
                // The value accessors cause very weired behaviour, only updating sometimes. We should figure out why. So use the key for now.
                if (UsedKey is ValueParameterKey<float> floatKey)
                    Parameters?.Set(floatKey, value);
            }

            public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
            {
                var shaderSource = base.GenerateShaderSource(context, baseKeys);
                Parameters = context.Parameters;
                // The value accessors cause very weired behaviour, only updating sometimes. We should figure out why. So use the key for now.
                //Accessor = context.Parameters.GetAccessor(UsedKey as ValueParameterKey<float>);
                return shaderSource;
            }
        }

        class LiveComputeColor : ComputeColor
        {
            private ParameterCollection UsedParameters;
            private ColorSpace UsedColorSpace;

            public void SetValue(Color4 value)
            {
                Value = value; // Already takes care of color space conversion when generating the shader
                if (UsedParameters != null)
                {
                    // But when setting it later while the shader is running (live) we need to do it on our own
                    value = value.ToColorSpace(UsedColorSpace);
                    if (PremultiplyAlpha)
                        value = Color4.PremultiplyAlpha(value);

                    // The value accessors cause very weired behaviour, only updating sometimes. We should figure out why. So use the key for now.
                    if (UsedKey is ValueParameterKey<Color4> color4Key)
                        UsedParameters.Set(color4Key, ref value);
                    else if (UsedKey is ValueParameterKey<Color3> color3Key)
                        UsedParameters.Set(color3Key, value.ToColor3());
                }
            }

            public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
            {
                var shaderSource = base.GenerateShaderSource(context, baseKeys);
                UsedColorSpace = context.ColorSpace;
                UsedParameters = context.Parameters;
                return shaderSource;
            }
        }
    }

    /// <summary>
    /// Material attributes define the core characteristics of a material, such as its diffuse color, diffuse shading model, and so on. Attributes are organized into geometry, shading, and misc.
    /// </summary>
    internal class GroupedMaterialAttributes
    {
        readonly MaterialAttributes @default = new MaterialAttributes();

        /// <summary>
        /// The shape of the material.
        /// </summary>
        public GeometryAttributes Geometry { get; set; }

        /// <summary>
        /// The color characteristics of the material and how it reacts to light.
        /// </summary>
        public ShadingAttributes Shading { get; set; }

        /// <summary>
        /// Occlusion, transparency and material layers.
        /// </summary>
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

    /// <summary>
    /// The material geometry attributes define the shape of a material.
    /// </summary>
    public class GeometryAttributes
    {
        /// <summary>
        /// Gets or sets the tessellation.
        /// </summary>
        /// <value>The tessellation.</value>
        /// <userdoc>The method used for tessellation (subdividing model poligons to increase realism)</userdoc>
        public IMaterialTessellationFeature Tessellation { get; set; }

        /// <summary>
        /// Gets or sets the displacement.
        /// </summary>
        /// <value>The displacement.</value>
        /// <userdoc>The method used for displacement (altering vertex positions by adding offsets)</userdoc>
        public IMaterialDisplacementFeature Displacement { get; set; }

        /// <summary>
        /// Gets or sets the surface.
        /// </summary>
        /// <value>The surface.</value>
        /// <userdoc>The method used to alter macrosurface aspects (eg perturbing the normals of the model)</userdoc>
        public IMaterialSurfaceFeature Surface { get; set; }

        /// <summary>
        /// Gets or sets the micro surface.
        /// </summary>
        /// <value>The micro surface.</value>
        /// <userdoc>The method used to alter the material microsurface</userdoc>
        public IMaterialMicroSurfaceFeature MicroSurface { get; set; }
    }

    /// <summary>
    /// The material shading attributes define the color characteristics of the material and how it reacts to light.
    /// </summary>
    public class ShadingAttributes
    {
        /// <summary>
        /// Gets or sets the diffuse.
        /// </summary>
        /// <value>The diffuse.</value>
        /// <userdoc>The method used to determine the diffuse color of the material. 
        /// The diffuse color is the essential (pure) color of the object without reflections.</userdoc>
        public IMaterialDiffuseFeature Diffuse { get; set; }

        /// <summary>
        /// Gets or sets the diffuse model.
        /// </summary>
        /// <value>The diffuse model.</value>
        /// <userdoc>The shading model used to render the diffuse color.</userdoc>
        public IMaterialDiffuseModelFeature DiffuseModel { get; set; }

        /// <summary>
        /// Gets or sets the specular.
        /// </summary>
        /// <value>The specular.</value>
        /// <userdoc>The method used to determine the specular color. 
        /// This is the color produced by the reflection of a white light on the object.</userdoc>
        public IMaterialSpecularFeature Specular { get; set; }

        /// <summary>
        /// Gets or sets the specular model.
        /// </summary>
        /// <value>The specular model.</value>
        /// <userdoc>The shading model used to render the material specular color</userdoc>
        public IMaterialSpecularModelFeature SpecularModel { get; set; }

        /// <summary>
        /// Gets or sets the emissive.
        /// </summary>
        /// <value>The emissive.</value>
        /// <userdoc>The method used to determine the emissive color (the color emitted by the object)
        /// </userdoc>
        public IMaterialEmissiveFeature Emissive { get; set; }
        public IMaterialSubsurfaceScatteringFeature SubsurfaceScattering { get; set; }
    }

    /// <summary>
    /// The material misc attributes allow to set the occulsion, transparency and material layers.
    /// </summary>
    public class MiscAttributes
    {
        /// <summary>
        /// Gets or sets the occlusion.
        /// </summary>
        /// <value>The occlusion.</value>
        /// <userdoc>The occlusion method. Occlusions modulate the ambient and direct lighting of the material to simulate shadows or cavity artifacts.
        /// </userdoc>
        public IMaterialOcclusionFeature Occlusion { get; set; }

        /// <summary>
        /// Gets or sets the transparency.
        /// </summary>
        /// <value>The transparency.</value>
        /// <userdoc>The method used to determine the transparency</userdoc>
        public IMaterialTransparencyFeature Transparency { get; set; }

        /// <summary>
        /// Gets or sets the overrides.
        /// </summary>
        /// <value>The overrides.</value>
        /// <userdoc>Override properties of the current material</userdoc>
        public MaterialOverrides Overrides { get; set; }

        /// <summary>
        /// Gets or sets the cull mode used for the material.
        /// </summary>
        /// <userdoc>Cull some faces of the model depending on orientation</userdoc>
        public CullMode CullMode { get; set; }

        /// <summary>
        /// Gets or sets the clear coat shading features for the material.
        /// </summary>
        /// <userdoc>Use clear-coat shading to simulate vehicle paint</userdoc>
        public IMaterialClearCoatFeature ClearCoat { get; set; }
    }
}
