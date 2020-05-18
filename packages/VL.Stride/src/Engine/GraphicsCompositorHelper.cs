using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Particles.Rendering;
using Stride.Rendering;
using Stride.Rendering.Background;
using Stride.Rendering.Compositing;
using Stride.Rendering.Images;
using Stride.Rendering.Lights;
using Stride.Rendering.Materials;
using Stride.Rendering.Shadows;
using Stride.Rendering.Sprites;
using Stride.Rendering.UI;
using VL.Stride.Rendering;

namespace OffscreenRenderer
{
    public static class Helpers
    {

        /// <summary>
        /// Creates a graphics compositor programatically that renders into a Rendertarget. It can render everything the default Compositor can 
        /// </summary>
        public static GraphicsCompositor CreateCompositor(
            Texture renderTarget,
            out ForwardRenderer forwardRenderer,
            RenderGroupMask groupMask = RenderGroupMask.All,
            CameraComponent cameraComponent = null,
            IPostProcessingEffects postProcessingEffects = null,
            MultisampleCount msaaLevel = MultisampleCount.X4)
        {
            #region Render stages
            var opaqueRenderStage = new RenderStage("Opaque", "Main") { SortMode = new StateChangeSortMode() };
            var transparentRenderStage = new RenderStage("Transparent", "Main") { SortMode = new BackToFrontSortMode() };
            var shadowMapCaster = new RenderStage("ShadowMapCaster", "ShadowMapCaster") { SortMode = new FrontToBackSortMode() };
            var shadowMapCasterrParaboloidRenderStage = new RenderStage("ShadowMapCasterParaboloid", "ShadowMapCasterParaboloid") { SortMode = new FrontToBackSortMode() };
            var shadowMapCasterCubeMapRenderStage = new RenderStage("ShadowMapCasterCubeMap", "ShadowMapCasterCubeMap") { SortMode = new FrontToBackSortMode() };
            var gBuffer = new RenderStage("GBuffer", "GBuffer") { SortMode = new FrontToBackSortMode() };
            #endregion

            #region RenderFeatures
            var meshRenderFeature = new MeshRenderFeature
            {
                PipelineProcessors =
                    {
                        new MeshPipelineProcessor() { TransparentRenderStage = transparentRenderStage },
                        new ShadowMeshPipelineProcessor() { DepthClipping = false, ShadowMapRenderStage = shadowMapCaster},
                        new ShadowMeshPipelineProcessor() { DepthClipping = true, ShadowMapRenderStage = shadowMapCasterrParaboloidRenderStage },
                        new ShadowMeshPipelineProcessor() { DepthClipping = true, ShadowMapRenderStage = shadowMapCasterCubeMapRenderStage }
                    },
                RenderFeatures =
                    {
                        new TransformRenderFeature(),
                        new SkinningRenderFeature(),
                        new MaterialRenderFeature(),
                        new ShadowCasterRenderFeature(),
                        new ForwardLightingRenderFeature()
                        {
                            LightRenderers =
                            {
                                new LightAmbientRenderer(),
                                new LightDirectionalGroupRenderer(),
                                new LightSkyboxRenderer(),
                                new LightClusteredPointSpotGroupRenderer(),
                                new LightPointGroupRenderer()
                            }
                        }
                    },
                RenderStageSelectors =
                    {
                        new MeshTransparentRenderStageSelector()
                        {
                            EffectName = "StrideForwardShadingEffect",
                            OpaqueRenderStage = opaqueRenderStage,
                            TransparentRenderStage = transparentRenderStage,
                            RenderGroup = groupMask
                        },
                        new ShadowMapRenderStageSelector()
                        {
                            EffectName = "StrideForwardShadingEffect.ShadowMapCaster",
                            ShadowMapRenderStage = shadowMapCaster,
                            RenderGroup = groupMask
                        },
                        new ShadowMapRenderStageSelector()
                        {
                            EffectName = "StrideForwardShadingEffect.ShadowMapCasterParaboloid",
                            ShadowMapRenderStage = shadowMapCasterrParaboloidRenderStage,
                            RenderGroup = groupMask
                        },
                        new ShadowMapRenderStageSelector()
                        {
                            EffectName = "StrideForwardShadingEffect.ShadowMapCasterCubeMap",
                            ShadowMapRenderStage = shadowMapCasterCubeMapRenderStage,
                            RenderGroup = groupMask
                        },
                        new MeshTransparentRenderStageSelector()
                        {
                            EffectName = "StrideForwardShadingEffect.ShadowMapCaster",
                            OpaqueRenderStage = gBuffer,
                            RenderGroup = groupMask
                        }
                    }

            };

            var spriteRenderFeature = new SpriteRenderFeature()
            {
                RenderStageSelectors =
                {
                    new SpriteTransparentRenderStageSelector()
                    {
                        EffectName = "Test", // TODO: Check this
                        OpaqueRenderStage = opaqueRenderStage,
                        TransparentRenderStage = transparentRenderStage,
                        RenderGroup = groupMask
                    }
                }
            };

            var backgroundRenderFeature = new BackgroundRenderFeature()
            {
                RenderStageSelectors =
                {
                    new SimpleGroupToRenderStageSelector()
                    {
                        EffectName = "Test",
                        RenderStage = opaqueRenderStage,
                        RenderGroup = groupMask
                    }
                }
            };

            var uiRenderFeature = new UIRenderFeature()
            {
                RenderStageSelectors =
                {
                    new SimpleGroupToRenderStageSelector()
                    {
                        EffectName = "Test",
                        RenderStage = transparentRenderStage,
                        RenderGroup = groupMask
                    }
                }
            };

            var particleEmitterRenderFeature = new ParticleEmitterRenderFeature()
            {
                RenderStageSelectors =
                {
                    new ParticleEmitterTransparentRenderStageSelector()
                    {
                        OpaqueRenderStage = opaqueRenderStage,
                        TransparentRenderStage = transparentRenderStage,
                        RenderGroup = groupMask
                    }
                }
            };

            var vlLayerRenderfeature = new LayerRenderFeature()
            {
                RenderStageSelectors =
                {
                    new SimpleGroupToRenderStageSelector()
                    {
                        RenderStage = opaqueRenderStage,
                        RenderGroup = groupMask
                    }
                }
            };

            #endregion

            forwardRenderer = new ForwardRenderer
            {
                GBufferRenderStage = gBuffer,
                MSAALevel = msaaLevel,
                OpaqueRenderStage = opaqueRenderStage,
                ShadowMapRenderStages = { shadowMapCaster },
                //SubsurfaceScatteringBlurEffect,
                TransparentRenderStage = transparentRenderStage,
                PostEffects = postProcessingEffects
            };

            #region Game
            var game = new SceneExternalCameraRenderer()
            {
                ExternalCamera = cameraComponent,
                Child = new RenderTextureSceneRenderer()
                {
                    RenderTexture = renderTarget,
                    Child = forwardRenderer,
                }
            };
            #endregion

            return new GraphicsCompositor
            {
                Cameras = { new SceneCameraSlot()},

                RenderStages =
                {
                    opaqueRenderStage,
                    transparentRenderStage,
                    shadowMapCaster,
                    shadowMapCasterrParaboloidRenderStage,
                    shadowMapCasterCubeMapRenderStage,
                    gBuffer
                },
                RenderFeatures =
                {
                    meshRenderFeature,
                    spriteRenderFeature,
                    backgroundRenderFeature,
                    uiRenderFeature,
                    particleEmitterRenderFeature,
                    vlLayerRenderfeature
                },

                Game = game
            };

        }
    }
}