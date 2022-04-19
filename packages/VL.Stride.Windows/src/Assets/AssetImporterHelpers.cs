//using Stride.Assets.Textures;
//using Stride.Core;
//using Stride.Core.Assets;
//using Stride.Core.Assets.Compiler;
//using Stride.Core.Serialization.Contents;
//using Stride.TextureConverter;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace VL.Stride.Windows.Assets
//{
//    public static class AssetImporterHelpers
//    {
//        protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
//        {
//            var convertParameters = new TextureHelper.ImportParameters(Parameters) { OutputUrl = Url };

//            using (var texTool = new TextureTool())
//            using (var texImage = texTool.Load(Parameters.SourcePathFromDisk, convertParameters.IsSRgb))
//            {
//                var importResult = Import(commandContext, texTool, texImage, convertParameters);

//                return Task.FromResult(importResult);
//            }
//        }

//        private ResultStatus Import(ICommandContext commandContext, TextureTool textureTool, TexImage texImage, TextureHelper.ImportParameters convertParameters)
//        {
//            var assetManager = new ContentManager(MicrothreadLocalDatabases.ProviderService);
//            var useSeparateDataContainer = TextureHelper.ShouldUseDataContainer(Parameters.IsStreamable, texImage.Dimension);

//            // Note: for streamable textures we want to store mip maps in a separate storage container and read them on request instead of whole asset deserialization (at once)

//            return useSeparateDataContainer
//                ? TextureHelper.ImportStreamableTextureImage(assetManager, textureTool, texImage, convertParameters, CancellationToken, commandContext)
//                : TextureHelper.ImportTextureImage(assetManager, textureTool, texImage, convertParameters, CancellationToken, commandContext.Logger);
//        }

//        public static void CompileTexture(AssetItem item)
//        {
//            var context = new AssetCompilerContext()
//            {
//                Platform = PlatformType.Windows,
//                CompilationContext = typeof(AssetCompilationContext)
//            };

//            var compiler = new TextureAssetCompiler();
//            var result = compiler.Prepare(context, item);
//            foreach(var buildStep in result.BuildSteps)
//            {
//                buildStep.Execute()
//            }
//        }
//    }
//}
