// Copyright (c) Stride contributors (https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;

namespace VL.Stride.Rendering
{
    /// <summary>
    /// Custom render feature, that uploads constants needed by the VLEffectMain effect
    /// </summary>
    public class VLEffectRenderFeature : SubRenderFeature
    {
        private StaticObjectPropertyKey<RenderEffect> renderEffectKey;

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            base.InitializeCore();

            renderEffectKey = ((RootEffectRenderFeature)RootRenderFeature).RenderEffectKey;
        }

        /// <inheritdoc/>
        public override void PrepareEffectPermutations(RenderDrawContext context)
        {
            var renderEffects = RootRenderFeature.RenderData.GetData(renderEffectKey);
            int effectSlotCount = ((RootEffectRenderFeature)RootRenderFeature).EffectPermutationSlotCount;

            foreach (var renderObject in RootRenderFeature.RenderObjects)
            {
                var staticObjectNode = renderObject.StaticObjectNode;
                var renderMesh = (RenderMesh)renderObject;

                for (int i = 0; i < effectSlotCount; ++i)
                {
                    var staticEffectObjectNode = staticObjectNode * effectSlotCount + i;
                    var renderEffect = renderEffects[staticEffectObjectNode];

                    // Skip effects not used during this frame
                    if (renderEffect == null || !renderEffect.IsUsedDuringThisFrame(RenderSystem))
                        continue;

                    // Generate shader permuatations
                    var enableByname = renderMesh.Mesh.Parameters.Get(VLEffectParameters.EnableExtensionName);
                    renderEffect.EffectValidator.ValidateParameter(VLEffectParameters.EnableExtensionName, enableByname);
                    if (enableByname)
                        renderEffect.EffectValidator.ValidateParameter(VLEffectParameters.MaterialExtensionName, renderMesh.Mesh.Parameters.Get(VLEffectParameters.MaterialExtensionName));

                    var enableBySource = renderMesh.Mesh.Parameters.Get(VLEffectParameters.EnableExtensionShader);
                    renderEffect.EffectValidator.ValidateParameter(VLEffectParameters.EnableExtensionShader, enableBySource);
                    if (enableBySource)
                        renderEffect.EffectValidator.ValidateParameter(VLEffectParameters.MaterialExtensionShader, renderMesh.Mesh.Parameters.Get(VLEffectParameters.MaterialExtensionShader));
                }
            }
        }
    }
}
