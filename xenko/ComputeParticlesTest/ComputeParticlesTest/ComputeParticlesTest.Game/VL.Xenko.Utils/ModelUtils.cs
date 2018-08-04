using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Rendering;
using Xenko.Rendering.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Xenko.Utils
{
    public static class ModelUtils
    {
        public static Material LoadMaterial(string path)
        {
            return VLHDE.GameInstance.Content.Load<Material>(path);
        }

        public static ModelComponent GetOrCreateModelComponent(Entity entity)
        {
            return entity.GetOrCreate<ModelComponent>();
        }

        public static void SetColor(Model model, Color4 color, float metalness)
        {
            var m = model.Materials.FirstOrDefault()?.Material.Passes.FirstOrDefault();

            if (m != null)
            {
                m.Parameters.Set(MaterialKeys.DiffuseValue, color);
                m.Parameters.Set(MaterialKeys.MetalnessValue, metalness);
            }

        }

        public static Entity GetPrefabInstance(string prefabName)
        {
            // Note that "MyBulletPrefab" refers to the name and location of your prefab asset
            var myBulletPrefab = VLHDE.GameInstance.Content.Load<Prefab>(prefabName);

            // Instantiate a prefab
            var instance = myBulletPrefab.Instantiate();
            return instance[0];
        }
    }
}
