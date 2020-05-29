using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Colors;
using Stride.Rendering.ComputeEffect.GGXPrefiltering;
using Stride.Rendering.ComputeEffect.LambertianPrefiltering;
using Stride.Rendering.Lights;
using Stride.Rendering.Materials;
using Stride.Rendering.Skyboxes;
using Stride.Shaders;
using System;
using System.Collections.Generic;
using System.Text;
using VL.Core;
using VL.Lib.Basics.Resources;

namespace VL.Stride.Rendering.Lights
{
    static class LightNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            var lightsCategory = "Stride.Rendering.Lights";

            yield return factory.NewColorLightNode<LightAmbient>(lightsCategory);

            yield return factory.NewEnvironmentLightNode<LightSkybox>(lightsCategory)
                .AddInput(nameof(LightSkybox.Skybox), x => x.Skybox, (x, v) => x.Skybox = v);

            yield return factory.NewSkyboxNode(lightsCategory);

            yield return factory.NewDirectLightNode<LightDirectional>(lightsCategory);

            yield return factory.NewDirectLightNode<LightPoint>(lightsCategory)
                .AddInput(nameof(LightPoint.Radius), x => x.Radius, (x, v) => x.Radius = v, 1f);

            yield return factory.NewDirectLightNode<LightSpot>(lightsCategory)
                .AddInput(nameof(LightSpot.Range), x => x.Range, (x, v) => x.Range = v, 3f)
                .AddInput(nameof(LightSpot.AngleInner), x => x.AngleInner, (x, v) => x.AngleInner = v, 30f)
                .AddInput(nameof(LightSpot.AngleOuter), x => x.AngleOuter, (x, v) => x.AngleOuter = v, 35f)
                .AddInput(nameof(LightSpot.ProjectiveTexture), x => x.ProjectiveTexture, (x, v) => x.ProjectiveTexture = v)
                .AddInput(nameof(LightSpot.UVScale), x => x.UVScale, (x, v) => x.UVScale = v, Vector2.One)
                .AddInput(nameof(LightSpot.UVOffset), x => x.UVOffset, (x, v) => x.UVOffset = v, Vector2.Zero)
                .AddInput(nameof(LightSpot.MipMapScale), x => x.MipMapScale, (x, v) => x.MipMapScale = v, 0f)
                .AddInput(nameof(LightSpot.AspectRatio), x => x.AspectRatio, (x, v) => x.AspectRatio = v, 1f)
                .AddInput(nameof(LightSpot.TransitionArea), x => x.TransitionArea, (x, v) => x.TransitionArea = v, 0.2f)
                .AddInput(nameof(LightSpot.ProjectionPlaneDistance), x => x.ProjectionPlaneDistance, (x, v) => x.ProjectionPlaneDistance = v, 1f)
                .AddInput(nameof(LightSpot.FlipMode), x => x.FlipMode, (x, v) => x.FlipMode = v);

            yield return factory.NewShadowNode<LightDirectionalShadowMap>(lightsCategory)
                .AddInput(nameof(LightDirectionalShadowMap.CascadeCount), x => x.CascadeCount, (x, v) => x.CascadeCount = v, LightShadowMapCascadeCount.FourCascades)
                .AddInputWithFallback(nameof(LightDirectionalShadowMap.DepthRange), x => x.DepthRange, (x, v, f) =>
                {
                    var s = x.DepthRange;
                    var value = v ?? f;
                    s.IsAutomatic = value.IsAutomatic;
                    s.ManualMinDistance = value.ManualMinDistance;
                    s.ManualMaxDistance = value.ManualMaxDistance;
                    s.IsBlendingCascades = value.IsBlendingCascades;
                })
                .AddInput(nameof(LightDirectionalShadowMap.PartitionMode), x => x.PartitionMode, (x, v) => x.PartitionMode = v)
                .AddInput(nameof(LightDirectionalShadowMap.StabilizationMode), x => x.StabilizationMode, (x, v) => x.StabilizationMode = v, LightShadowMapStabilizationMode.ProjectionSnapping)
                .AddDefaultPins()
                .AddEnabledPin();

            yield return factory.NewShadowNode<LightPointShadowMap>(lightsCategory)
                .AddInput(nameof(LightPointShadowMap.Type), x => x.Type, (x, v) => x.Type = v, LightPointShadowMapType.CubeMap)
                .AddDefaultPins()
                .AddEnabledPin();

            yield return factory.NewShadowNode<LightStandardShadowMap>(lightsCategory)
                .AddDefaultPins()
                .AddEnabledPin();

            yield return factory.Create<LightShadowMapFilterTypePcf>(category: lightsCategory, copyOnWrite: false)
                .AddInput(nameof(LightShadowMapFilterTypePcf.FilterSize), x => x.FilterSize, (x, v) => x.FilterSize = v);

