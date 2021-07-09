using Stride.Graphics;
using Stride.Rendering;
using System;
using System.Collections.Generic;
using VL.Core;

namespace VL.Stride.Rendering
{
    class MipmapPinManager : IDisposable
    {
        readonly NodeContext nodeContext;
        readonly IVLPin<Texture> texturePin;
        readonly IVLPin<Texture> shaderTexturePin;
        readonly IVLPin<bool> alwaysGeneratePin;
        readonly string profilerName;

        Texture lastInputTexture;
        MipMapGenerator generator;
        bool render;

        public MipmapPinManager(NodeContext nodeContext, IVLPin<Texture> texturePin, IVLPin<Texture> shaderTexturePin, IVLPin<bool> alwaysGeneratePin, string profilerName = "Pin MipMap Generator")
        {
            this.nodeContext = nodeContext;
            this.texturePin = texturePin;
            this.shaderTexturePin = shaderTexturePin;
            this.alwaysGeneratePin = alwaysGeneratePin;
            this.profilerName = profilerName;
        }

        public void Update()
        {
            var currentInputTexture = texturePin.Value;

            // Input already has mips
            if (currentInputTexture?.MipLevels > 1)
            {
                shaderTexturePin.Value = currentInputTexture;
                generator?.Dispose();
                generator = null;
                render = false;
                return; //done
            }

            // Mips must be generated 
            generator ??= new MipMapGenerator(nodeContext) { Name = profilerName };

            generator.InputTexture = currentInputTexture;
            shaderTexturePin.Value = generator.OutputTexture;

            render = alwaysGeneratePin.Value || currentInputTexture != lastInputTexture;

            lastInputTexture = currentInputTexture;
        }

        public void Draw(RenderDrawContext context)
        {
            if (render)
                generator.Draw(context);
        }

        public void Dispose()
        {
            generator?.Dispose();
            generator = null;
        }
    }

    public class TextureFXMipmapManager : IGraphicsRendererBase, IDisposable
    {
        readonly NodeContext nodeContext;

        List<MipmapPinManager> pins = new List<MipmapPinManager>();

        public TextureFXMipmapManager(NodeContext nodeContext)
        {
            this.nodeContext = nodeContext;
        }

        public void AddInput(IVLPin<Texture> texturePin, IVLPin<Texture> shaderTexturePin, IVLPin<bool> alwaysGeneratePin, string profilerName)
        {
            pins.Add(new MipmapPinManager(nodeContext, texturePin, shaderTexturePin, alwaysGeneratePin, profilerName));
        }

        public void Update()
        {
            for (int i = 0; i < pins.Count; i++)
            {
                pins[i].Update();
            }
        }

        public void Draw(RenderDrawContext context)
        {
            for (int i = 0; i < pins.Count; i++)
            {
                pins[i].Draw(context);
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < pins.Count; i++)
            {
                pins[i].Dispose();
            }

            pins.Clear();
        }
    }
}
