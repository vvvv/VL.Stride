using SiliconStudio.Xenko.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VL.Xenko.Shaders
{
    public static class NativeDeviceUtils
    {
        static FieldInfo nativeDeviceContextFi;

        static FieldInfo NativeDeviceContextFi
        {
            get
            {
                if(nativeDeviceContextFi == null)
                {
                    var type = VLHDE.GameInstance.GraphicsContext.CommandList.GetType();
                    nativeDeviceContextFi = type.GetRuntimeFields().Where(fi => fi.Name == "nativeDeviceContext").First();
                }
                return nativeDeviceContextFi;
            }
        }

        public static SharpDX.Direct3D11.DeviceContext GetNativeDeviceContext(this CommandList commandList)
            => (SharpDX.Direct3D11.DeviceContext)NativeDeviceContextFi.GetValue(commandList);

        public static void DrawInstancedIndirect(CommandList commandList)
        {
            var nativeDeviceContext = GetNativeDeviceContext(commandList);
           // nativeDeviceContext.DrawInstancedIndirect();
        }

        internal static void ComputeShaderSetUnorderedAccessView(this CommandList commandList, int slot, SharpDX.Direct3D11.UnorderedAccessView unorderedAccessView, int counterValue)
        {
            commandList.GetNativeDeviceContext().ComputeShader.SetUnorderedAccessView(slot, unorderedAccessView, counterValue);
        }
    }
}
