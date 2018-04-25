using System;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace VL.Xenko
{
    public class VLHDE : SyncScript
    {
        public static Game GameInstance;
        public static new GraphicsDevice GraphicsDevice => GameInstance.GraphicsDevice;

        private readonly Action FStart;
        private readonly Action FUpdate;

        // Declared public member fields and properties will show in the game studio
        public VLHDE(Action start, Action update)
        {
            FStart = start;
            FUpdate = update;
        }

        public override void Start()
        {
            FStart();
        }

        public override void Update()
        {
            FUpdate();
        }
    }
}