            yield return factory.Create<LightDirectionalShadowMap.DepthRangeParameters>(category: lightsCategory)
                .AddInput(nameof(LightDirectionalShadowMap.DepthRangeParameters.IsAutomatic), x => x.IsAutomatic, (x, v) => x.IsAutomatic = v, true)
                .AddInput(nameof(LightDirectionalShadowMap.DepthRangeParameters.ManualMinDistance), x => x.ManualMinDistance, (x, v) => x.ManualMinDistance = v, LightDirectionalShadowMap.DepthRangeParameters.DefaultMinDistance)
                .AddInput(nameof(LightDirectionalShadowMap.DepthRangeParameters.ManualMaxDistance), x => x.ManualMaxDistance, (x, v) => x.ManualMaxDistance = v, LightDirectionalShadowMap.DepthRangeParameters.DefaultMaxDistance)
                .AddInput(nameof(LightDirectionalShadowMap.DepthRangeParameters.IsBlendingCascades), x => x.IsBlendingCascades, (x, v) => x.IsBlendingCascades = v, true);

            yield return factory.Create<LightDirectionalShadowMap.PartitionManual>(category: lightsCategory)
                .AddInput(nameof(LightDirectionalShadowMap.PartitionManual.SplitDistance0), x => x.SplitDistance0, (x, v) => x.SplitDistance0 = v, 0.05f)
                .AddInput(nameof(LightDirectionalShadowMap.PartitionManual.SplitDistance1), x => x.SplitDistance1, (x, v) => x.SplitDistance1 = v, 0.15f)
                .AddInput(nameof(LightDirectionalShadowMap.PartitionManual.SplitDistance2), x => x.SplitDistance2, (x, v) => x.SplitDistance2 = v, 0.50f)
                .AddInput(nameof(LightDirectionalShadowMap.PartitionManual.SplitDistance3), x => x.SplitDistance3, (x, v) => x.SplitDistance3 = v, 1.00f);

