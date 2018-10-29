using System.Collections.Generic;
using System.IO;
using VL.Core;
using Xenko.Core;
using Xenko.Core.IO;
using Xenko.Core.Serialization.Contents;
using Xenko.Engine;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Shaders.Compiler;
using VLServiceRegistry = VL.Core.ServiceRegistry;

[assembly: NodeFactory(typeof(VL.Xenko.EffectLib.EffectNodeFactory))]

namespace VL.Xenko.EffectLib
{
    public class EffectNodeFactory : IVLNodeDescriptionFactory
    {
        public readonly ContentManager ContentManger;
        public readonly EffectSystem EffectSystem;
        public readonly IServiceRegistry ServiceRegistry;
        public readonly IGraphicsDeviceService DeviceService;

        Dictionary<string, IVLNodeDescription> nodeDescriptions;

        public EffectNodeFactory()
        {
            var game = VLServiceRegistry.Default.GetService<Game>();
            ServiceRegistry = game?.Services;
            ContentManger = game?.Content;
            EffectSystem = game?.EffectSystem;
            DeviceService = game?.GraphicsDeviceManager;
        }

        public IEnumerable<IVLNodeDescription> GetNodeDescriptions() => NodeDescriptions.Values;

        public IVLNodeDescription GetNodeDescription(string name, string category) => NodeDescriptions.ValueOrDefault(name);

        Dictionary<string, IVLNodeDescription> NodeDescriptions => nodeDescriptions ?? (nodeDescriptions = Load());

        Dictionary<string, IVLNodeDescription> Load()
        {
            lock (this)
            {
                if (this.nodeDescriptions != null)
                    return this.nodeDescriptions;

                var nodeDescriptions = new Dictionary<string, IVLNodeDescription>();
                if (ContentManger != null)
                {
                    var files = ContentManger.FileProvider.ListFiles(EffectCompilerBase.DefaultSourceShaderFolder, "*.xksl", VirtualSearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var effectName = Path.GetFileNameWithoutExtension(file);
                        var description = new EffectNodeDescription(this, effectName);
                        nodeDescriptions[effectName] = description;
                    }
                }
                return nodeDescriptions;
            }
        }
    }
}
