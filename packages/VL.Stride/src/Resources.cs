using System;
using System.Linq;
using VL.Core;
using VL.Lib.Basics.Resources;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Core.Annotations;

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

        // Game window
        public static IResourceProvider<GameWindow> GetGameWindowProvider(this NodeContext nodeContext)
        {
            return nodeContext.Factory.CreateService<IResourceProvider<GameWindow>>(nodeContext);
        }

        public static IResourceHandle<GameWindow> GetGameWindowHandle(this NodeContext nodeContext)
        {
            return nodeContext.GetGameWindowProvider().GetHandle();
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
        public static IResourceProvider<Resource<GraphicsContext>> GetGraphicsContextProvider(this NodeContext nodeContext)
        {
            return nodeContext.Factory.CreateService<IResourceProvider<Resource<GraphicsContext>>>(nodeContext);
        }

        public static IResourceHandle<Resource<GraphicsContext>> GetGraphicsContextHandle(this NodeContext nodeContext)
        {
            return nodeContext.GetGraphicsContextProvider().GetHandle();
        }
    }

    
}
