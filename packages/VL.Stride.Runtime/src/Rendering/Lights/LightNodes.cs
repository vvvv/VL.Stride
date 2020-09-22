using Stride.Core.Mathematics;
using Stride.Rendering.Colors;
using Stride.Rendering.Lights;
using System.Collections.Generic;
using VL.Core;

namespace VL.Stride.Rendering.Lights
{
    static partial class LightNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            var baseCategory = "Stride.Lights.Advanced";
            var lightTypesCategory = $"{baseCategory}.LightTypes";
            var shadowMapsCategory = $"{baseCategory}.ShadowMaps";

            yield return NewColorLightNode<LightAmbient>(factory, lightTypesCategory);

            yield return NewEnvironmentLightNode<LightSkybox>(factory, lightTypesCategory)
                .AddInput(nameof(LightSkybox.Skybox), x => x.Skybox, (x, v) => x.Skybox = v);

            yield return NewSkyboxNode(factory, lightTypesCategory);

            yield return NewDirectLightNode<LightDirectional>(factory, lightTypesCategory);

            yield return NewDirectLightNode<LightPoint>(factory, lightTypesCategory)
                .AddInput(nameof(LightPoint.Radius), x => x.Radius, (x, v) => x.Radius = v, 5f);

            yield return NewDirectLightNode<LightSpot>(factory, lightTypesCategory)
                .AddInput(nameof(LightSpot.Range), x => x.Range, (x, v) => x.Range = v, 5f)
                .AddInput(nameof(LightSpot.AngleInner), x => x.AngleInner, (x, v) => x.AngleInner = v, 30f)
                .AddInput(nameof(LightSpot.AngleOuter), x => x.AngleOuter, (x, v) => x.AngleOuter = v, 35f)
                .AddInput(nameof(LightSpot.ProjectiveTexture), x => x.ProjectiveTexture, (x, v) => x.ProjectiveTexture = v)
                .AddInput(nameof(LightSpot.UVScale), x => x.UVScale, (x, v) => x.UVScale = v, Vector2.One)
                .AddInput(nameof(LightSpot.UVOffset), x => x.UVOffset, (x, v) => x.UVOffset = v, Vector2.Zero)
                .AddInput(nameof(LightSpot.MipMapScale), x => x.MipMapScale, (x, v) => x.MipMapScale = v, 0f)
                .AddInput(nameof(LightSpot.AspectRatio), x => x.AspectRatio, (x, v) => x.AspectRatio = v, 1f)
                .AddInput(nameof(LightSpot.TransitionArea), x => x.TransitionArea, (x, v) => x.TransitionArea = v, 0.2f)
                .AddInput(nameof(LightSpot.ProjectionPlaneDistance), x => x.ProjectionPlaneDistance, (x, v) => x.ProjectionPlaneDistance = v, 1f)
                .AddInput(nameof(LightSpot.FlipMode), x => x.FlipMode, (x, v) => x.FlipMode = v);

            yield return NewShadowNode<LightDirectionalShadowMap>(factory, shadowMapsCategory)
                .AddInput(nameof(LightDirectionalShadowMap.CascadeCount), x => x.CascadeCount, (x, v) => x.CascadeCount = v, LightShadowMapCascadeCount.FourCascades)
                .AddInput(nameof(LightDirectionalShadowMap.DepthRange), x => x.DepthRange, (x, v) =>
                {
                    var s = x.DepthRange;
                    s.IsAutomatic = v.IsAutomatic;
                    s.ManualMinDistance = v.ManualMinDistance;
                    s.ManualMaxDistance = v.ManualMaxDistance;
                    s.IsBlendingCascades = v.IsBlendingCascades;
                })
                .AddInput(nameof(LightDirectionalShadowMap.PartitionMode), x => x.PartitionMode, (x, v) => x.PartitionMode = v)
                .AddInput(nameof(LightDirectionalShadowMap.StabilizationMode), x => x.StabilizationMode, (x, v) => x.StabilizationMode = v, LightShadowMapStabilizationMode.ProjectionSnapping)
                .AddDefaultPins()
                .AddEnabledPin();

