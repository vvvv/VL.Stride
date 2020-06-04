using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Serialization;
using Stride.Core.Serialization.Contents;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;

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

        /// <summary>
        /// Same as Material.New loading referenced content in parameter collection (like EnvironmentLightingDFG_LUT)
        /// </summary>
        public static Material New(GraphicsDevice device, MaterialDescriptor descriptor, ContentManager content)
        {
            var m = Material.New(device, descriptor);
            foreach (var pass in m.Passes)
            {
                //var t = pass.Parameters.Get(MaterialSpecularMicrofacetEnvironmentGGXLUTKeys.EnvironmentLightingDFG_LUT);
                //if (t != null)
                //{
                //    var reference = AttachedReferenceManager.GetAttachedReference(t);
                //    var realT = content.Load<Texture>(reference.Url, ContentManagerLoaderSettings.StreamingDisabled);
                //    pass.Parameters.Set(MaterialSpecularMicrofacetEnvironmentGGXLUTKeys.EnvironmentLightingDFG_LUT, realT);
                //}

                foreach (var p in pass.Parameters.ParameterKeyInfos)
                {
                    var key = p.Key;
                    if (key.Type != ParameterKeyType.Object)
                        continue;
                    var value = pass.Parameters.GetObject(key);
                    if (value is null)
                        continue;
                    var reference = AttachedReferenceManager.GetAttachedReference(value);
                    if (reference is null)
                        continue;
                    var c = content.Load(key.PropertyType, reference.Url, ContentManagerLoaderSettings.StreamingDisabled);
                    if (c is null)
                        continue;
                    pass.Parameters.SetObject(key, c);
                }
            }
            return m;
        }
    }
}
