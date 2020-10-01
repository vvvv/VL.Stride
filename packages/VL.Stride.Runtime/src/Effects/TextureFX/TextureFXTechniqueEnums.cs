using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Stride.Effects.TextureFX
{
    public enum ColorRampTechnique { RGB, Hue, Luma, Saturation };
    public enum LevelsClampType { None, Top, Bottom, Both };
    public enum TunnelsDistortionTechnique { Square, Cylinder, Fly };
    public enum GlowTechnique { Pre, Glow, Mix };
    public enum BlendTechnique { Normal, Add, Subtract, Screen, Multiply, Darken, Lighten, Difference, Exclusion, Overlay, Hardlight, Softlight, Dodge, Burn, Reflect, Glow, Freeze, Heat, Divide }; 
}
