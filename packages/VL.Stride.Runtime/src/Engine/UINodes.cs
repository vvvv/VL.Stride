using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.UI;
using Stride.UI.Panels;
using Stride.UI.Controls;
using System;
using System.Collections.Generic;
using VL.Core;
using VL.Lib.Basics.Resources;
using Stride.Graphics;
using Stride.Rendering.Sprites;

namespace VL.Stride.Engine
{
    static class UINodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory nodeFactory)
        {
            string uiCategory = "Stride.Experimental.UI";
            string panelsCategory = $"{uiCategory}.Panels";
            string controlsCategory = $"{uiCategory}.Controls";

            yield return NewUINode<UIComponent>(nodeFactory, uiCategory);
            yield return NewUINode<UIPage>(nodeFactory, uiCategory);

            // panels
            yield return NewUINode<Grid>(nodeFactory, panelsCategory);
            yield return NewUINode<UniformGrid>(nodeFactory, panelsCategory);
            yield return NewUINode<StackPanel>(nodeFactory, panelsCategory);

            // controls
            yield return NewUINode<Button>(nodeFactory, controlsCategory);
            yield return NewUINode<ToggleButton>(nodeFactory, controlsCategory);
            yield return NewUINode<ImageElement>(nodeFactory, controlsCategory);
            yield return NewUINode<EditText>(nodeFactory, controlsCategory);
            yield return NewUINode<TextBlock>(nodeFactory, controlsCategory);

            yield return NewUINode<SpriteSheet>(nodeFactory, controlsCategory);
            yield return NewUINode<SpriteFromSheet>(nodeFactory, controlsCategory);
            yield return NewUINode<SpriteFromTexture>(nodeFactory, controlsCategory);


        }

        static StrideNodeDesc<T> NewUINode<T>(this IVLNodeDescriptionFactory nodeFactory, string category)
            where T : new()
        {
            return new StrideNodeDesc<T>(nodeFactory, null, category);
        }

    }
}
