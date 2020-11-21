using Stride.Core.IO;
using Stride.Rendering;
using Stride.Shaders.Compiler;
using Stride.Shaders.Parser;
using System.Collections.Generic;
using System.Reflection;
using VL.Stride.Core.IO;

namespace VL.Stride.Rendering
{
    static class EffectSystemExtensions
    {
        public static void InstallEffectCompilerWithCustomPaths(this EffectSystem effectSystem)
        {
            var databaseProvider = effectSystem.Services.GetService<IDatabaseFileProviderService>();
            var shaderFileProvider = new ShaderFileProvider(databaseProvider.FileProvider);
            effectSystem.Services.AddService(shaderFileProvider);
            effectSystem.Compiler = EffectCompilerFactory.CreateEffectCompiler(shaderFileProvider, effectSystem, database: databaseProvider.FileProvider);
        }

        public static void AddShaderSource(this EffectSystem effectSystem, string type, string sourceCode, string sourcePath)
        {
            var compiler = effectSystem.Compiler as EffectCompiler;
            if (compiler is null && effectSystem.Compiler is EffectCompilerCache effectCompilerCache)
                compiler = typeof(EffectCompilerChain).GetProperty("Compiler", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(effectCompilerCache) as EffectCompiler;
            if (compiler != null)
            {
                var getParserMethod = typeof(EffectCompiler).GetMethod("GetMixinParser", BindingFlags.Instance | BindingFlags.NonPublic);
                var parser = getParserMethod.Invoke(compiler, null) as ShaderMixinParser;
                var sourceManager = parser.SourceManager;
                sourceManager.AddShaderSource(type, sourceCode, sourcePath);
            }
        }

        public static void EnsurePathIsVisible(this EffectSystem effectSystem, string path)
        {
            var shaderFileProvider = effectSystem.Services.GetService<ShaderFileProvider>();
            if (shaderFileProvider != null)
                shaderFileProvider.Ensure(path);
        }

        sealed class ShaderFileProvider : AggregateFileProvider
        {
            private readonly HashSet<string> paths = new HashSet<string>();

            public ShaderFileProvider(params IVirtualFileProvider[] virtualFileProviders)
                : base(virtualFileProviders)
            {
            }

            public void Ensure(string path)
            {
                lock (paths)
                {
                    if (paths.Add(path))
                        Add(new FileSystemProvider(rootPath: null, path));
                }
            }
        }
    }
}
