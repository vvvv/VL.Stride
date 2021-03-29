using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Stride.Effects.TextureFX
{
    public enum AlphaChannel { Average, R, G, B, A };
    public enum BlendType { Normal, Add, Subtract, Screen, Multiply, Darken, Lighten, Difference, Exclusion, Overlay, Hardlight, Softlight, Dodge, Burn, Reflect, Glow, Freeze, Heat, Divide };
    public enum BumpType { Directional, Point };
    public enum ChannelKeyingType { Alpha, Red, Green, Blue, Luma, Saturation };
    public enum ConvertColorType { Alpha, Red, Green, Blue, Hue, Saturation, Value, HSVtoRGB, RGBtoHSV };
    public enum MapColorType { Hue, HueSaturation, HueValue, Luma, RedBlue, RGBA, SaturationValue, Tone, Value };
    public enum RampColorType { RGB, Hue, Luma, Saturation };
    public enum GlowType { Pre, Glow, Mix };
    public enum HaloType { Linear, Smooth, Spike, Textured };
    public enum LevelsClampType { None, Top, Bottom, Both };
    public enum LomographType { One, Two, Three, Four, Five, Six, Gray, Sepia };
    public enum NoiseType { Perlin, PerlinGrad, Value, ValueGrad, Simplex, SimplexGrad, WorleyFast, WorleyFastGrad };
    public enum PaletteType { HSL, HSV, Radial };
    public enum CoordinatesType { Cartesian, Polar };
    public enum SwizzleType { Red, Green, Blue, Alpha };
    public enum TunnelType { Square, Cylinder, Fly };
}