            yield return NewShadowNode<LightPointShadowMap>(factory, shadowMapsCategory)
                .AddInput(nameof(LightPointShadowMap.Type), x => x.Type, (x, v) => x.Type = v, LightPointShadowMapType.CubeMap)
                .AddDefaultPins()
                .AddEnabledPin();

            yield return NewShadowNode<LightStandardShadowMap>(factory, shadowMapsCategory)
                .AddDefaultPins()
                .AddEnabledPin();

            yield return factory.NewNode<LightShadowMapFilterTypePcf>(category: shadowMapsCategory, copyOnWrite: false)
                .AddInput(nameof(LightShadowMapFilterTypePcf.FilterSize), x => x.FilterSize, (x, v) => x.FilterSize = v, LightShadowMapFilterTypePcfSize.Filter5x5);

            yield return factory.NewNode<LightDirectionalShadowMap.DepthRangeParameters>(category: shadowMapsCategory)
                .AddInput(nameof(LightDirectionalShadowMap.DepthRangeParameters.IsAutomatic), x => x.IsAutomatic, (x, v) => x.IsAutomatic = v, true)
                .AddInput(nameof(LightDirectionalShadowMap.DepthRangeParameters.ManualMinDistance), x => x.ManualMinDistance, (x, v) => x.ManualMinDistance = v, LightDirectionalShadowMap.DepthRangeParameters.DefaultMinDistance)
                .AddInput(nameof(LightDirectionalShadowMap.DepthRangeParameters.ManualMaxDistance), x => x.ManualMaxDistance, (x, v) => x.ManualMaxDistance = v, LightDirectionalShadowMap.DepthRangeParameters.DefaultMaxDistance)
                .AddInput(nameof(LightDirectionalShadowMap.DepthRangeParameters.IsBlendingCascades), x => x.IsBlendingCascades, (x, v) => x.IsBlendingCascades = v, true);

            yield return factory.NewNode<LightDirectionalShadowMap.PartitionManual>(category: shadowMapsCategory)
                .AddInput(nameof(LightDirectionalShadowMap.PartitionManual.SplitDistance0), x => x.SplitDistance0, (x, v) => x.SplitDistance0 = v, 0.05f)
                .AddInput(nameof(LightDirectionalShadowMap.PartitionManual.SplitDistance1), x => x.SplitDistance1, (x, v) => x.SplitDistance1 = v, 0.15f)
                .AddInput(nameof(LightDirectionalShadowMap.PartitionManual.SplitDistance2), x => x.SplitDistance2, (x, v) => x.SplitDistance2 = v, 0.50f)
                .AddInput(nameof(LightDirectionalShadowMap.PartitionManual.SplitDistance3), x => x.SplitDistance3, (x, v) => x.SplitDistance3 = v, 1.00f);

