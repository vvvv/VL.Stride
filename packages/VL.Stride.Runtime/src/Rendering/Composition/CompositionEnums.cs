using Stride.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace VL.Stride.Rendering.Composition
{
    public enum PredefinedSortMode
    {
        None,
        /// <summary>
        ///  Sort elements according to the pattern: [RenderFeature Sort Key 8 bits] [Distance back to front 32 bits] [RenderObject states 24 bits]
        /// </summary>
        BackToFront,
        /// <summary>
        ///  Sort elements according to the pattern: [RenderFeature Sort Key 8 bits] [Distance front to back 16 bits] [RenderObject states 32 bits]
        /// </summary>
        FrontToBack,
        /// <summary>
        /// Sort elements according to the pattern: [RenderFeature Sort Key 8 bits] RenderObject states 32 bits] [Distance front to back 16 bits]
        /// </summary>
        StateChange
    }

    public static class EnumExtensions
    {
        public static SortMode ToSortMode(this PredefinedSortMode value)
        {
            switch (value)
            {
                case PredefinedSortMode.None:
                    return default;
                case PredefinedSortMode.BackToFront:
                    return new BackToFrontSortMode();
                case PredefinedSortMode.FrontToBack:
                    return new FrontToBackSortMode();
                case PredefinedSortMode.StateChange:
                    return new StateChangeSortMode();
                default:
                    throw new NotImplementedException();
            }
        }

        public static PredefinedSortMode ToPredefinedSortMode(this SortMode value)
        {
            if (value is null)
                return PredefinedSortMode.None;
            if (value is BackToFrontSortMode)
                return PredefinedSortMode.BackToFront;
            if (value is FrontToBackSortMode)
                return PredefinedSortMode.FrontToBack;
            if (value is StateChangeSortMode)
                return PredefinedSortMode.StateChange;
            throw new NotImplementedException();
        }
    }
}
