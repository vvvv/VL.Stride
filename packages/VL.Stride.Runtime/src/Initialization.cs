using Stride.Graphics;
using Stride.Input;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VL.Core;
using VL.Core.CompilerServices;
using VL.Lib.Basics.Resources;
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
            // Cache the factories in a static field - their node definitions will never change.
            // Not doing so can cause the hotswap to exchange nodes thereby causing weired crashes when for example
            // one of those nodes being re-created is the graphics compositor.

            RegisterStaticNodeFactory(ref graphicsNodes, services, "VL.Stride.Graphics.Nodes", nodeFactory =>
            {
                return GraphicsNodes.GetNodeDescriptions(nodeFactory);
            });

            RegisterStaticNodeFactory(ref renderingNodes, services, "VL.Stride.Rendering.Nodes", nodeFactory =>
            {
                return MaterialNodes.GetNodeDescriptions(nodeFactory)
                    .Concat(LightNodes.GetNodeDescriptions(nodeFactory))
                    .Concat(CompositingNodes.GetNodeDescriptions(nodeFactory))
                    .Concat(RenderingNodes.GetNodeDescriptions(nodeFactory));
            });

            RegisterStaticNodeFactory(ref engineNode, services, "VL.Stride.Engine.Nodes", nodeFactory =>
            {
                return EngineNodes.GetNodeDescriptions(nodeFactory)
                    .Concat(PhysicsNodes.GetNodeDescriptions(nodeFactory));
            });

            EffectShaderNodes.Register(services);
        }

        void RegisterStaticNodeFactory(ref IVLNodeDescriptionFactory location, IVLFactory services, string name, Func<IVLNodeDescriptionFactory, IEnumerable<IVLNodeDescription>> factory)
        {
            services.RegisterNodeFactory(location ?? (location = NodeBuilding.NewNodeFactory(services, name, nodeFactory =>
            {
                var nodes = ImmutableArray.CreateBuilder<IVLNodeDescription>();

                nodes.AddRange(factory(nodeFactory));

                return NodeBuilding.NewFactoryImpl(nodes.ToImmutable());
            })));
        }

        static IVLNodeDescriptionFactory graphicsNodes, renderingNodes, engineNode;
    }
}