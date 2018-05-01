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
    public struct StreamsDrawStaticSettings
    {
        int TubeShapeRes;// = 7;
        int RingBuffer_BinSize;// = 4; // Ctrl Points per Filament
        int GeomPatch_BinSize;// = 1;
        float GeomPatch_BinSizeInv;// = 1;
        float LengthMult;// = 1;
        Vector2 TessFactor_DistMinMax;// = float2(100, 600);
        Vector2 TessFactor_MinMaxValue;// = float2(64, 2);
        float Stripes_NormLerp;// = 1;
        float DotCullingThresholdValue;// = 0.61f;
        Vector4 Col_mapVel;// = float4(0,1,0,1);

        public StreamsDrawStaticSettings(int tubeShapeRes, int ringBuffer_BinSize, int geomPatch_BinSize, float geomPatch_BinSizeInv, float lengthMult, Vector2 tessFactor_DistMinMax, Vector2 tessFactor_MinMaxValue, float stripes_NormLerp, float dotCullingThresholdValue, Vector4 col_mapVel)
        {
            TubeShapeRes = tubeShapeRes;
            RingBuffer_BinSize = ringBuffer_BinSize;
            GeomPatch_BinSize = geomPatch_BinSize;
            GeomPatch_BinSizeInv = geomPatch_BinSizeInv;
            LengthMult = lengthMult;
            TessFactor_DistMinMax = tessFactor_DistMinMax;
            TessFactor_MinMaxValue = tessFactor_MinMaxValue;
            Stripes_NormLerp = stripes_NormLerp;
            DotCullingThresholdValue = dotCullingThresholdValue;
            Col_mapVel = col_mapVel;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StreamsDrawRareUpdateSettings
    {
        Vector2 FogMinMaxDist;// : FOG_MINMAXDISTANCE ;
        float Thickness;// = 0.1;
        float ColMult;// = 1;
        float UcoordDeathValue;// = 1;
        Vector4 Col_mapVel;// = float4(0,1,0,1);

        public StreamsDrawRareUpdateSettings(Vector2 fogMinMaxDist, float thickness, float colMult, float ucoordDeathValue, Vector4 col_mapVel)
        {
            FogMinMaxDist = fogMinMaxDist;
            Thickness = thickness;
            ColMult = colMult;
            UcoordDeathValue = ucoordDeathValue;
            Col_mapVel = col_mapVel;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StreamsDrawDynamicSettings
    {
        Matrix ViewProjectionInverse;
        int RingBufferShift;
        float InfraFrame_Cycle;

        public StreamsDrawDynamicSettings(Matrix viewProjectionInverse, int ringBufferShift, float infraFrame_Cycle)
        {
            ViewProjectionInverse = viewProjectionInverse;
            RingBufferShift = ringBufferShift;
            InfraFrame_Cycle = infraFrame_Cycle;
        }
    }

}