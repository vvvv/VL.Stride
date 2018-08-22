using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Rendering;
using Xenko.Rendering.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XenkoModel = global::Xenko.Rendering.Model;

namespace VL.Xenko.Utils
{
    public static class ModelUtils
    {
        public static ModelComponent GetOrCreateModelComponent(Entity entity)
        {
            return entity.GetOrCreate<ModelComponent>();
        }

        public static void SetColor(XenkoModel model, Color4 color, float metalness)
        {
            var m = model.Materials.FirstOrDefault()?.Material.Passes.FirstOrDefault();

            if (m != null)
            {
                m.Parameters.Set(MaterialKeys.DiffuseValue, color);
                m.Parameters.Set(MaterialKeys.MetalnessValue, metalness);
            }

        }
    }
}
