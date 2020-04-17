using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Rendering;
using Xenko.Rendering.Images;

namespace VL.Xenko.Rendering
{
    public class TextureFXEffect : ImageEffectShader
    {
        private TimeSpan lastExceptionTime;
        private TimeSpan retryTime = TimeSpan.FromSeconds(3);

        public TextureFXEffect(string effectName = null, bool delaySetRenderTargets = false)
            : base(effectName, delaySetRenderTargets)
        { }

        protected override void DrawCore(RenderDrawContext context)
        {
            if ((context.RenderContext.Time.Total - lastExceptionTime) < retryTime)
                return;

            try
            {
                base.DrawCore(context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                lastExceptionTime = context.RenderContext.Time.Total;
            }
        }
    }
}
