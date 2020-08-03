using Stride.Core;
using Stride.Engine;
using Stride.Input;
using VL.Stride.Input;

namespace VL.Stride.Engine
{
    public static class EngineExtensions
    {
        public static bool GetNearestWindowInputSource(this Entity entity, out IInputSource inputSource)
        {
            inputSource = null;
            var scene = entity?.Scene;
            
            while (scene != null && inputSource is null)
            {
                scene.GetWindowInputSource(out inputSource);
                scene = scene.Parent;
            }

            return inputSource != null;
        }

    }
}
