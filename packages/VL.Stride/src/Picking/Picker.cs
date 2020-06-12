using OpenTK;
using Stride.Engine;
using Stride.Rendering;
using Stride.Rendering.Compositing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VL.Stride.Picking
{
    public class Picker
    {
        PickingSceneRenderer entityPicker;

        public Picker(GraphicsCompositor graphicsCompositor, SceneInstance sceneInstance)
        {
            var pickingRenderStage = new RenderStage("Picking", "Picking");
            graphicsCompositor.RenderStages.Add(pickingRenderStage);

            //pickingRenderStage.Filter = new PickingFilter(this);

            // Meshes
            var meshRenderFeature = graphicsCompositor.RenderFeatures.OfType<MeshRenderFeature>().FirstOrDefault();
            // TODO: Complain (log) if there is no MeshRenderFeature
            if (meshRenderFeature != null)
            {
                meshRenderFeature.RenderFeatures.Add(new PickingRenderFeature());
                meshRenderFeature.RenderStageSelectors.Add(new SimpleGroupToRenderStageSelector
                {
                    EffectName = "VLForwardShadingEffect" + ".Picking",
                    RenderStage = pickingRenderStage,
                    RenderGroup = RenderGroupMask.All
                });
            }

            //// Sprites
            //var spriteRenderFeature = graphicsCompositor.RenderFeatures.OfType<SpriteRenderFeature>().FirstOrDefault();
            //// TODO: Complain (log) if there is no SpriteRenderFeature
            //if (spriteRenderFeature != null)
            //{
            //    spriteRenderFeature.RenderStageSelectors.Add(new SimpleGroupToRenderStageSelector
            //    {
            //        EffectName = "Test",
            //        RenderStage = pickingRenderStage,
            //        RenderGroup = RenderGroupMask.All
            //    });
            //}

            // TODO: SpriteStudio (not here but as a plugin)
            var cameraRenderer = graphicsCompositor.Game as SceneExternalCameraRenderer;
            cameraRenderer.Child = new SceneRendererCollection()
            {
                cameraRenderer.Child,
                (entityPicker = new PickingSceneRenderer { PickingRenderStage = pickingRenderStage })
            };

            //var editorCompositor = (EditorTopLevelCompositor)graphicsCompositor.Game;
            //editorCompositor.PostGizmoCompositors.Add(entityPicker = new PickingSceneRenderer { PickingRenderStage = pickingRenderStage });

            //var contentScene = ((EntityHierarchyEditorGame)game).ContentScene;
            //if (contentScene != null) entityPicker.CacheScene(contentScene, true);
            entityPicker.CacheScene(sceneInstance.RootScene, true);
        }

        public EntityPickingResult Update()
        {
            return entityPicker.Pick();
        }
    }
}
