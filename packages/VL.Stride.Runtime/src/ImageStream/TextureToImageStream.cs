using Stride.Graphics;
using Stride.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using VL.Core;
using VL.Lib.Basics.Imaging;
using VL.Lib.Basics.Resources;
using StridePixelFormat = Stride.Graphics.PixelFormat;
using VLPixelFormat = VL.Lib.Basics.Imaging.PixelFormat;

namespace VL.Stride.ImageStream
{
    public sealed class TextureToImageStream : RendererBase
    {
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private readonly Queue<(Texture texture, string metadata)> textureDownloads = new Queue<(Texture texture, string metadata)>();
        private readonly Subject<IResourceProvider<IImage>> imageStream = new Subject<IResourceProvider<IImage>>();
        private readonly ServiceRegistry serviceRegistry;
        private readonly CompositeDisposable subscriptions;
        private readonly SerialDisposable texturePoolSubscription;

        public TextureToImageStream()
        {
            serviceRegistry = ServiceRegistry.Current;
            subscriptions = new CompositeDisposable()
            {
                (texturePoolSubscription = new SerialDisposable()),
            };
        }

        public Texture Texture { get; set; }

        public string Metadata { get; set; }

        public IObservable<IResourceProvider<IImage>> ImageStream => imageStream;

        /// <inheritdoc />
        protected override void DrawCore(RenderDrawContext context)
        {
            var texture = Texture;
            if (texture is null)
            {
                texturePoolSubscription.Disposable = null;
                return;
            }

            // Workaround: Re-install the service registry - should be done by vvvv itself in the render callback
            using var _ = serviceRegistry.MakeCurrentIfNone();

            var texturePool = GetTexturePool(context.GraphicsDevice, texture);

            {
                // Request copy
                var stagingTexture = texturePool.Rent();
                context.CommandList.Copy(texture, stagingTexture);
                textureDownloads.Enqueue((stagingTexture, Metadata));
            }

            {
                // Download recently staged
                var (stagedTexture, stagedMetadata) = textureDownloads.Peek();
                var doNotWait = textureDownloads.Count < 4;
                var commandList = context.CommandList;
                var mappedResource = commandList.MapSubresource(stagedTexture, 0, global::Stride.Graphics.MapMode.Read, doNotWait);
                var data = mappedResource.DataBox;
                if (!data.IsEmpty)
                {
                    // Dequeue
                    textureDownloads.Dequeue();

                    // Setup the new image resource
                    var imageInfo = new ImageInfo(stagedTexture.Width, stagedTexture.Height, ToPixelFormat(stagedTexture.Format), isPremultipliedAlpha: true, data.RowPitch, stagedTexture.Format.ToString())
                    {
                        Metadata = stagedMetadata
                    };
                    var image = new IntPtrImage(data.DataPointer, data.SlicePitch, imageInfo);
                    var imageProvider = ResourceProvider.Return(image, ReleaseImage);

                    // Push it downstream
                    imageStream.OnNext(imageProvider);

                    void ReleaseImage(IntPtrImage i)
                    {
                        if (SynchronizationContext.Current != synchronizationContext)
                            synchronizationContext.Post(x => ReleaseImage((IntPtrImage)x), i);
                        else
                        {
                            i.Dispose();
                            if (!IsDisposed)
                                commandList.UnmapSubresource(mappedResource);
                            texturePool.Return(stagedTexture);
                        }
                    }
                }
            }

            static VLPixelFormat ToPixelFormat(StridePixelFormat format)
            {
                switch (format)
                {
                    case StridePixelFormat.R8_UNorm: return VLPixelFormat.R8;
                    case StridePixelFormat.R16_UNorm: return VLPixelFormat.R16;
                    case StridePixelFormat.R32_Float: return VLPixelFormat.R32F;
                    case StridePixelFormat.R8G8B8A8_UNorm: return VLPixelFormat.R8G8B8A8;
                    case StridePixelFormat.R8G8B8A8_UNorm_SRgb: return VLPixelFormat.R8G8B8A8;
                    case StridePixelFormat.B8G8R8X8_UNorm: return VLPixelFormat.B8G8R8X8;
                    case StridePixelFormat.B8G8R8X8_UNorm_SRgb: return VLPixelFormat.B8G8R8X8;
                    case StridePixelFormat.B8G8R8A8_UNorm: return VLPixelFormat.B8G8R8A8;
                    case StridePixelFormat.B8G8R8A8_UNorm_SRgb: return VLPixelFormat.B8G8R8A8;
                    case StridePixelFormat.R16G16B16A16_Float: return VLPixelFormat.R16G16B16A16F;
                    case StridePixelFormat.R32G32_Float: return VLPixelFormat.R32G32F;
                    case StridePixelFormat.R32G32B32A32_Float: return VLPixelFormat.R32G32B32A32F;
                    default:
                        throw new Exception("Unsupported pixel format");
                }
            }
        }

        private TexturePool GetTexturePool(GraphicsDevice graphicsDevice, Texture texture)
        {
            return TexturePool.Get(graphicsDevice, texture.Description.ToStagingDescription())
                .Subscribe(texturePoolSubscription);
        }

        protected override void Destroy()
        {
            while (textureDownloads.Count > 0)
                textureDownloads.Dequeue().texture.Dispose();

            subscriptions.Dispose();

            base.Destroy();
        }
    }
}