            yield return factory.NewNode<LightDirectionalShadowMap.PartitionLogarithmic>(category: shadowMapsCategory)
                .AddInput(nameof(LightDirectionalShadowMap.PartitionLogarithmic.PSSMFactor), x => x.PSSMFactor, (x, v) => x.PSSMFactor = v, 0.5f);
        }

        static CustomNodeDesc<TLight> NewEnvironmentLightNode<TLight>(IVLNodeDescriptionFactory factory, string category)
            where TLight : IEnvironmentLight, new()
        {
            return factory.NewNode<TLight>(category: category, copyOnWrite: false, fragmented: true);
        }

        static CustomNodeDesc<TLight> NewColorLightNode<TLight>(IVLNodeDescriptionFactory factory, string category)
            where TLight : IColorLight, new()
        {
            return factory.NewNode<TLight>(category: category, copyOnWrite: false, fragmented: true)
                .AddInput(nameof(IColorLight.Color), x => new Color4(x.Color.ComputeColor()), (x, v) => ((ColorRgbProvider)x.Color).Value = v.ToColor3(), Color4.White);
        }

        static CustomNodeDesc<TLight> NewDirectLightNode<TLight>(IVLNodeDescriptionFactory factory, string category)
            where TLight : IColorLight, IDirectLight, new()
        {
            return NewColorLightNode<TLight>(factory, category)
                .AddInput(
                    name: nameof(IDirectLight.Shadow),
                    getter: x => x.Shadow, 
                    setter: (x, v) =>
                    {
                        var s = x.Shadow;
                        if (v != null)
                        {
                            s.Enabled = v.Enabled;
                            s.Filter = v.Filter;
                            s.Size = v.Size;
                            s.BiasParameters.DepthBias = v.BiasParameters.DepthBias;
                            s.BiasParameters.NormalOffsetScale = v.BiasParameters.NormalOffsetScale;
                            s.Debug = v.Debug;
                        }
                        else
                        {
                            s.Enabled = false;
                        }
                    },
                    defaultValue: null /* null disables the shadow */);
        }

        static CustomNodeDesc<TShadow> NewShadowNode<TShadow>(IVLNodeDescriptionFactory factory, string category)
            where TShadow : LightShadowMap, new()
        {
            return factory.NewNode<TShadow>(category: category, copyOnWrite: true /* In order to detect changes */, fragmented: true);
        }

        static CustomNodeDesc<TShadow> AddDefaultPins<TShadow>(this CustomNodeDesc<TShadow> node)
            where TShadow : LightShadowMap, new()
        {
            return node
                .AddInput(nameof(LightShadowMap.Filter), x => x.Filter, (x, v) => x.Filter = v)
                // Larger sizes can crash Stride when multiple lights / light shafts are used - one can find bunch of TODOs in ShadowMapRenderer regarding texture atlas limits
                .AddInput(nameof(LightShadowMap.Size), x => x.Size, (x, v) => x.Size = v, LightShadowMapSize.Medium)
                .AddInput(nameof(LightShadowMap.BiasParameters.DepthBias), x => x.BiasParameters.DepthBias, (x, v) => x.BiasParameters.DepthBias = v, 0.01f)
                .AddInput(nameof(LightShadowMap.BiasParameters.NormalOffsetScale), x => x.BiasParameters.NormalOffsetScale, (x, v) => x.BiasParameters.NormalOffsetScale = v, 10f)
                .AddInput(nameof(LightShadowMap.Debug), x => x.Debug, (x, v) => x.Debug = v);
        }

        static CustomNodeDesc<TShadow> AddEnabledPin<TShadow>(this CustomNodeDesc<TShadow> node)
            where TShadow : LightShadowMap, new()
        {
            return node.AddInput(nameof(LightShadowMap.Enabled), x => x.Enabled, (x, v) => x.Enabled = v, true);
        }

        static IVLNodeDescription NewSkyboxNode(IVLNodeDescriptionFactory factory, string category)
        {
            return factory.NewNode<SkyboxDescription>(
                name: "Skybox",
                category: category,
                copyOnWrite: false,
                hasStateOutput: false,
                fragmented: true)
                .AddInput(nameof(SkyboxDescription.CubeMap), x => x.CubeMap, (x, v) => x.CubeMap = v)
                .AddInput(nameof(SkyboxDescription.IsSpecularOnly), x => x.IsSpecularOnly, (x, v) => x.IsSpecularOnly = v)
                .AddInput(nameof(SkyboxDescription.DiffuseSHOrder), x => x.DiffuseSHOrder, (x, v) => x.DiffuseSHOrder = v, SkyboxPreFilteringDiffuseOrder.Order3)
                .AddInput(nameof(SkyboxDescription.SpecularCubeMapSize), x => x.SpecularCubeMapSize, (x, v) => x.SpecularCubeMapSize = v, 256)
                .AddCachedOutput("Output", (nodeContext, x) =>
                {
                    using (var context = new SkyboxGeneratorContext(nodeContext))
                    {
                        return GenerateSkybox(x, context);
                    }
                });
        }
    }
}
