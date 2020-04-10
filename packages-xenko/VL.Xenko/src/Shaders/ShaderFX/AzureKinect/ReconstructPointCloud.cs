using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using Xenko.Core.Mathematics;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;

namespace VL.Xenko.Shaders.ShaderFX.Functions
{
    /// <summary>
    /// Represents the ReconstructPointcloud shader
    /// </summary>
    public class ReconstructPointCloud : Funk2In1Out<float, Vector2, Vector4>
    {
        public ReconstructPointCloud(string functionName, IEnumerable<KeyValuePair<string, IComputeNode>> inputs)
            : base(functionName, inputs)
        {
           
        }
    }
}
