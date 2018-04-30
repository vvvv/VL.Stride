using SiliconStudio.Xenko.Graphics;
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
        static FieldInfo nativeDeviceContextFi;
        static FieldInfo unorderedAccessViewsFi;
        static FieldInfo nativeDeviceChildFi;
        static FieldInfo unorderedAccessViewFi;

        static NativeDeviceUtils()
        {
            var comandListType = Type.GetType("SiliconStudio.Xenko.Graphics.CommandList, SiliconStudio.Xenko.Graphics");
            var comandListTypeFields = comandListType.GetRuntimeFields();
            nativeDeviceContextFi = comandListTypeFields.Where(fi => fi.Name == "nativeDeviceContext").First();
            unorderedAccessViewsFi = comandListTypeFields.Where(fi => fi.Name == "unorderedAccessViews").First();

            var graphicsResourceBaseType = Type.GetType("SiliconStudio.Xenko.Graphics.GraphicsResourceBase, SiliconStudio.Xenko.Graphics");
            nativeDeviceChildFi = graphicsResourceBaseType.GetRuntimeFields().Where(fi => fi.Name == "nativeDeviceChild").First();

            var graphicsResourceType = Type.GetType("SiliconStudio.Xenko.Graphics.GraphicsResource, SiliconStudio.Xenko.Graphics");
            unorderedAccessViewFi = graphicsResourceType.GetRuntimeFields().Where(fi => fi.Name == "unorderedAccessView").First();
        }

        public static SharpDX.Direct3D11.DeviceContext GetNativeDeviceContext(this CommandList commandList)
        {
            return (SharpDX.Direct3D11.DeviceContext)nativeDeviceContextFi.GetValue(commandList);
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
    }
}
