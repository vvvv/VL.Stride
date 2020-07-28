using Stride.Core;
using Stride.Engine;
using Stride.Rendering;
using Stride.Rendering.Compositing;
using System.Linq;
using System.Reflection;
using VL.Stride.Input;

namespace VL.Stride.Engine
{
    /// <summary>
    /// Renders a scene instance with a graphics compositor.
    /// </summary>
    public class SceneInstanceRenderer : IGraphicsRendererBase
    {
        /// <summary>
        /// Gets or sets the scene instance.
        /// </summary>
        public SceneInstance SceneInstance { get; set; }

        /// <summary>
        /// Gets or sets the graphics compositor.
        /// </summary>
        public GraphicsCompositor GraphicsCompositor { get; set; }

        public void Draw(RenderDrawContext renderDrawContext)
        {
            var renderContext = renderDrawContext.RenderContext;
            if (SceneInstance?.RootScene != null)
            {
                renderContext.GetWindowInputSource(out var inputSouce);
                SceneInstance.RootScene.SetWindowInputSource(inputSouce);
            }

            // Reset the context
            renderContext.Reset();

            // ???
            // renderContext.Allocator.Recycle(link => true);

            // Execute Draw step of SceneInstance
            // This will run entity processors
            SceneInstance?.Draw(renderContext);

            renderDrawContext.ResourceGroupAllocator.Flush();
            renderDrawContext.QueryManager.Flush();

            // Push context (pop after using)
            using (renderDrawContext.RenderContext.PushTagAndRestore(SceneInstance.Current, SceneInstance))
            {
                GraphicsCompositor?.Draw(renderDrawContext);
            }
        }
    }

    // TODO: Get rid of me by making the SceneInstance.Draw method public
    static class SceneInstanceExt
    {
        static MethodInfo drawMethod = typeof(SceneInstance).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(m => m.Name == "Draw");

        public static void Draw(this SceneInstance sceneInstance, RenderContext renderContext)
        {
            if (sceneInstance != null)
                drawMethod.Invoke(sceneInstance, new[] { renderContext });
        }
    }
}
