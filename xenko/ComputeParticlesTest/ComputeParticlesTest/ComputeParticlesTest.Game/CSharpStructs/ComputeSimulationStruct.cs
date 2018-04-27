
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ComputeParticlesTest
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SimulationSettings
    {
        Vector4 Gravity;
        float VelDamp_delta;
        float VelDamp_min;
        float VelDamp_gamma;
        float Target_Enable;
        float Target_Strength;
        Matrix Target_T;
        float Bounce_Enable;
        float Bounce_Friction;
        float Bounce_Damp;

        public SimulationSettings(Vector4 gravity, float velDamp_delta, float velDamp_min, float velDamp_gamma, float target_Enable, float target_Strength, Matrix target_T, float bounce_Enable, float bounce_Friction, float bounce_Damp)
        {
            Gravity = gravity;
            VelDamp_delta = velDamp_delta;
            VelDamp_min = velDamp_min;
            VelDamp_gamma = velDamp_gamma;
            Target_Enable = target_Enable;
            Target_Strength = target_Strength;
            Target_T = target_T;
            Bounce_Enable = bounce_Enable;
            Bounce_Friction = bounce_Friction;
            Bounce_Damp = bounce_Damp;
        }
    }
}
