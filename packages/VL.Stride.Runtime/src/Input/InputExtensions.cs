using Stride.Core;
using Stride.Engine;
using Stride.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Stride.Input
{
    public static class InputExtensions
    {
        /// <summary>
        /// A property key to get the window input source from the <see cref="ComponentBase.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<IInputSource> WindowInputSource = new PropertyKey<IInputSource>("WindowInputSource", typeof(IInputSource));

        public static T SetWindowInputSource<T>(this T input, IInputSource inputSource) where T : ComponentBase
        {
            input.Tags.Set(WindowInputSource, inputSource);
            return input;
        }

        public static T GetWindowInputSource<T>(this T input, out IInputSource inputSource) where T : ComponentBase
        {
            inputSource = input.Tags.Get(WindowInputSource);
            return input;
        }

        public static IInputSource GetDevices(this IInputSource inputSource, out IMouseDevice mouseDevice, out IKeyboardDevice keyboardDevice, out IPointerDevice pointerDevice)
        {
            mouseDevice = null;
            keyboardDevice = null;
            pointerDevice = null;

            if (inputSource != null)
            {
                foreach (var item in inputSource.Devices)
                {
                    var device = item.Value;

                    if (device is IMouseDevice mouse)
                        mouseDevice = mouse;

                    else if (device is IKeyboardDevice keyboard)
                        keyboardDevice = keyboard;

                    else if (device is IPointerDevice pointer)
                        pointerDevice = pointer;
                } 
            }

            return inputSource;
        }



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
