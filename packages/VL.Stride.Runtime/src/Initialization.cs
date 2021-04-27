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
            // Use our own static node factory cache to manage the lifetime of our factories. The cache provided by VL itself is only per compilation.
            // The node factory cache will invalidate itself in case a factory or one of its nodes invalidates.
            // Not doing so can cause the hotswap to exchange nodes thereby causing weired crashes when for example
            // one of those nodes being re-created is the graphics compositor.

            RegisterStaticNodeFactory(services, "VL.Stride.Graphics.Nodes", nodeFactory =>
            {
                return GraphicsNodes.GetNodeDescriptions(nodeFactory);
            });

            RegisterStaticNodeFactory(services, "VL.Stride.Rendering.Nodes", nodeFactory =>
            {
                return MaterialNodes.GetNodeDescriptions(nodeFactory)
                    .Concat(LightNodes.GetNodeDescriptions(nodeFactory))
                    .Concat(CompositingNodes.GetNodeDescriptions(nodeFactory))
                    .Concat(RenderingNodes.GetNodeDescriptions(nodeFactory));
            });

            RegisterStaticNodeFactory(services, "VL.Stride.Engine.Nodes", nodeFactory =>
            {
                return EngineNodes.GetNodeDescriptions(nodeFactory)
                    .Concat(PhysicsNodes.GetNodeDescriptions(nodeFactory))
                    .Concat(VRNodes.GetNodeDescriptions(nodeFactory))
                    ;
            });

            RegisterStaticNodeFactory(services, "VL.Stride.Rendering.EffectShaderNodes", init: EffectShaderNodes.Init);
        }

        void RegisterStaticNodeFactory(IVLFactory services, string name, Func<IVLNodeDescriptionFactory, IEnumerable<IVLNodeDescription>> init)
        {
            RegisterStaticNodeFactory(services, name, nodeFactory => NodeBuilding.NewFactoryImpl(init(nodeFactory).ToImmutableArray()));
        }

        void RegisterStaticNodeFactory(IVLFactory services, string name, Func<IVLNodeDescriptionFactory, NodeBuilding.FactoryImpl> init)
        {
            var cachedFactory = staticCache.GetOrAdd(name, () => NodeBuilding.NewNodeFactory(services, name, init));
            services.RegisterNodeFactory(cachedFactory);
        }

        static readonly NodeFactoryCache staticCache = new NodeFactoryCache();
    }
}