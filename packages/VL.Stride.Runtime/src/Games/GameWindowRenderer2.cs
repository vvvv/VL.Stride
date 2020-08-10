using Stride.Core;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VL.Stride.Games
{
    // Only needed for SkiaRenderer to get notified about back buffers to be resized.
    public class GameWindowRenderer2 : GameWindowRenderer
    {
        public static readonly PropertyKey<GameWindowRenderer2> Current = new PropertyKey<GameWindowRenderer2>("CurrentGameWindowRenderer2", typeof(GameWindowRenderer2));

        private ComponentBaseExtensions.PropertyTagRestore<GameWindowRenderer2> tagRestore;
        private bool beginDrawOk;
        private FieldInfo isBackBufferToResize, windowUserResized;

        public GameWindowRenderer2(IServiceRegistry registry, GameContext gameContext) : base(registry, gameContext)
        {
            isBackBufferToResize = typeof(GameWindowRenderer).GetField(nameof(isBackBufferToResize), BindingFlags.NonPublic | BindingFlags.Instance);
            windowUserResized = typeof(GameWindowRenderer).GetField(nameof(windowUserResized), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public event EventHandler Resizing;

        public override bool BeginDraw()
        {
            if ((bool)isBackBufferToResize.GetValue(this) || (bool)windowUserResized.GetValue(this))
                Resizing?.Invoke(this, EventArgs.Empty);

            if (beginDrawOk = base.BeginDraw())
            {
                var context = RenderContext.GetShared(Services);
                tagRestore = context.PushTagAndRestore(Current, this);
            }

            return beginDrawOk;
        }

        public override void EndDraw()
        {
            if (beginDrawOk)
            {
                tagRestore.Dispose();
            }

            base.EndDraw();
        }
    }
}
