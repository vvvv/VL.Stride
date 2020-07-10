using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Stride.Utils
{
    public static class CameraUtils
    {
        public static Ray ScreenToWorldRay(CameraComponent cameraComponent, Vector2 position)
        {
            if (cameraComponent == null)
            {
                throw new ArgumentNullException(nameof(cameraComponent));
            }

            Matrix inverseViewProjection = Matrix.Invert(cameraComponent.ViewProjectionMatrix);

            Vector3 clipSpace;
            clipSpace.X = position.X * 2f - 1f;
            clipSpace.Y = 1f - position.Y * 2f;

            clipSpace.Z = 0f;
            var near = Vector3.TransformCoordinate(clipSpace, inverseViewProjection);

            clipSpace.Z = 1f;
            var far = Vector3.TransformCoordinate(clipSpace, inverseViewProjection);

            return new Ray(near, Vector3.Normalize(far - near));
        }
    }
}
