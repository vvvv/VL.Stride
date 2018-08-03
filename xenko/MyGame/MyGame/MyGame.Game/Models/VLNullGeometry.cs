using System;

using Xenko.Core;
using Xenko.Rendering;
using Xenko.Graphics;
using Buffer = Xenko.Graphics.Buffer;
using Xenko.Core.Mathematics;

namespace MyGame
{
    /// <summary>
    /// A geometric primitive used to draw a simple model built from a set of vertices and indices.
    /// </summary>
    public class VLNullGeometry : ComponentBase 
    {
        /// <summary>
        /// The pipeline state.
        /// </summary>
        public readonly MutablePipelineState PipelineState;

        /// <summary>
        /// The vertex buffer used by this geometric primitive.
        /// </summary>
        public readonly Buffer VertexBuffer;

        /// <summary>
        /// The index buffer used by this geometric primitive.
        /// </summary>
        public readonly Buffer IndexBuffer;

        /// <summary>
        /// The default graphics device.
        /// </summary>
        protected readonly GraphicsDevice GraphicsDevice;

        /// <summary>
        /// The input layout used by this geometric primitive (shared for all geometric primitive).
        /// </summary>
        private readonly VertexBufferBinding VertexBufferBinding;

        /// <summary>
        /// True if the index buffer is a 32 bit index buffer.
        /// </summary>
        public readonly bool IsIndex32Bits;

        int VertexCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometricPrimitive{T}"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="geometryMesh">The geometry mesh.</param>
        /// <exception cref="System.InvalidOperationException">Cannot generate more than 65535 indices on feature level HW <= 9.3</exception>
        public VLNullGeometry(GraphicsDevice graphicsDevice, int vertexCount)
        {
            GraphicsDevice = graphicsDevice;
            PipelineState = new MutablePipelineState(graphicsDevice);
            VertexCount = vertexCount;

            var nullVertex = new VLNullVertex();
            var vertices = new VLNullVertex[] { nullVertex };

            VertexBuffer = Buffer.Vertex.New(graphicsDevice, new Vector4[1]);


            // For now it will keep buffers for recreation.
            // TODO: A better alternative would be to store recreation parameters so that we can reuse procedural code.
            VertexBufferBinding = new VertexBufferBinding();

            if (vertexCount < 0xFFFF)
            {
                var indicesShort = new ushort[1];
                IndexBuffer = Buffer.Index.New(graphicsDevice, 1).RecreateWith(indicesShort).DisposeBy(this);
            }
            else
            {
                var indices = new int[1];
                if (graphicsDevice.Features.CurrentProfile <= GraphicsProfile.Level_9_3)
                {
                    throw new InvalidOperationException("Cannot generate more than 65535 indices on feature level HW <= 9.3");
                }

                IndexBuffer = Buffer.Index.New(graphicsDevice, 1).RecreateWith(indices).DisposeBy(this);
                IsIndex32Bits = true;
            }

            PipelineState.State.SetDefaults();
            PipelineState.State.PrimitiveType = PrimitiveType.PointList;
        }

        /// <summary>
        /// Draws this <see cref="GeometricPrimitive" />.
        /// </summary>
        /// <param name="commandList">The command list.</param>
        public void Draw(GraphicsContext graphicsContext, EffectInstance effectInstance)
        {
            var commandList = graphicsContext.CommandList;

            // Update pipeline state
            PipelineState.State.RootSignature = effectInstance.RootSignature;
            PipelineState.State.EffectBytecode = effectInstance.Effect.Bytecode;
            PipelineState.State.Output.CaptureState(commandList);
            PipelineState.Update();
            commandList.SetPipelineState(PipelineState.CurrentState);

            effectInstance.Apply(graphicsContext);

            commandList.SetVertexBuffer(0, VertexBuffer, 0, 0);

            // Finally Draw this mesh
            commandList.Draw(VertexCount);
        }

        public MeshDraw ToMeshDraw()
        {
            var indexBufferBinding = new IndexBufferBinding(IndexBuffer, IsIndex32Bits, 0);

            var data = new MeshDraw
            {
                StartLocation = 0,
                PrimitiveType = PrimitiveType.PointList,
                VertexBuffers = new[] { VertexBufferBinding },
                IndexBuffer = indexBufferBinding,
                DrawCount = VertexBufferBinding.Count
            };

            return data;
        }
    }
}