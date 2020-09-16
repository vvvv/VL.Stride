using Stride.Graphics;
using Stride.Input;
using System.Collections.Immutable;
using VL.Core;
using VL.Core.CompilerServices;
using VL.Lib.Basics.Resources;
using VL.Stride.EffectLib;
using VL.Stride.Engine;
using VL.Stride.Graphics;
using VL.Stride.Rendering;
using VL.Stride.Rendering.Compositing;
using VL.Stride.Rendering.Lights;
using VL.Stride.Rendering.Materials;

[assembly: AssemblyInitializer(typeof(VL.Stride.Core.Initialization))]

namespace VL.Stride.Core
{
    public sealed class Initialization : AssemblyInitializer<Initialization>
    {
        protected override void RegisterServices(IVLFactory factory)
        {
            Serialization.RegisterSerializers(factory);

            // Graphics device
            factory.RegisterService<NodeContext, IResourceProvider<GraphicsDevice>>(nodeContext =>
            {
                var gameProvider = nodeContext.GetGameProvider();
                return gameProvider.Bind(game => ResourceProvider.Return(game.GraphicsDevice));
            });

            // Graphics context
            factory.RegisterService<NodeContext, IResourceProvider<GraphicsContext>>(nodeContext =>
            {
                var gameProvider = nodeContext.GetGameProvider();
                return gameProvider.Bind(game => ResourceProvider.Return(game.GraphicsContext));
            });

            // Input manager
            factory.RegisterService<NodeContext, IResourceProvider<InputManager>>(nodeContext =>
            {
                var gameProvider = nodeContext.GetGameProvider();
                return gameProvider.Bind(game => ResourceProvider.Return(game.Input));
            });

            RegisterNodeFactories(factory);
        }

        void RegisterNodeFactories(IVLFactory services)
        {
            services.RegisterNodeFactory("VL.Stride.Graphics.Nodes", nodeFactory =>
            {
                var nodes = ImmutableArray.CreateBuilder<IVLNodeDescription>();

                nodes.AddRange(GraphicsNodes.GetNodeDescriptions(nodeFactory));

                return NodeBuilding.NewFactoryImpl(nodes.ToImmutable());
            });

            services.RegisterNodeFactory("VL.Stride.Rendering.Nodes", nodeFactory =>
            {
                var nodes = ImmutableArray.CreateBuilder<IVLNodeDescription>();

                nodes.AddRange(MaterialNodes.GetNodeDescriptions(nodeFactory));
                nodes.AddRange(LightNodes.GetNodeDescriptions(nodeFactory));
                nodes.AddRange(CompositingNodes.GetNodeDescriptions(nodeFactory));
                nodes.AddRange(RenderingNodes.GetNodeDescriptions(nodeFactory));

                return NodeBuilding.NewFactoryImpl(nodes.ToImmutable());
            });

            services.RegisterNodeFactory("VL.Stride.Engine.Nodes", nodeFactory =>
            {
                var nodes = ImmutableArray.CreateBuilder<IVLNodeDescription>();

                nodes.AddRange(EngineNodes.GetNodeDescriptions(nodeFactory));
                nodes.AddRange(PhysicsNodes.GetNodeDescriptions(nodeFactory));

                return NodeBuilding.NewFactoryImpl(nodes.ToImmutable());
            });

            //services.RegisterNodeFactory(effectFactory ?? (effectFactory = new EffectNodeFactory()));
            TextureFXNodeFactory.Register(services);
        }

        //static IVLNodeDescriptionFactory effectFactory;
    }
}