using System;
using Stride.Rendering;
using Stride.Rendering.Images;

namespace VL.Stride.Rendering
{
    public class TextureFXEffect : ImageEffectShader
    {
        private TimeSpan? lastExceptionTime;
        private TimeSpan retryTime = TimeSpan.FromSeconds(3);

        public TextureFXEffect(string effectName = null, bool delaySetRenderTargets = false)
            : base(effectName, delaySetRenderTargets)
        { 
        }

        public bool IsOutputAssigned => OutputCount > 0 && GetOutput(0) != null;

        protected override void PreDrawCore(RenderDrawContext context)
        {
            if (IsOutputAssigned)
                base.PreDrawCore(context);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var time = context.RenderContext.Time;
            if (time != null && lastExceptionTime.HasValue && (time.Total - lastExceptionTime) < retryTime)
                return;

            if (IsOutputAssigned)
            {
                try
                {
                    base.DrawCore(context);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    if (time != null)
                        lastExceptionTime = time.Total;
                }
            }
        }

        protected override void PostDrawCore(RenderDrawContext context)
        {
            if (IsOutputAssigned)
                base.PostDrawCore(context);
        }
    }
}
