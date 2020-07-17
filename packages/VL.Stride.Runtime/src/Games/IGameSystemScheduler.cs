using Stride.Games;

namespace VL.Stride.Games
{
    /// <summary>
    /// Allows to schedule game systems in a frame. Their Update and Draw methods will be called in the order as they have been scheduled.
    /// </summary>
    public interface IGameSystemScheduler
    {
        /// <summary>
        /// Schedule a game system to be processed in this frame.
        /// </summary>
        /// <param name="gameSystem">The game system to schedule.</param>
        void Schedule(GameSystemBase gameSystem);
    }
}
