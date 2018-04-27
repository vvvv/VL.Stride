
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
    public struct AttrSettings
    {
        public Vector4 pos;
        public float radius;
        public float strength;
        public float gamma;
        public Matrix vecT;
        public float pad;

        public AttrSettings(Vector4 pos, Matrix vecT, float pad, float radius = 1, float strength = 0, float gamma = 1)
        {
            this.pos = pos;
            this.radius = radius;
            this.strength = strength;
            this.gamma = gamma;
            this.vecT = vecT;
            this.pad = pad;
        }
    }
}
