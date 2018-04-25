// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.ProceduralModels;
using System;
using System.Collections.Generic;

namespace MyGame
{
    [DataContract("NullGeometryProceduralModel")]
    [Display("Null Model", Expand = ExpandRule.Once)]
    [ComponentOrder(10222)]
    public class VLNullModel : IProceduralModel
    {
        public VLNullModel()
        {
            MaterialInstance = new MaterialInstance();
            VertexCount = 1;
        }

        public void SetMaterial(string name, Material material)
        {
            if (name == "Material")
            {
                MaterialInstance.Material = material;
            }
        }

        [DataMember(510)]
        public Vector3 Scale = Vector3.One;

        /// <summary>
        /// Gets or sets the UV scale.
        /// </summary>
        /// <value>The UV scale</value>
        /// <userdoc>The scale to apply to the UV coordinates of the shape. This can be used to tile a texture on it.</userdoc>
        [DataMember(520)]
        [Display("UV Scale")]
        public Vector2 UvScale { get; set; }

        /// <summary>
        /// Gets or sets the local offset that will be applied to the procedural model's vertexes.
        /// </summary>
        [DataMember(530)]
        public Vector3 LocalOffset { get; set; }

        /// <summary>
        /// Gets the material instance.
        /// </summary>
        /// <value>The material instance.</value>
        /// <userdoc>The reference material asset to use with this model.</userdoc>
        [DataMember(600)]
        [NotNull]
        [Display("Material")]
        public MaterialInstance MaterialInstance { get; private set; }

        /// <inheritdoc/>
        [DataMemberIgnore]
        public IEnumerable<KeyValuePair<string, MaterialInstance>> MaterialInstances { get { yield return new KeyValuePair<string, MaterialInstance>("Material", MaterialInstance); } }

        /// <summary>
        /// Gets or sets the vertex count.
        /// </summary>
        /// <value>The size.</value>
        /// <userdoc>The size of the cube along the Ox, Oy and Oz axis.</userdoc>
        [DataMember(10)]
        public int VertexCount { get; set; }

        public void Generate(IServiceRegistry services, Model model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var needsTempDevice = false;
            var graphicsDevice = services?.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;
            if (graphicsDevice == null)
            {
                graphicsDevice = GraphicsDevice.New();
                needsTempDevice = true;
            }

            var boundingBox = BoundingBox.Empty;

            BoundingSphere boundingSphere = BoundingSphere.Empty;

            var geometry = new VLNullGeometry(graphicsDevice, VertexCount);


            var mesh = new Mesh { Draw = geometry.ToMeshDraw(), BoundingBox = boundingBox, BoundingSphere = boundingSphere };

            model.BoundingBox = boundingBox;
            model.BoundingSphere = boundingSphere;
            model.Add(mesh);

            if (MaterialInstance?.Material != null)
            {
                model.Materials.Add(MaterialInstance);
            }

            if (needsTempDevice)
            {
                graphicsDevice.Dispose();
            }
        }
    }
}
