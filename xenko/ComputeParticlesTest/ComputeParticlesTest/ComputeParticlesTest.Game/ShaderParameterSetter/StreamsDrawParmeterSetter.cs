using SiliconStudio.Xenko.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputeParticlesTest.ShaderParameterSetter
{
    public static class StreamsDrawParmeterSetter
    {
        public static ParameterCollection SetStreamsDrawParameters(ParameterCollection parameters, int tubeShapeRes, int geomPatch_BinSize)
        {
            //parameters.Set(StreamsDrawKeys.TubeShapeRes, tubeShapeRes);
            //parameters.Set(StreamsDrawKeys.GeomPatch_BinSize, geomPatch_BinSize);

            return parameters;
        }
    }
}
