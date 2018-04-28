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
    public struct PointcloudProcessorSettings
    {
        float Points_Size;
        float Culling_DotThreshold;
        Matrix Pointcloud_T;
        float pad1;
        float pad2;

        public PointcloudProcessorSettings(float points_Size, float culling_DotThreshold, Matrix pointcloud_T, float pad1, float pad2)
        {
            Points_Size = points_Size;
            Culling_DotThreshold = culling_DotThreshold;
            Pointcloud_T = pointcloud_T;
            this.pad1 = pad1;
            this.pad2 = pad2;
        }
    }
}