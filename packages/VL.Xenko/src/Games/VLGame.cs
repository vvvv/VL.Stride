using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VL.Xenko.Rendering;
using Xenko.Engine;
using Xenko.Games;
using Xenko.Rendering;

namespace VL.Xenko.Games
{
    public class VLGame : Game
    {
        public VLGame()
            : base()
        {
        }

        public void AddLayerRenderFeature()
        {
            var renderStages = SceneSystem.GraphicsCompositor.RenderStages;
            var opaqueStage = renderStages.FirstOrDefault(s => s.Name == "Opaque") ?? renderStages.FirstOrDefault();

            if (opaqueStage != null)
            {
                var stageSelector = new SimpleGroupToRenderStageSelector()
                {
                    RenderStage = opaqueStage
                };

                var layerRenderer = new LayerRenderFeature();

                layerRenderer.RenderStageSelectors.Add(stageSelector);
                SceneSystem.GraphicsCompositor.RenderFeatures.Add(layerRenderer); 
            }
        }
    }
}
