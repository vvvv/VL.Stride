#nullable enable
using Stride.Graphics;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection.Metadata;
using VL.Core;
using VL.Lib.Basics.Imaging;
using VL.Lib.Basics.Resources;

namespace VL.Stride.ImageStream
{
    public sealed class ImageStreamToTexture : IDisposable
    {
        private readonly SerialDisposable imageStreamSubscription = new SerialDisposable();
        private readonly SerialDisposable latestSubscription = new SerialDisposable();
        private readonly SerialDisposable currentSubscription = new SerialDisposable();

        private IObservable<IResourceProvider<IImage>>? imageStream;
        private IResourceProvider<Texture>? current, latest;

        private IResourceHandle<GraphicsDevice> graphicsDevice;

        public ImageStreamToTexture()
        {
            graphicsDevice = ServiceRegistry.Current.GetService<IResourceProvider<GraphicsDevice>>().GetHandle();
        }

        public unsafe IObservable<IResourceProvider<IImage>>? ImageStream 
        {
            get => imageStream;
            set
            {
                if (value != imageStream)
                {
                    imageStream = value;

                    imageStreamSubscription.Disposable = value?
                        .Do(provider =>
                        {
                            var textureProvider = StrideUtils.ToTexture(provider, graphicsDevice.Resource).ShareInParallel();
                            var handle = textureProvider.GetHandle(); // Upload the texture

                            // Exchange provider
                            lock (this)
                            {
                                latest = textureProvider;
                                latestSubscription.Disposable = handle;
                            }
                        })
                        .Finally(() =>
                        {
                            lock (this)
                            {
                                latest = null;
                                latestSubscription.Disposable = null;
                            }
                        })
                        .Subscribe();
                }
            }
        }

        public IResourceProvider<Texture> TextureProvider
        {
            get
            {
                lock (this)
                {
                    var latest = this.latest;
                    if (latest != current)
                    {
                        current = latest;
                        currentSubscription.Disposable = current?.GetHandle();
                    }
                    return current ?? ResourceProvider.Default<Texture>.GetInstance(default!);
                }
            }
        }

        public void Dispose()
        {
            imageStreamSubscription.Dispose();
            latestSubscription.Dispose();
            currentSubscription.Dispose();
            graphicsDevice.Dispose();
        }
    }
}
#nullable restore