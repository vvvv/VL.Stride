using Xenko.Engine;

namespace VL.Xenko
{
    public static class GameExtensions
    {
        /// <summary>
        /// Attaches VL to the game once the game window is created, call this right before game.Run()
        /// </summary>
        /// <param name="game">The game.</param>
        public static void AttachVL(this Game game)
        {
            game.WindowCreated += Game_WindowCreated;
            VLScript.GameInstance = game;
        }

        private static void Game_WindowCreated(object sender, System.EventArgs e)
        {
            var game = (Game)sender;
            game.Window.AllowUserResizing = true;

            var timer = new System.Timers.Timer(3000);
            timer.AutoReset = false;
            timer.Elapsed += (s, a) => {
                //attach VL script
                var script = new VLScript();
                game.Script.Add(script);
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }
    }
}
