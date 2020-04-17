using System;
using System.Runtime.Serialization;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Rendering.Materials.ComputeColors;

namespace VL.Xenko.Shaders.ShaderFX
{
    public interface IComputeVoid : IComputeNode
    {

    }

    public abstract class ComputeVoid : ComputeNode, IComputeVoid
    {

    }
}
