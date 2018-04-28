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
        float Points_Count;
        float Points_Size;
        float Culling_DotThreshold;
        Matrix Pointcloud_T;
        float pad;

        public PointcloudProcessorSettings(float points_Count, float points_Size, float culling_DotThreshold, Matrix pointcloud_T, float pad)
        {
            Points_Count = points_Count;
            Points_Size = points_Size;
            Culling_DotThreshold = culling_DotThreshold;
            Pointcloud_T = pointcloud_T;
            this.pad = pad;
        }
    }
}