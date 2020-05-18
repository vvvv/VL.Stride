using SharpDX.Direct3D12;
using Stride.Core;
using Stride.Core.Annotations;
using Stride.Rendering.Materials;
using System;

namespace VL.Stride.Materials
{
    // Little wrapper exposing the function as enums. Makes it easier to use in VL.

    /// <summary>
    /// The microfacet specular shading model.
    /// </summary>
    [Display("Microfacet")]
    public class MaterialSpecularMicrofacetModelFeatureUsingEnums : MaterialFeature, IMaterialSpecularModelFeature, IEquatable<MaterialSpecularMicrofacetModelFeatureUsingEnums>
    {
        protected readonly MaterialSpecularMicrofacetModelFeature feature = new MaterialSpecularMicrofacetModelFeature();

        public MaterialSpecularMicrofacetModelFeatureUsingEnums()
            : this (new MaterialSpecularMicrofacetModelFeature())
        {

        }

        protected MaterialSpecularMicrofacetModelFeatureUsingEnums(MaterialSpecularMicrofacetModelFeature feature)
        {
            this.feature = feature;
        }

        /// <userdoc>Specify the function to use to calculate the Fresnel component of the micro-facet lighting equation. 
        /// This defines the amount of the incoming light that is reflected.</userdoc>
        [DataMember(10)]
        [Display("Fresnel")]
        public MicrofacetFresnelFunction Fresnel
        {
            get => feature.Fresnel.ToEnum();
            set => feature.Fresnel = value.ToFunction();
        }

        /// <userdoc>Specify the function to use to calculate the visibility component of the micro-facet lighting equation.</userdoc>
        [DataMember(20)]
        [Display("Visibility")]
        public MicrofacetVisibilityFunction Visibility
        {
            get => feature.Visibility.ToEnum();
            set => feature.Visibility = value.ToFunction();
        }

        /// <userdoc>Specify the function to use to calculate the normal distribution in the micro-facet lighting equation. 
        /// This defines how the normal is distributed.</userdoc>
        [DataMember(30)]
        [Display("Normal Distribution")]
        public MicrofacetNormalDistributionFunction NormalDistribution
        {
            get => feature.NormalDistribution.ToEnum();
            set => feature.NormalDistribution = value.ToFunction();
        }

        /// <userdoc>Specify the function to use to calculate the environment DFG term in the micro-facet lighting equation. 
        /// This defines how the material reflects specular cubemaps.</userdoc>
        [DataMember(30)]
        [Display("Environment (DFG)")]
        public MicrofacetEnvironmentFunction Environment
        {
            get => feature.Environment.ToEnum();
            set => feature.Environment = value.ToFunction();
        }

        public override sealed void GenerateShader(MaterialGeneratorContext context)
        {
            feature.GenerateShader(context);
        }

        public override sealed void MultipassGeneration(MaterialGeneratorContext context)
        {
            feature.MultipassGeneration(context);
        }

        public bool Equals(IMaterialShadingModelFeature other)
        {
            return Equals((object)other);
        }

        public bool Equals(MaterialSpecularMicrofacetModelFeatureUsingEnums other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return feature.Equals(other.feature);
        }

        public override sealed bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals(obj as MaterialSpecularMicrofacetModelFeatureUsingEnums);
        }

