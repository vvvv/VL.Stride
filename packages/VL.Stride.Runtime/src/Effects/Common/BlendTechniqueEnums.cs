namespace VL.Stride.Effects.Common
{
    /// <summary>
    /// Operands of the Blend node.
    /// </summary>
    public enum BlendMode
    { 
            //MIX
            Normal,
            Average,
            GeometricMean,

            //DARKEN
            Darken,
            Multiply,
            ColorBurn,
            LinearBurn,
            DarkerColor,

            //LIGHTEN
            Lighten,
            Screen,
            Add,
            LighterColor,
            Glow,

            //CONTRAST
            Overlay,
            Softlight,
            ColorDodge,
            Reflect,
            HardMix,
            Freeze,
            Pinlight,
            Hardlight,
            VividLight,
            LinearLight,



            //INVERSION
            Difference,
            Exclusion,
            Subtract,
            Divide,
            Negation,
            AdditiveSubstractive,
            Heat,
            Phoenix,
            GrainExtract,
            GrainMerge,


            //COMPONENT
            Hue,
            Saturation,
            Color,
            Lightness,
            Value,
            Red,
            Green,
            Blue,
    };
}
