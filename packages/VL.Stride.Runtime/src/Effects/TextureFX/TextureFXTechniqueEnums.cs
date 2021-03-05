using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Stride.Effects.TextureFX
{
    public enum AlphaChannel { Average, R, G, B, A };
    public enum BlendTechnique { Normal, Add, Subtract, Screen, Multiply, Darken, Lighten, Difference, Exclusion, Overlay, Hardlight, Softlight, Dodge, Burn, Reflect, Glow, Freeze, Heat, Divide };
    public enum ColorRampTechnique { RGB, Hue, Luma, Saturation };
    public enum GlowTechnique { Pre, Glow, Mix };
    public enum LevelsClampType { None, Top, Bottom, Both };
    public enum NoiseType { Perlin, PerlinGrad, Value, ValueGrad, Simplex, SimplexGrad, WorleyFast, WorleyFastGrad };
    public enum SwizzleTechnique { Red, Green, Blue, Alpha };
    public enum TunnelsDistortionTechnique { Square, Cylinder, Fly };

   

    
}
