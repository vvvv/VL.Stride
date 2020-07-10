using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Rendering.Compositing;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using VL.Core;
using VL.Lang.PublicAPI;
using VL.Lib.Basics.Resources;
using VL.Stride.Games;
using Vortice.Vulkan;

namespace VL.Stride
{
    public class Renderer : IDisposable
    {
        private readonly NodeContext FNodeContext;
        private readonly IResourceHandle<Game> FGameHandle;
        private readonly IResourceHandle<GameWindow> FWindowHandle;
        private readonly SceneSystem FSceneSystem;
        private RectangleF FBounds = RectangleF.Empty;
        private readonly bool FSaveBounds;
        private readonly bool FBoundToDocument;
        private readonly bool FShowDialogIfDocumentChanged;
        private readonly SerialDisposable sizeChangedSubscription = new SerialDisposable();
        private Int2 FLastPosition;

        string FName;
        public string Name
        {
            get => FName;
            set
            {
                if (value != FName)
                {
                    FName = value;
                    ComputeTitle();
                }
            }
        }

        bool FEnabled;
        public bool Enabled 
        {
            get => FEnabled;
            set
            {
                if (value != FEnabled)
                {
                    FEnabled = value;
                    ComputeTitle();
                }
            }
        }

        public SceneInstance FSceneInstance;
        public SceneInstance SceneInstance
        {
            get => FSceneInstance;
            set
            {
                if (value != FSceneInstance)
                {
                    FSceneInstance = value;
                    ComputeTitle();
                }
            }
        }

        public GraphicsCompositor FGraphicsCompositor;
        public GraphicsCompositor GraphicsCompositor
        {
            get => FGraphicsCompositor;
            set
            {
                if (value != FGraphicsCompositor)
                {
                    FGraphicsCompositor = value;
                    ComputeTitle();
                }
            }
        }


        void ComputeTitle()
        {
            var s = new StringBuilder(Name);
            if (!Enabled)
                s.Append(" [Disabled]");
            else 
            {
                if (SceneInstance == null)
                    s.Append(" [No Scene Instance]");
                if (GraphicsCompositor?.Name != null && GraphicsCompositor?.Name != "GraphicsCompositor")
                    s.Append($" [{GraphicsCompositor.Name}]");
                if (GraphicsCompositor == null)
                    s.Append($" [No Graphics Compositor]");
            }

            Window.Title = s.ToString();
        }


        public Renderer(NodeContext nodeContext, RectangleF bounds, bool saveBounds = true, bool boundToDocument = false, bool dialogIfDocumentChanged = false)
        {
            FNodeContext = nodeContext;
            FBounds = bounds;
            FSaveBounds = saveBounds;
            FBoundToDocument = boundToDocument;
            FShowDialogIfDocumentChanged = dialogIfDocumentChanged;

            FGameHandle = nodeContext.GetGameHandle();
            FWindowHandle = nodeContext.GetGameWindowProvider().GetHandle();

            var game = FGameHandle.Resource;
            FSceneSystem = new SceneSystem(game.Services);
            game.GameSystems.Add(FSceneSystem);

            if (bounds.Width > 1 && bounds.Height > 1)
            {
                game.GraphicsDeviceManager.PreferredBackBufferWidth = (int)bounds.Width;
                game.GraphicsDeviceManager.PreferredBackBufferHeight = (int)bounds.Height;
                game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
                game.GraphicsDeviceManager.ApplyChanges();

                Window.Position = new Int2((int)bounds.X, (int)bounds.Y);
            }

            SetupEvents(Window);

            FName = "Stride";
            Enabled = true;
        }

        public GameWindow Window => FWindowHandle.Resource;

        public void Update(bool verticalSync = false)
        {
            var game = (VLGame)FGameHandle.Resource;

            //v-sync
            if (verticalSync != game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace)
            {
                game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = verticalSync;
                game.GraphicsDeviceManager.ApplyChanges();
            }

            //write bounds to patch?
            if (Window.Position != FLastPosition)
            {
                UpdateBounds(null);
                FLastPosition = Window.Position;
            }

            if (Enabled)
            {
                FSceneSystem.GraphicsCompositor = GraphicsCompositor;
                FSceneSystem.SceneInstance = SceneInstance;
            }
            else
            {
                FSceneSystem.GraphicsCompositor = default;
                FSceneSystem.SceneInstance = default;
            }
        }

        void SetupEvents(GameWindow window)
        {
            //register events handlers
            sizeChangedSubscription.Disposable = Observable.Merge(
                Observable.FromEventPattern(window, nameof(Window.ClientSizeChanged)), 
                Observable.FromEventPattern(window, nameof(Window.OrientationChanged)))
                .Throttle(TimeSpan.FromSeconds(0.5))
                .Subscribe(UpdateBounds);

            window.Closing += Window_Closing;
        }

        private void UpdateBounds(EventPattern<object> obj)
        {
            //write bounds into pin
            if (FSaveBounds)
            {
                var b = Window.ClientBounds;
                var p = Window.Position;
                FBounds = new RectangleF(p.X, p.Y, b.Width, b.Height);
                var solution = VL.Model.VLSession.Instance.CurrentSolution as ISolution;
                solution = solution?.SetPinValue(FNodeContext.Path.Stack, "Bounds", FBounds);
                solution?.Confirm(Model.SolutionUpdateKind.DontCompile); 
            }
        }
        private void Window_Closing(object sender, EventArgs e)
        {
            //close doument, if requested
            if (FBoundToDocument)
                Session.CloseDocumentOfNode(FNodeContext.Path.Stack.Peek(), FShowDialogIfDocumentChanged);

            Window.Closing -= Window_Closing;
        }

        public void Dispose()
        {
            // Remove our scene system from the game
            var game = FGameHandle.Resource;
            game.GameSystems.Remove(FSceneSystem);
            // Clear entity manager and compositor so they don't get disposed by the scene system
            FSceneSystem.SceneInstance = null;
            FSceneSystem.GraphicsCompositor = null;
            FSceneSystem.Dispose();

            FWindowHandle?.Dispose();
            FGameHandle?.Dispose();
        }
    }
}
