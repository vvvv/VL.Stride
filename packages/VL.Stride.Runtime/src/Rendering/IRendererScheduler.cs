using Stride.Rendering;

namespace VL.Stride.Rendering
{
    /// <summary>
    /// Allows to schedule graphics renderers for drawing.
    /// </summary>
    public interface IRendererScheduler
    {
        /// <summary>
        /// Schedules a graphics renderer to be drawn.
        /// </summary>
        /// <param name="graphicsRenderer">The graphics renderer to schedule.</param>
        void Schedule(IGraphicsRendererBase graphicsRenderer);

        /// <summary>
        /// Removes a graphics renderer before it is drawn.
        /// </summary>
        /// <param name="graphicsRenderer">The graphics renderer to remove.</param>
        void Remove(IGraphicsRendererBase graphicsRenderer);
    }
}
