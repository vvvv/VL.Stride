using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Rendering;

namespace VL.Stride.Rendering
{
    /// <summary>
    /// Extension methods for <see cref="Material"/>.
    /// </summary>
    public static class MaterialExtensions
    {
        /// <summary>
        /// Clone the <see cref="Material"/>.
        /// </summary>
        /// <param name="material">The material to clone.</param>
        /// <returns>The cloned material.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="material"/> is <see langword="null"/>.</exception>
        public static Material Clone(this Material material)
        {
            if (material == null)
            {
                throw new ArgumentNullException(nameof(material));
            }

            var clone = new Material();

            CopyProperties(material, clone);

            return clone;
        }

        internal static void CopyProperties(Material material, Material clone)
        {
            foreach (var pass in material.Passes)
            {
                clone.Passes.Add(new MaterialPass()
                {
                    HasTransparency = pass.HasTransparency,
                    BlendState = pass.BlendState,
                    CullMode = pass.CullMode,
                    IsLightDependent = pass.IsLightDependent,
                    TessellationMethod = pass.TessellationMethod,
                    PassIndex = pass.PassIndex,
                    Parameters = new ParameterCollection(pass.Parameters)
                });
            }
        }
    }
}
