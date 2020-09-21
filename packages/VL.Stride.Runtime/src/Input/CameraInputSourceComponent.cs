using Stride.Animations;
using Stride.Core.Annotations;
using Stride.Engine;
using Stride.Engine.Design;
using Stride.Input;
using Stride.Rendering;
using Stride.Rendering.Compositing;
using VL.Stride.Shaders.ShaderFX;

namespace VL.Stride.Input
{
    [DefaultEntityComponentProcessor(typeof(InputSourceProcessor), ExecutionMode = ExecutionMode.All)]
    public class CameraInputSourceComponent : ActivableEntityComponent
    {
        public IInputSource InputSource { get; set; }
    }

    public class CameraInputSourceSceneRenderer : SceneRendererBase
    {
        CameraInputSourceComponent FCameraInputSourceComponent;
        public CameraInputSourceComponent CameraInputSourceComponent
        {
            get => FCameraInputSourceComponent;
            set
            {
                if (value != FCameraInputSourceComponent)
                {
                    if (FCameraInputSourceComponent != null)
                        FCameraInputSourceComponent.InputSource = null;
                    FCameraInputSourceComponent = value;
                }
            }
        }

        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            var inputSource = context.GetWindowInputSource();

            if (inputSource != null && CameraInputSourceComponent != null)
            {
                if (CameraInputSourceComponent.Enabled)
                    CameraInputSourceComponent.InputSource = inputSource;
                else
                    CameraInputSourceComponent.InputSource = null;
            }
        }
    }
}
