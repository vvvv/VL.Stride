namespace VL.Stride.Effects.Common
{
    /// <summary>
    /// Operands of the ColorSpaceConversion nodes.
    /// </summary>
    public enum ColorSpaceConversion
    {
        SRgbToLinear,
        SRgbToOklab,
        SRgbToGamma,

        GammaToLinear,
        GammaToSRgb,
        GammaToOklab,

        LinearToSRgb,
        LinearToGamma,
        LinearToOklab,

        OklabToLinear,
        OklabToSRgb,
        OklabToGamma,
    };
}
