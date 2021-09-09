using Stride.Core.Mathematics;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Stride.Rendering.Materials
{
    public class VLMaterialEmissiveFeature : IMaterialEmissiveFeature
    {
        public IComputeNode VertexAddition { get; set; }

        public IComputeNode PixelAddition { get; set; }

        public IComputeNode MaterialExtension { get; set; }
        
        public IMaterialEmissiveFeature MaterialEmissiveFeature { get; set; }

        public bool Enabled { get; set; } = true;

        public bool Equals(IMaterialShadingModelFeature other)
        {
            return other is MaterialEmissiveMapFeature;
        }

        public void Visit(MaterialGeneratorContext context)
        {
            MaterialEmissiveFeature?.Visit(context);

            if (Enabled && context.Step == MaterialGeneratorStep.GenerateShader)
            {
                AddMaterialExtension(context);

                if (VertexAddition != null)
                {
                    AddVertexAddition(MaterialShaderStage.Vertex, context);
                    //context.AddFinalCallback(MaterialShaderStage.Vertex, AddVertexAddition);
                }

                if (PixelAddition != null)
                {
                    AddPixelAddition(MaterialShaderStage.Pixel, context);
                    //context.AddFinalCallback(MaterialShaderStage.Pixel, AddPixelAddition);
                }
            }
        }

        void AddMaterialExtension(MaterialGeneratorContext context)
        {
            var enableExtension = MaterialExtension != null;
            context.Parameters.Set(VLEffectParameters.EnableExtensionShader, enableExtension);

            if (enableExtension)
                context.Parameters.Set(VLEffectParameters.MaterialExtensionShader, MaterialExtension.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White)));
        }

        void AddVertexAddition(MaterialShaderStage stage, MaterialGeneratorContext context)
        {
            var shaderSource = VertexAddition.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White));
            context.AddShaderSource(MaterialShaderStage.Vertex, shaderSource);
        }

        void AddPixelAddition(MaterialShaderStage stage, MaterialGeneratorContext context)
        {
            var shaderSource = PixelAddition.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White));
            context.AddShaderSource(MaterialShaderStage.Pixel, shaderSource);
        }
    }
}
