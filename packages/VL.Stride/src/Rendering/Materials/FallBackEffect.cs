using Stride.Core.Annotations;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Shaders;
using Stride.Shaders.Compiler;
using System;
using System.Collections.Generic;
using System.Text;

namespace VL.Stride.Rendering.Materials
{
    public class FallBackEffect
    {
        EffectSystem effectSystem;
        Material fallbackTextureMaterial;
        Material fallbackColorMaterial;

        protected Effect ComputeMeshFallbackEffect(RenderObject renderObject, [NotNull] RenderEffect renderEffect, RenderEffectState renderEffectState)
        {
            try
            {
                var renderMesh = (RenderMesh)renderObject;

                bool hasDiffuseMap = renderMesh.MaterialPass.Parameters.ContainsKey(MaterialKeys.DiffuseMap);
                var fallbackMaterial = hasDiffuseMap
                    ? fallbackTextureMaterial
                    : fallbackColorMaterial;

                // High priority
                var compilerParameters = new CompilerParameters { EffectParameters = { TaskPriority = -1 } };

                // Support skinning
                if (renderMesh.Mesh.Skinning != null && renderMesh.Mesh.Skinning.Bones.Length <= 56)
                {
                    compilerParameters.Set(MaterialKeys.HasSkinningPosition, renderMesh.Mesh.Parameters.Get(MaterialKeys.HasSkinningPosition));
                    compilerParameters.Set(MaterialKeys.HasSkinningNormal, renderMesh.Mesh.Parameters.Get(MaterialKeys.HasSkinningNormal));
                    compilerParameters.Set(MaterialKeys.HasSkinningTangent, renderMesh.Mesh.Parameters.Get(MaterialKeys.HasSkinningTangent));

                    compilerParameters.Set(MaterialKeys.SkinningMaxBones, 56);
                }

                // Set material permutations
                compilerParameters.Set(MaterialKeys.PixelStageSurfaceShaders, fallbackMaterial.Passes[0].Parameters.Get(MaterialKeys.PixelStageSurfaceShaders));
                compilerParameters.Set(MaterialKeys.PixelStageStreamInitializer, fallbackMaterial.Passes[0].Parameters.Get(MaterialKeys.PixelStageStreamInitializer));

                // Set lighting permutations (use custom white light, since this effect will not be processed by the lighting render feature)
                compilerParameters.Set(LightingKeys.EnvironmentLights, new ShaderSourceCollection { new ShaderClassSource("LightConstantWhite") });

                // Initialize parameters with material ones (need a CopyTo?)
                renderEffect.FallbackParameters = new ParameterCollection(renderMesh.MaterialPass.Parameters);

                // Don't show selection wireframe/highlights as compiling
                var ignoreState = renderEffect.EffectSelector.EffectName.EndsWith(".Wireframe") || renderEffect.EffectSelector.EffectName.EndsWith(".Highlight") ||
                                  renderEffect.EffectSelector.EffectName.EndsWith(".Picking");

                //// Also set a value so that we know something is loading (green glowing FX) or error (red glowing FX)
                //if (!ignoreState)
                //{
                //    if (renderEffectState == RenderEffectState.Compiling)
                //        compilerParameters.Set(SceneEditorParameters.IsEffectCompiling, true);
                //    else if (renderEffectState == RenderEffectState.Error)
                //        compilerParameters.Set(SceneEditorParameters.IsEffectError, true);
                //}

                if (renderEffectState == RenderEffectState.Error)
                {
                    // Retry every few seconds
                    renderEffect.RetryTime = DateTime.UtcNow + TimeSpan.FromSeconds(5);
                }

                return effectSystem.LoadEffect(renderEffect.EffectSelector.EffectName, compilerParameters).WaitForResult();
            }
            catch
            {
                // TODO: Log or rethrow?
                renderEffect.State = RenderEffectState.Error;
                return null;
            }
        }
    }
}
