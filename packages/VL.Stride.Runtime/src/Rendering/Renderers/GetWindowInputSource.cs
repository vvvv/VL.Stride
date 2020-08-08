using Stride.Input;
using Stride.Rendering;
using System;
using System.Diagnostics;
using VL.Stride.Input;

namespace VL.Stride.Rendering
{
    public class GetWindowInputSource : IGraphicsRendererBase
    {
        public IGraphicsRendererBase Input { get; set; }
        public IInputSource InputSource { get; private set; }

        public void Draw(RenderDrawContext context)
        {
            try
            {
                var renderContext = context.RenderContext;
                renderContext.GetWindowInputSource(out var inputSource);
                InputSource = inputSource;

                Input?.Draw(context);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
