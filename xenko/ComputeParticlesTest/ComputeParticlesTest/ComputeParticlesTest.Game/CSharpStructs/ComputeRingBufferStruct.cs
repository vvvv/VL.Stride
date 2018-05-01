
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
    public struct RingBufferSettings
    {
        int ParticlesCount;
        int RingLength;
        int RingBufferPhase;
        int EmissionIndexOffset;

        public RingBufferSettings(int particlesCount, int ringLength, int ringBufferPhase, int emissionIndexOffset)
        {
            ParticlesCount = particlesCount;
            RingLength = ringLength;
            RingBufferPhase = ringBufferPhase;
            EmissionIndexOffset = emissionIndexOffset;
        }
    }
}
