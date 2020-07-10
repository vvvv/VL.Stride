using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.ComputeEffect.GGXPrefiltering;
using Stride.Rendering.ComputeEffect.LambertianPrefiltering;
using Stride.Rendering.Materials;
using Stride.Rendering.Skyboxes;
using Stride.Shaders;
using System;
using VL.Core;
using VL.Lib.Basics.Resources;

namespace VL.Stride.Rendering.Lights
{
    // Mostly taken from Strides SkyboxGenerator but stripped of any asset related stuff
    static partial class LightNodes
    {
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