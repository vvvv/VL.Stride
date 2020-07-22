using Stride.Core;
using Stride.Core.IO;
using Stride.Core.Serialization.Contents;
using Stride.Core.Storage;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Shaders.Compiler;

namespace VL.Stride
{
    /// <summary>
    /// Used to share one common service registry between our node factories.
    /// </summary>
    static class SharedServices
    {
        static readonly object syncRoot = new object();
        static ServiceRegistry serviceRegistry;

        public static ServiceRegistry GetRegistry()
        {
            lock (syncRoot)
            {
                return serviceRegistry ??= Init();
            }
        }

        // Taken from Stride/SkyboxGeneratorContext
        static ServiceRegistry Init()
        {
            var services = new ServiceRegistry();

            var fileProvider = InitializeAssetDatabase();
            var fileProviderService = new DatabaseFileProviderService(fileProvider);
            services.AddService<IDatabaseFileProviderService>(fileProviderService);

            var Content = new ContentManager(services);
            services.AddService<IContentManager>(Content);
            services.AddService(Content);

            var graphicsDevice = GraphicsDevice.New();
            var graphicsDeviceService = new GraphicsDeviceServiceLocal(services, graphicsDevice);
            services.AddService<IGraphicsDeviceService>(graphicsDeviceService);

            var graphicsContext = new GraphicsContext(graphicsDevice);
            services.AddService(graphicsContext);

            var effectSystem = new EffectSystem(services);
            effectSystem.Compiler = EffectCompilerFactory.CreateEffectCompiler(Content.FileProvider, effectSystem);

            services.AddService(effectSystem);
            effectSystem.Initialize();
            ((IContentable)effectSystem).LoadContent();
            ((EffectCompilerCache)effectSystem.Compiler).CompileEffectAsynchronously = false;

            return services;
        }

        // Taken from Stride/Game
        static DatabaseFileProvider InitializeAssetDatabase()
        {
            // Create and mount database file system
            var objDatabase = ObjectDatabase.CreateDefaultDatabase();

            // Only set a mount path if not mounted already
            var mountPath = VirtualFileSystem.ResolveProviderUnsafe("/asset", true).Provider == null ? "/asset" : null;
            return new DatabaseFileProvider(objDatabase, mountPath);
        }
    }
}
