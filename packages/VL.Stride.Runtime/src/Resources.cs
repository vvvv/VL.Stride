using System;
using System.Linq;
using VL.Core;
using VL.Lib.Basics.Resources;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Core.Annotations;
using Stride.Input;

namespace VL.Stride
{
    public static class Resources
    {
        // Game
        public static IResourceProvider<Game> GetGameProvider(this NodeContext nodeContext)
        {
            return nodeContext.Factory.CreateService<IResourceProvider<Game>>(nodeContext);
        }

        public static IResourceHandle<Game> GetGameHandle(this NodeContext nodeContext)
        {
            return nodeContext.GetGameProvider().GetHandle();
        }

        // Graphics device
        public static IResourceProvider<GraphicsDevice> GetDeviceProvider(this NodeContext nodeContext)
        {
            return nodeContext.Factory.CreateService<IResourceProvider<GraphicsDevice>>(nodeContext);
        }

        public static IResourceHandle<GraphicsDevice> GetDeviceHandle(this NodeContext nodeContext)
        {
            return nodeContext.GetDeviceProvider().GetHandle();
        }

        // Graphics context
        public static IResourceProvider<GraphicsContext> GetGraphicsContextProvider(this NodeContext nodeContext)
        {
            return nodeContext.Factory.CreateService<IResourceProvider<GraphicsContext>>(nodeContext);
        }

        public static IResourceHandle<GraphicsContext> GetGraphicsContextHandle(this NodeContext nodeContext)
        {
            return nodeContext.GetGraphicsContextProvider().GetHandle();
        }

        // Input manager
        public static IResourceProvider<InputManager> GetInputManagerProvider(this NodeContext nodeContext)
        {
            return nodeContext.Factory.CreateService<IResourceProvider<InputManager>>(nodeContext);
        }

        public static IResourceHandle<InputManager> GetInputManagerHandle(this NodeContext nodeContext)
        {
            return nodeContext.GetInputManagerProvider().GetHandle();
        }
    }

    
}
