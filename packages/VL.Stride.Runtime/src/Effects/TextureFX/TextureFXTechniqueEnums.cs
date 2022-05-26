﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Stride.Effects.TextureFX
{

    public enum AlphaChannel { Average, R, G, B, A };
    public enum BumpType { Directional, Point };
    public enum ChannelKeyingType { Alpha, Red, Green, Blue, Luma, Saturation };
    public enum ConvertColorType { HSVtoRGB, HSLtoRGB, RGBtoHSV, RGBtoHSL };
    public enum MapColorType { Luma, Hue, Saturation, Value, HueSaturation, HueValue, SaturationValue, Tone, RedBlue, RGBA };
    public enum GlowType { Pre, Glow, Mix };
    public enum HaloType { Smooth, Linear };
    public enum LevelsClampType { None, Top, Bottom, Both };
    public enum LomographType { One, Two, Three, Four, Five, Six, Gray, Sepia };
    public enum NoiseType { Perlin, PerlinGrad, Value, ValueGrad, Simplex, SimplexGrad, WorleyFast, WorleyFastGrad };
    public enum PaletteType { HSL, HSV, Radial };
    public enum CoordinatesType { Cartesian, Polar };
    public enum TunnelType { Square, Cylinder, Fly };
    public enum ResizeInterpolationType { NearestNeighbor, Linear, CubicBSpline, CubicCatmullRom/*, Lanczos*/ };
    public enum RoundingType { Round, Floor, Ceil };
    public enum TextureChannel { Channel0, Channel1, Channel2, Channel3 };
    public enum BlurPasses { OnePass, TwoPasses, ThreePasses };
    public enum MorphologyType { Rectangle, Diamond, Circle };
}
