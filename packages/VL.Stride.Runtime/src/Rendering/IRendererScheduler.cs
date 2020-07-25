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
    }
}
