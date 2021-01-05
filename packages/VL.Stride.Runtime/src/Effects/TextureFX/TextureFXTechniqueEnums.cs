using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Stride.Effects.TextureFX
{
   
    public enum BumpTechnique { Directional, Point };
    public enum ChannelKeyingTechnique { Alpha, Red, Green, Blue, Luma, Saturation };
    public enum ColorConvertTechnique { Alpha, Red, Green, Blue, Hue, Saturation, Value, HSVtoRGB, RGBtoHSV };
    public enum ColorMapTechnique { Hue, HueSaturation, HueValue, Luma, RedBlue, RGBA, SaturationValue, Tone, Value };
    public enum ColorRampTechnique { RGB, Hue, Luma, Saturation };
    public enum LevelsClampType { None, Top, Bottom, Both };
    public enum TunnelsDistortionTechnique { Square, Cylinder, Fly };
    public enum GlowTechnique { Pre, Glow, Mix };
    public enum AlphaChannel { Average, R, G, B, A };
    public enum NoiseType { Perlin, PerlinGrad, Value, ValueGrad, Simplex, SimplexGrad, WorleyFast, WorleyFastGrad };
    public enum BlendTechnique { Normal, Add, Subtract, Screen, Multiply, Darken, Lighten, Difference, Exclusion, Overlay, Hardlight, Softlight, Dodge, Burn, Reflect, Glow, Freeze, Heat, Divide }; 
}
