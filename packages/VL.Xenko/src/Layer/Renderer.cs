using System;
using VL.Xenko.Rendering;
using Xenko.Engine;

namespace VL.Xenko.Layer
{
    /// <summary>
    /// Renders a layer from within the view of the main camera.
    /// </summary>
    public class Renderer : IDisposable
    {
        readonly Game game;
        readonly Entity entity;
        readonly LayerComponent layerComponent;

        public Renderer(Game game)
        {
            this.game = game;
            entity = new Entity("VL Renderer");
            layerComponent = new LayerComponent();
            entity.Add(layerComponent);
            game.SceneSystem.SceneInstance.RootScene.Entities.Add(entity);
        }

        public void Update(ILowLevelAPIRender layer, bool enabled = true)
        {
            if (enabled)
                layerComponent.Layer = layer;
            else
                layerComponent.Layer = null;
        }

        public void Dispose()
        {
            game.SceneSystem.SceneInstance.RootScene.Entities.Remove(entity);
            entity.Dispose();
        }
    }
}
