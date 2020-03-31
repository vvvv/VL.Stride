using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using Xenko.Shaders.Compiler;

namespace VL.Xenko.Shaders
{
    public class ShaderFXGeneratorContext : ShaderGeneratorContext
    {
        public EffectSystem EffectSystem { get; }
        private GraphicsDevice graphicsDevice;
        private EffectCompilerParameters effectCompilerParameters = EffectCompilerParameters.Default;

        public ShaderFXGeneratorContext(GraphicsDevice graphicsDevice, EffectSystem effectSystem)
            : base(graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            EffectSystem = effectSystem;
        }

        public void CompileShader(ShaderSource source)
        {
            // Setup compilation parameters
            var compilerParameters = new CompilerParameters();
            compilerParameters.EffectParameters.Platform = GraphicsDevice.Platform;
            compilerParameters.EffectParameters.Profile = graphicsDevice.Features.RequestedProfile;


            // optimization/debug levels
#if DEBUG
            compilerParameters.EffectParameters.OptimizationLevel = 0;
            compilerParameters.EffectParameters.Debug = true;
#else
            compilerParameters.EffectParameters.OptimizationLevel = 2;
            compilerParameters.EffectParameters.Debug = false;
#endif

            var compiler = EffectSystem.Compiler;
            var compilerResults = compiler.Compile(source, compilerParameters);
        }
    }
}
