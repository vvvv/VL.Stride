using Stride.Rendering;

namespace VL.Stride.Rendering
{
    /// <summary>
    /// Custom render feature, that manages the VLEffectMain effect
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
                    var enableByName = renderMesh.Mesh.Parameters.Get(VLEffectParameters.EnableExtensionName);
                    renderEffect.EffectValidator.ValidateParameter(VLEffectParameters.EnableExtensionName, enableByName);
                    if (enableByName)
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
