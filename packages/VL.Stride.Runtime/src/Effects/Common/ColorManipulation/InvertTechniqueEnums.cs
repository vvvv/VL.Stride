namespace VL.Stride.Effects.Common
{
    /// <summary>
    /// Operands of the Invert nodes.
    /// </summary>
    public enum Invert
    {
        /// <summary>
        /// Invert RGB.
        /// </summary>
        RGB,

        /// <summary>
        /// Invert Red.
        /// </summary>
        Red,

        /// <summary>
        /// Invert Green.
        /// </summary>
        Green,

        /// <summary>
        /// Invert Blue.
        /// </summary>
        Blue,

        /// <summary>
        /// Invert Red and Green.
        /// </summary>
        RedGreen,

        /// <summary>
        /// Invert Red and Blue.
        /// </summary>
        RedBlue,

        /// <summary>
        /// Invert Green and Blue.
        /// </summary>
        GreenBlue,

        /// <summary>
        /// Invert HSV value.
        /// </summary>
        Value,

        /// <summary>
        /// Invert HSL lightness.
        /// </summary>
        Lightness,

        /// <summary>
        /// Invert HSV saturation.
        /// </summary>
        Saturation,

        /// <summary>
        /// Invert Hue.
        /// </summary>
        Hue,

        /// <summary>
        /// Invert RGB and Shift Hue by 180.
        /// </summary>
        RGBShift,
    };
}
