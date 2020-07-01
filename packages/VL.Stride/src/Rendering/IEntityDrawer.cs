using Stride.Engine;
using Stride.Rendering;

namespace VL.Stride.Rendering
{
    /// <summary>
    /// Interface to implement by patches that want to render using the LowLevelAPI
    /// </summary>
    public interface IEntityDrawer
    {
        /// <summary>
        /// Called by the drawer component processor to get the renderer
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns></returns>
        IGraphicsRendererBase Process(Entity entity);
    }
}
