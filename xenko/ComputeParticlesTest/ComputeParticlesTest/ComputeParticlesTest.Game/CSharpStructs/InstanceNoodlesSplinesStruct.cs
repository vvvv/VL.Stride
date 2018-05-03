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
    public struct SplineSettings
    {
        int VertexPerSpline;
        int ControlPointPerSpline;
        bool Loop;
        float TesselationFactor;
        float Radius;

        public SplineSettings(int vertexPerSpline, int controlPointPerSpline, bool loop, float tesselationFactor, float radius)
        {
            VertexPerSpline = vertexPerSpline;
            ControlPointPerSpline = controlPointPerSpline;
            Loop = loop;
            TesselationFactor = tesselationFactor;
            Radius = radius;
        }
    }
}