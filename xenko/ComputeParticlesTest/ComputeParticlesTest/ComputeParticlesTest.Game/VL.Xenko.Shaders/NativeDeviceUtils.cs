using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace VL.Xenko.Shaders
{
    public static class NativeDeviceUtils
    {
        //command list
        static FieldInfo nativeDeviceContextFi;
        static FieldInfo unorderedAccessViewsFi;
        static FieldInfo nativeDeviceChildFi;
        static FieldInfo unorderedAccessViewFi;

        //graphics device
        static FieldInfo nativeDeviceFi;
        static MethodInfo registerBufferMemoryUsageMi;


        static FieldInfo geometryShaderFi;

        //stream out buffer
        static ConstructorInfo bufferCi;
        static FieldInfo bufferDescriptionFi;
        static FieldInfo nativeDescriptionFi;
        static FieldInfo elementCountFi;
        static PropertyInfo viewFlagsPi;
        static PropertyInfo viewFormatPi;
        static MethodInfo initializeViewsMi;

        static NativeDeviceUtils()
        {
            var comandListType = Type.GetType("SiliconStudio.Xenko.Graphics.CommandList, SiliconStudio.Xenko.Graphics");
            var comandListTypeFields = comandListType.GetRuntimeFields();
            nativeDeviceContextFi = comandListTypeFields.Where(fi => fi.Name == "nativeDeviceContext").First();
            unorderedAccessViewsFi = comandListTypeFields.Where(fi => fi.Name == "unorderedAccessViews").First();

            var graphicsResourceBaseType = Type.GetType("SiliconStudio.Xenko.Graphics.GraphicsResourceBase, SiliconStudio.Xenko.Graphics");
            nativeDeviceChildFi = graphicsResourceBaseType.GetFieldWithName("nativeDeviceChild");

            var graphicsResourceType = Type.GetType("SiliconStudio.Xenko.Graphics.GraphicsResource, SiliconStudio.Xenko.Graphics");
            unorderedAccessViewFi = graphicsResourceType.GetFieldWithName("unorderedAccessView");

            var pipelineStateType = Type.GetType("SiliconStudio.Xenko.Graphics.PipelineState, SiliconStudio.Xenko.Graphics");
            geometryShaderFi = pipelineStateType.GetFieldWithName("geometryShader");
 
            //graphics device native device
            var graphicsDeviceType = Type.GetType("SiliconStudio.Xenko.Graphics.GraphicsDevice, SiliconStudio.Xenko.Graphics");
            nativeDeviceFi = graphicsDeviceType.GetFieldWithName("nativeDevice");
            registerBufferMemoryUsageMi = graphicsDeviceType.GetmethodWithName("RegisterBufferMemoryUsage");

            //buffer
            var bufferType = Type.GetType("SiliconStudio.Xenko.Graphics.Buffer, SiliconStudio.Xenko.Graphics");
            var bufferTypeInfo = bufferType.GetTypeInfo();
            bufferCi = bufferTypeInfo.DeclaredConstructors.Where(ci => ci.GetParameters().Count() == 1).First();
            bufferDescriptionFi = bufferType.GetFieldWithName("bufferDescription");
            nativeDescriptionFi = bufferType.GetFieldWithName("nativeDescription");
            elementCountFi = bufferType.GetFieldWithName("elementCount");
            viewFlagsPi = bufferType.GetPropertyWithName("ViewFlags");
            viewFormatPi = bufferType.GetPropertyWithName("ViewFormat");
            initializeViewsMi = bufferType.GetmethodWithName("InitializeViews");

        }

        static FieldInfo GetFieldWithName(this Type t, string name)
        {
            return t.GetRuntimeFields().Where(i => i.Name == name).First();
        }

        static PropertyInfo GetPropertyWithName(this Type t, string name)
        {
            return t.GetRuntimeProperties().Where(i => i.Name == name).First();
        }

        static MethodInfo GetmethodWithName(this Type t, string name)
        {
            return t.GetRuntimeMethods().Where(i => i.Name == name).First();
        }

        static T InvokeMethod<T>(this MethodInfo mi, object instance, params object[] parameters)
        {
            return (T)mi.Invoke(instance, parameters);
        }

        static void InvokeMethod(this MethodInfo mi, object instance, params object[] parameters)
        {
            mi.Invoke(instance, parameters);
        }

        public static SharpDX.Direct3D11.DeviceContext GetNativeDeviceContext(this CommandList commandList)
        {
            return (SharpDX.Direct3D11.DeviceContext)nativeDeviceContextFi.GetValue(commandList);
        }

        public static SharpDX.Direct3D11.Device GetNativeDevice(this GraphicsDevice graphicsDevice)
        {
            return (SharpDX.Direct3D11.Device)nativeDeviceFi.GetValue(graphicsDevice);
        }

        public static void ComputeShaderReApplyUnorderedAccessView(this CommandList commandList, int slot, int counterValue)
        {
            var uavs = (SharpDX.Direct3D11.UnorderedAccessView[])unorderedAccessViewsFi.GetValue(commandList);
            commandList.GetNativeDeviceContext().ComputeShader.SetUnorderedAccessView(slot, uavs[slot], counterValue);
        }

        public static void DrawInstancedIndirect(this CommandList commandList, Buffer argsBuffer, int alignedByteOffsetForArgs)
        {
            var buffer = (SharpDX.Direct3D11.Buffer)nativeDeviceChildFi.GetValue(argsBuffer);
            commandList.GetNativeDeviceContext().DrawInstancedIndirect(buffer, alignedByteOffsetForArgs);
        }

        public static CommandList CopyStructureCount(this CommandList commandList, Buffer sourceBuffer, Buffer targetBuffer, int destinationAlignedByteOffset)
        {
            var buffer = (SharpDX.Direct3D11.Buffer)nativeDeviceChildFi.GetValue(targetBuffer);
            var uav = (SharpDX.Direct3D11.UnorderedAccessView)unorderedAccessViewFi.GetValue(sourceBuffer);
            commandList.GetNativeDeviceContext().CopyStructureCount(buffer, destinationAlignedByteOffset, uav);
            return commandList;
        }

        public static CommandList SetStreamOutTarget(this CommandList commandList, Buffer targetBuffer, int offset)
        {
            var buffer = (SharpDX.Direct3D11.Buffer)nativeDeviceChildFi.GetValue(targetBuffer);
            var so = commandList.GetNativeDeviceContext().StreamOutput;
            so.SetTarget(buffer, offset);
            return commandList;
        }

        public static MutablePipelineState ReApplyGeometryStreamOutShader(this MutablePipelineState mutablePipelineState, GraphicsDevice graphicsDevice, EffectInstance geometryEffectInstance, string semanticName)
        {
            var bytecode = geometryEffectInstance.Effect.Bytecode;
            var reflection = bytecode.Reflection;

            var geometryBytecode = bytecode.Stages.First(s => s.Stage == ShaderStage.Geometry);

            // Calculate the strides
            var soStrides = new List<int>();
            foreach (var streamOutputElement in reflection.ShaderStreamOutputDeclarations)
            {
                for (int i = soStrides.Count; i < (streamOutputElement.Stream + 1); i++)
                {
                    soStrides.Add(0);
                }
                soStrides[streamOutputElement.Stream] += streamOutputElement.ComponentCount * sizeof(float);
            }


            SharpDX.Direct3D11.StreamOutputElement[] soElements;
            int[] soStridesArray;

            if (string.IsNullOrWhiteSpace(semanticName))
            {
                soElements = new SharpDX.Direct3D11.StreamOutputElement[]
                {
                    new SharpDX.Direct3D11.StreamOutputElement(0, "SV_Position", 0, 0, 4, 0),
                    new SharpDX.Direct3D11.StreamOutputElement(0, "NORMAL", 0, 0, 4, 0),
                    new SharpDX.Direct3D11.StreamOutputElement(0, "TEXCOORD", 0, 0, 4, 0)
                };

                soStridesArray = new int[] { 16, 16, 16 };
            }
            else
            {
                soElements = new SharpDX.Direct3D11.StreamOutputElement[]
                {
                    new SharpDX.Direct3D11.StreamOutputElement(0, semanticName, 0, 0, 4, 0)
                };

                soStridesArray = soStrides.ToArray();
            }


            var nativeDevice = graphicsDevice.GetNativeDevice();
            var geometryShader = new SharpDX.Direct3D11.GeometryShader(nativeDevice, geometryBytecode, soElements, soStrides.ToArray(), reflection.StreamOutputRasterizedStream);
            geometryShaderFi.SetValue(mutablePipelineState.CurrentState, geometryShader);
            return mutablePipelineState;
        }

        public static Buffer NewStreamOutBuffer(GraphicsDevice graphicsDevice, int sizeInBytes)
        {
            var b = (Buffer)bufferCi.Invoke(new[] { graphicsDevice });

            var bd = new BufferDescription(sizeInBytes, BufferFlags.VertexBuffer, GraphicsResourceUsage.Default);

            var nbd = new SharpDX.Direct3D11.BufferDescription()
            {
                SizeInBytes = bd.SizeInBytes,
                StructureByteStride = 0,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                BindFlags = SharpDX.Direct3D11.BindFlags.VertexBuffer | SharpDX.Direct3D11.BindFlags.StreamOutput,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                Usage = SharpDX.Direct3D11.ResourceUsage.Default
            };

            //set fields
            bufferDescriptionFi.SetValue(b, bd);
            nativeDescriptionFi.SetValue(b, nbd);
            elementCountFi.SetValue(b, 1);

            //set properties
            viewFlagsPi.SetMethod.InvokeMethod(b, bd.BufferFlags);
            viewFormatPi.SetMethod.InvokeMethod(b, PixelFormat.None);

            //set native device child
            var nb = new SharpDX.Direct3D11.Buffer(graphicsDevice.GetNativeDevice(), nbd);
            nativeDeviceChildFi.SetValue(b, nb);

            initializeViewsMi.Invoke(b, new object[0]);

            registerBufferMemoryUsageMi.InvokeMethod(graphicsDevice, b.SizeInBytes);

            return b;
        }
    }
}