            yield return factory.Create<LightDirectionalShadowMap.PartitionLogarithmic>(category: lightsCategory)
                .AddInput(nameof(LightDirectionalShadowMap.PartitionLogarithmic.PSSMFactor), x => x.PSSMFactor, (x, v) => x.PSSMFactor = v, 0.5f);
        }

        static CustomNodeDesc<TLight> NewEnvironmentLightNode<TLight>(this IVLNodeDescriptionFactory factory, string category)
            where TLight : IEnvironmentLight, new()
        {
            return factory.Create<TLight>(category: category, copyOnWrite: false, fragmented: true);
        }

        static CustomNodeDesc<TLight> NewColorLightNode<TLight>(this IVLNodeDescriptionFactory factory, string category)
            where TLight : IColorLight, new()
        {
            return factory.Create<TLight>(category: category, copyOnWrite: false, fragmented: true)
                .AddInput(nameof(IColorLight.Color), x => new Color4(x.Color.ComputeColor()), (x, v) => ((ColorRgbProvider)x.Color).Value = v.ToColor3(), Color4.White);
        }

        static CustomNodeDesc<TLight> NewDirectLightNode<TLight>(this IVLNodeDescriptionFactory factory, string category)
            where TLight : IColorLight, IDirectLight, new()
        {
            return factory.NewColorLightNode<TLight>(category)
                .AddInput(nameof(IDirectLight.Shadow), x => x.Shadow, setter: (x, v) =>
                {
                    var s = x.Shadow;
                    if (v != null)
                    {
                        s.Enabled = v.Enabled;
                        s.Filter = v.Filter;
                        s.Size = v.Size;
                        s.BiasParameters.DepthBias = v.BiasParameters.DepthBias;
                        s.BiasParameters.NormalOffsetScale = v.BiasParameters.NormalOffsetScale;
                        s.Debug = v.Debug;
                        s.Filter = v.Filter;
                        s.Filter = v.Filter;
                    }
                    else
                    {
                        s.Enabled = false;
                    }
                });
        }

        static CustomNodeDesc<TShadow> NewShadowNode<TShadow>(this IVLNodeDescriptionFactory factory, string category)
            where TShadow : LightShadowMap, new()
        {
            return factory.Create<TShadow>(category: category, copyOnWrite: false, fragmented: true);
        }

        static CustomNodeDesc<TShadow> AddDefaultPins<TShadow>(this CustomNodeDesc<TShadow> node)
            where TShadow : LightShadowMap, new()
        {
            return node
                .AddInput(nameof(LightShadowMap.Filter), x => x.Filter, (x, v) => x.Filter = v)
                // Larger sizes can crash Stride when multiple lights / light shafts are used - one can find bunch of TODOs in ShadowMapRenderer regarding texture atlas limits
                .AddInput(nameof(LightShadowMap.Size), x => x.Size, (x, v) => x.Size = v, LightShadowMapSize.Medium)
                .AddInput(nameof(LightShadowMap.BiasParameters.DepthBias), x => x.BiasParameters.DepthBias, (x, v) => x.BiasParameters.DepthBias = v, 0.01f)
                .AddInput(nameof(LightShadowMap.BiasParameters.NormalOffsetScale), x => x.BiasParameters.NormalOffsetScale, (x, v) => x.BiasParameters.NormalOffsetScale = v, 10f)
                .AddInput(nameof(LightShadowMap.Debug), x => x.Debug, (x, v) => x.Debug = v);
        }

        static CustomNodeDesc<TShadow> AddEnabledPin<TShadow>(this CustomNodeDesc<TShadow> node)
            where TShadow : LightShadowMap, new()
        {
            return node.AddInput(nameof(LightShadowMap.Enabled), x => x.Enabled, (x, v) => x.Enabled = v, true);
        }

        static IVLNodeDescription NewSkyboxNode(this IVLNodeDescriptionFactory factory, string category)
        {
            return factory.Create<SkyboxDescription>(
                name: "Skybox",
                category: category,
                copyOnWrite: false,
                hasStateOutput: false,
                fragmented: true,
                update: (nodeContext, description) =>
                {
                    using (var context = new SkyboxGeneratorContext(nodeContext))
                    {
                        description.GeneratedSkybox = GenerateSkybox(description, context);
                    }
                })
                .AddInput(nameof(SkyboxDescription.CubeMap), x => x.CubeMap, (x, v) => x.CubeMap = v)
                .AddInput(nameof(SkyboxDescription.IsSpecularOnly), x => x.IsSpecularOnly, (x, v) => x.IsSpecularOnly = v)
                .AddInput(nameof(SkyboxDescription.DiffuseSHOrder), x => x.DiffuseSHOrder, (x, v) => x.DiffuseSHOrder = v, SkyboxPreFilteringDiffuseOrder.Order3)
                .AddInput(nameof(SkyboxDescription.SpecularCubeMapSize), x => x.SpecularCubeMapSize, (x, v) => x.SpecularCubeMapSize = v, 256)
                .AddOutput<Skybox>("Output", x => x.GeneratedSkybox);
        }

        class SkyboxDescription
        {
            public SkyboxDescription()
            {
                IsSpecularOnly = false;
                DiffuseSHOrder = SkyboxPreFilteringDiffuseOrder.Order3;
                SpecularCubeMapSize = 256;
            }

            /// <summary>
            /// Gets or sets the type of skybox.
            /// </summary>
            /// <value>The type of skybox.</value>
            /// <userdoc>The texture to use as skybox (eg a cubemap or panoramic texture)</userdoc>
            public Texture CubeMap { get; set; }

            /// <summary>
            /// Gets or set if this skybox affects specular only, if <c>false</c> this skybox will affect ambient lighting
            /// </summary>
            /// <userdoc>
            /// Use the skybox only for specular lighting
            /// </userdoc>
            public bool IsSpecularOnly { get; set; }

            /// <summary>
            /// Gets or sets the diffuse sh order.
            /// </summary>
            /// <value>The diffuse sh order.</value>
            /// <userdoc>The level of detail of the compressed skybox, used for diffuse lighting (dull materials). Order5 is more detailed than Order3.</userdoc>
            public SkyboxPreFilteringDiffuseOrder DiffuseSHOrder { get; set; }

            /// <summary>
            /// Gets or sets the specular cubemap size
            /// </summary>
            /// <value>The specular cubemap size.</value>
            /// <userdoc>The cubemap size used for specular lighting. Larger cubemap have more detail.</userdoc>
            public int SpecularCubeMapSize { get; set; }

            internal Skybox GeneratedSkybox;
        }

        class SkyboxGeneratorContext : ShaderGeneratorContext
        {
            readonly IResourceHandle<Game> FGameHandle;

            public SkyboxGeneratorContext(NodeContext nodeContext)
            {
                FGameHandle = nodeContext.GetGameHandle();
                var game = FGameHandle.Resource;

                Content = game.Content;
                RenderContext = RenderContext.GetShared(Services);
                RenderDrawContext = new RenderDrawContext(Services, RenderContext, game.GraphicsContext);
            }

            public IServiceRegistry Services => FGameHandle.Resource.Services;

            public EffectSystem EffectSystem => FGameHandle.Resource.EffectSystem;

            public GraphicsDevice GraphicsDevice => FGameHandle.Resource.GraphicsDevice;

            public IGraphicsDeviceService GraphicsDeviceService => FGameHandle.Resource.GraphicsDeviceManager;

            public RenderContext RenderContext { get; }

            public RenderDrawContext RenderDrawContext { get; }

            protected override void Destroy()
            {
                FGameHandle.Dispose();

                base.Destroy();
            }
        }

        static Skybox GenerateSkybox(SkyboxDescription description, SkyboxGeneratorContext context)
        {
            if (description == null) throw new ArgumentNullException("description");
            if (context == null) throw new ArgumentNullException("context");

            var parameters = new ParameterCollection();
            var skybox = new Skybox();
            skybox.Parameters = parameters;

            var cubemap = description.CubeMap;
            if (cubemap == null)
            {
                return skybox;
            }

            // load the skybox texture from the asset.
            var skyboxTexture = cubemap;
            if (skyboxTexture.ViewDimension == TextureDimension.Texture2D)
            {
                var cubemapSize = (int)Math.Pow(2, Math.Ceiling(Math.Log(skyboxTexture.Width / 4) / Math.Log(2))); // maximum resolution is around horizontal middle line which composes 4 images.
                skyboxTexture = CubemapFromTextureRenderer.GenerateCubemap(context.Services, context.RenderDrawContext, skyboxTexture, cubemapSize);
            }
            else if (skyboxTexture.ViewDimension != TextureDimension.TextureCube)
            {
                throw new ArgumentException($"SkyboxGenerator: The texture type ({skyboxTexture.ViewDimension}) used as skybox is not supported. Should be a Cubemap or a 2D texture.");
            }

            // If we are using the skybox asset for lighting, we can compute it
            // Specular lighting only?
            if (!description.IsSpecularOnly)
            {
                // -------------------------------------------------------------------
                // Calculate Diffuse prefiltering
                // -------------------------------------------------------------------
                var lamberFiltering = new LambertianPrefilteringSHNoCompute(context.RenderContext)
                {
                    HarmonicOrder = (int)description.DiffuseSHOrder,
                    RadianceMap = skyboxTexture
                };
                lamberFiltering.Draw(context.RenderDrawContext);

                var coefficients = lamberFiltering.PrefilteredLambertianSH.Coefficients;
                for (int i = 0; i < coefficients.Length; i++)
                {
                    coefficients[i] = coefficients[i] * SphericalHarmonics.BaseCoefficients[i];
                }

                skybox.DiffuseLightingParameters.Set(SkyboxKeys.Shader, new ShaderClassSource("SphericalHarmonicsEnvironmentColor", lamberFiltering.HarmonicOrder));
                skybox.DiffuseLightingParameters.Set(SphericalHarmonicsEnvironmentColorKeys.SphericalColors, coefficients);
            }

            // -------------------------------------------------------------------
            // Calculate Specular prefiltering
            // -------------------------------------------------------------------
            var specularRadiancePrefilterGGX = new RadiancePrefilteringGGXNoCompute(context.RenderContext);

            var textureSize = description.SpecularCubeMapSize <= 0 ? 64 : description.SpecularCubeMapSize;
            textureSize = (int)Math.Pow(2, Math.Round(Math.Log(textureSize, 2)));
            if (textureSize < 64) textureSize = 64;

            // TODO: Add support for HDR 32bits 
            var filteringTextureFormat = skyboxTexture.Format.IsHDR() ? skyboxTexture.Format : PixelFormat.R8G8B8A8_UNorm;

            //var outputTexture = Texture.New2D(graphicsDevice, 256, 256, skyboxTexture.Format, TextureFlags.ShaderResource | TextureFlags.UnorderedAccess, 6);
            using (var outputTexture = Texture.New2D(context.GraphicsDevice, textureSize, textureSize, true, filteringTextureFormat, TextureFlags.ShaderResource | TextureFlags.RenderTarget, 6))
            {
                specularRadiancePrefilterGGX.RadianceMap = skyboxTexture;
                specularRadiancePrefilterGGX.PrefilteredRadiance = outputTexture;
                specularRadiancePrefilterGGX.Draw(context.RenderDrawContext);

                var cubeTexture = Texture.NewCube(context.GraphicsDevice, textureSize, true, filteringTextureFormat);
                context.RenderDrawContext.CommandList.Copy(outputTexture, cubeTexture);

                skybox.SpecularLightingParameters.Set(SkyboxKeys.Shader, new ShaderClassSource("RoughnessCubeMapEnvironmentColor"));
                skybox.SpecularLightingParameters.Set(SkyboxKeys.CubeMap, cubeTexture);
            }
            // TODO: cubeTexture is not deallocated

            return skybox;
        }
    }
}