        public override sealed int GetHashCode() => feature.GetHashCode();
    }

    public class MaterialSpecularCelShadingModelFeatureUsingEnums : MaterialSpecularMicrofacetModelFeatureUsingEnums, IEquatable<MaterialSpecularCelShadingModelFeatureUsingEnums>
    {
        public MaterialSpecularCelShadingModelFeatureUsingEnums()
            : base(new MaterialSpecularCelShadingModelFeature())
        {

        }

        new MaterialSpecularCelShadingModelFeature feature => base.feature as MaterialSpecularCelShadingModelFeature;

        [DataMember(5)]
        [Display("Ramp Function")]
        public IMaterialCelShadingLightFunction RampFunction
        {
            get => feature.RampFunction;
            set => feature.RampFunction = value;
        }

        public bool Equals(MaterialSpecularCelShadingModelFeatureUsingEnums other)
        {
            return base.Equals(other);
        }
    }

    public class MaterialSpecularThinGlassModelFeatureUsingEnums : MaterialSpecularMicrofacetModelFeatureUsingEnums, IEquatable<MaterialSpecularThinGlassModelFeatureUsingEnums>
    {
        public MaterialSpecularThinGlassModelFeatureUsingEnums()
            : base(new MaterialSpecularThinGlassModelFeature())
        {

        }

        new MaterialSpecularThinGlassModelFeature feature => base.feature as MaterialSpecularThinGlassModelFeature;

        /// <summary>
        /// Gets or sets the refractive index of the material.
        /// </summary>
        /// <value>The alpha.</value>
        /// <userdoc>An additional factor that can be used to modulate original alpha of the material.</userdoc>
        [DataMember(2)]
        [DataMemberRange(1.0, 5.0, 0.01, 0.1, 3)]
        public float RefractiveIndex
        {
            get => feature.RefractiveIndex;
            set => feature.RefractiveIndex = value;
        }

        public bool Equals(MaterialSpecularThinGlassModelFeatureUsingEnums other)
        {
            return base.Equals(other);
        }
    }

    public enum MicrofacetEnvironmentFunction
    {
        GGXLUT,
        GGXPolynomial,
        ThinGlass
    }

    public enum MicrofacetFresnelFunction
    {
        None,
        Schlick,
        ThinGlass
    }

    public enum MicrofacetNormalDistributionFunction
    {
        Beckmann,
        BlinnPhong,
        GGX
    }

    public enum MicrofacetVisibilityFunction
    {
        CookTorrance,
        Implicit,
        Kelemen,
        SmithBeckmann,
        SmithGGXCorrelated,
        SmithSchlickBeckmann,
        SmithSchlickGGX
    }

    public static class EnumExtensions
    {
        public static IMaterialSpecularMicrofacetEnvironmentFunction ToFunction(this MicrofacetEnvironmentFunction value)
        {
            switch (value)
            {
                case MicrofacetEnvironmentFunction.GGXLUT:
                    return new MaterialSpecularMicrofacetEnvironmentGGXLUT();
                case MicrofacetEnvironmentFunction.GGXPolynomial:
                    return new MaterialSpecularMicrofacetEnvironmentGGXPolynomial();
                case MicrofacetEnvironmentFunction.ThinGlass:
                    return new MaterialSpecularMicrofacetEnvironmentThinGlass();
                default:
                    throw new NotImplementedException();
            }
        }

        public static MicrofacetEnvironmentFunction ToEnum(this IMaterialSpecularMicrofacetEnvironmentFunction value)
        {
            if (value is MaterialSpecularMicrofacetEnvironmentGGXLUT)
                return MicrofacetEnvironmentFunction.GGXLUT;
            if (value is MaterialSpecularMicrofacetEnvironmentGGXPolynomial)
                return MicrofacetEnvironmentFunction.GGXPolynomial;
            if (value is MaterialSpecularMicrofacetEnvironmentThinGlass)
                return MicrofacetEnvironmentFunction.ThinGlass;
            throw new NotImplementedException();
        }

        public static IMaterialSpecularMicrofacetFresnelFunction ToFunction(this MicrofacetFresnelFunction value)
        {
            switch (value)
            {
                case MicrofacetFresnelFunction.None:
                    return new MaterialSpecularMicrofacetFresnelNone();
                case MicrofacetFresnelFunction.Schlick:
                    return new MaterialSpecularMicrofacetFresnelSchlick();
                case MicrofacetFresnelFunction.ThinGlass:
                    return new MaterialSpecularMicrofacetFresnelThinGlass();
                default:
                    throw new NotImplementedException();
            }
        }

        public static MicrofacetFresnelFunction ToEnum(this IMaterialSpecularMicrofacetFresnelFunction value)
        {
            if (value is MaterialSpecularMicrofacetFresnelNone)
                return MicrofacetFresnelFunction.None;
            if (value is MaterialSpecularMicrofacetFresnelSchlick)
                return MicrofacetFresnelFunction.Schlick;
            if (value is MaterialSpecularMicrofacetFresnelThinGlass)
                return MicrofacetFresnelFunction.ThinGlass;
            throw new NotImplementedException();
        }

        public static IMaterialSpecularMicrofacetNormalDistributionFunction ToFunction(this MicrofacetNormalDistributionFunction value)
        {
            switch (value)
            {
                case MicrofacetNormalDistributionFunction.Beckmann:
                    return new MaterialSpecularMicrofacetNormalDistributionBeckmann();
                case MicrofacetNormalDistributionFunction.BlinnPhong:
                    return new MaterialSpecularMicrofacetNormalDistributionBlinnPhong();
                case MicrofacetNormalDistributionFunction.GGX:
                    return new MaterialSpecularMicrofacetNormalDistributionGGX();
                default:
                    throw new NotImplementedException();
            }
        }

        public static MicrofacetNormalDistributionFunction ToEnum(this IMaterialSpecularMicrofacetNormalDistributionFunction value)
        {
            if (value is MaterialSpecularMicrofacetNormalDistributionBeckmann)
                return MicrofacetNormalDistributionFunction.Beckmann;
            if (value is MaterialSpecularMicrofacetNormalDistributionBlinnPhong)
                return MicrofacetNormalDistributionFunction.BlinnPhong;
            if (value is MaterialSpecularMicrofacetNormalDistributionGGX)
                return MicrofacetNormalDistributionFunction.GGX;
            throw new NotImplementedException();
        }

        public static IMaterialSpecularMicrofacetVisibilityFunction ToFunction(this MicrofacetVisibilityFunction value)
        {
            switch (value)
            {
                case MicrofacetVisibilityFunction.CookTorrance:
                    return new MaterialSpecularMicrofacetVisibilityCookTorrance();
                case MicrofacetVisibilityFunction.Implicit:
                    return new MaterialSpecularMicrofacetVisibilityImplicit();
                case MicrofacetVisibilityFunction.Kelemen:
                    return new MaterialSpecularMicrofacetVisibilityKelemen();
                case MicrofacetVisibilityFunction.SmithBeckmann:
                    return new MaterialSpecularMicrofacetVisibilitySmithBeckmann();
                case MicrofacetVisibilityFunction.SmithGGXCorrelated:
                    return new MaterialSpecularMicrofacetVisibilitySmithGGXCorrelated();
                case MicrofacetVisibilityFunction.SmithSchlickBeckmann:
                    return new MaterialSpecularMicrofacetVisibilitySmithSchlickBeckmann();
                case MicrofacetVisibilityFunction.SmithSchlickGGX:
                    return new MaterialSpecularMicrofacetVisibilitySmithSchlickGGX();
                default:
                    throw new NotImplementedException();
            }
        }

        public static MicrofacetVisibilityFunction ToEnum(this IMaterialSpecularMicrofacetVisibilityFunction value)
        {
            if (value is MaterialSpecularMicrofacetVisibilityCookTorrance)
                return MicrofacetVisibilityFunction.CookTorrance;
            if (value is MaterialSpecularMicrofacetVisibilityImplicit)
                return MicrofacetVisibilityFunction.Implicit;
            if (value is MaterialSpecularMicrofacetVisibilityKelemen)
                return MicrofacetVisibilityFunction.Kelemen;
            if (value is MaterialSpecularMicrofacetVisibilitySmithBeckmann)
                return MicrofacetVisibilityFunction.SmithBeckmann;
            if (value is MaterialSpecularMicrofacetVisibilitySmithGGXCorrelated)
                return MicrofacetVisibilityFunction.SmithGGXCorrelated;
            if (value is MaterialSpecularMicrofacetVisibilitySmithSchlickBeckmann)
                return MicrofacetVisibilityFunction.SmithSchlickBeckmann;
            if (value is MaterialSpecularMicrofacetVisibilitySmithSchlickGGX)
                return MicrofacetVisibilityFunction.SmithSchlickGGX;
            throw new NotImplementedException();
        }
    }
}
