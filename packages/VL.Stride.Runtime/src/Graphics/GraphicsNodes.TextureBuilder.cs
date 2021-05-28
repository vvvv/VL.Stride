using Stride.Core;
using Stride.Engine;
using Stride.Graphics;
using VL.Core;
using VL.Lib.Basics.Resources;

namespace VL.Stride.Graphics
{
    static partial class GraphicsNodes
    {
        class TextureBuilder
        {
            private TextureDescription description;
            private TextureViewDescription viewDescription;
            private DataBox[] initalData;
            private bool needsRebuild = true;
            private Texture texture;
            internal bool Recreate;
            private readonly IResourceHandle<Game> gameHandle;

            public Texture Texture
            {
                get
                {
                    if (needsRebuild || Recreate)
                    {
                        RebuildTexture();
                        needsRebuild = false;
                    }
                    return texture;
                }

                private set => texture = value;
            }

            public TextureDescription Description
            {
                get => description;
                set
                {
                    description = value;
                    needsRebuild = true;
                }
            }

            public TextureViewDescription ViewDescription
            {
                get => viewDescription;
                set
                {
                    viewDescription = value;
                    needsRebuild = true;
                }
            }

            public DataBox[] InitalData
            {
                get => initalData;
                set
                {
                    initalData = value;
                    needsRebuild = true;
                }
            }

            public TextureBuilder(NodeContext nodeContext)
            {
                gameHandle = nodeContext.GetGameHandle();
            }

            public void Dispose()
            {
                texture?.Dispose();
                texture = null;
                gameHandle.Dispose();
            }

            private void RebuildTexture()
            {
                try
                {
                    texture?.Dispose();
                    texture = null;
                    var game = gameHandle.Resource;
                    texture = Texture.New(game.GraphicsDevice, description, viewDescription, initalData);
                }
                catch
                {
                    texture = null;
                }
            }
        }

        class TextureViewBuilder
        {
            private Texture texture;
            private TextureViewDescription viewDescription;
            private bool needsRebuild = true;
            private Texture textureView;
            internal bool Recreate;
            private readonly IResourceHandle<Game> gameHandle;

            public Texture TextureView
            {
                get
                {
                    if (needsRebuild || Recreate)
                    {
                        RebuildTextureView();
                        needsRebuild = false;
                    }
                    return textureView;
                }

                private set => textureView = value;
            }

            public Texture Input
            {
                get => texture;
                set
                {
                    texture = value;
                    needsRebuild = true;
                }
            }

            public TextureViewDescription ViewDescription
            {
                get => viewDescription;
                set
                {
                    viewDescription = value;
                    needsRebuild = true;
                }
            }

            public TextureViewBuilder(NodeContext nodeContext)
            {
                gameHandle = nodeContext.GetGameHandle();
            }

            public void Dispose()
            {
                textureView?.Dispose();
                textureView = null;
                gameHandle.Dispose();
            }

            private void RebuildTextureView()
            {
                try
                {
                    if (textureView != null)
                    {
                        textureView.Destroyed -= TextureView_Destroyed;
                        textureView.Dispose();
                        textureView = null; 
                    }

                    if (texture != null && (
                        viewDescription.Format == PixelFormat.None
                        || (texture.Format == viewDescription.Format)
                        || (texture.Format.IsTypeless() && (texture.Format.BlockSize() == viewDescription.Format.BlockSize()))
                        ))
                    {
                        var game = gameHandle.Resource;
                        textureView = texture.ToTextureView(viewDescription);
                        textureView.DisposeBy(texture);
                        textureView.Destroyed += TextureView_Destroyed;
                    }
                }
                catch
                {
                    textureView = null;
                }
            }

            private void TextureView_Destroyed(object sender, System.EventArgs e)
            {
                textureView = null;
            }
        }
    }
}
