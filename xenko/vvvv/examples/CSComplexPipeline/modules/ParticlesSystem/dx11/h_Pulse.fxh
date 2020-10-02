
// =============================================================================
// PULSE HEADER ================================================================
// =============================================================================

bool AirCSVariables__Pulse_Enable = 0;
uint AirCSVariables__Pulse_Count = 0;
StructuredBuffer<float3> AirCSVariables__Pulse_Pos;	// default = float3(0,0,0)
StructuredBuffer<float3> AirCSVariables__Pulse_RadiusGammaStrength;	//default = float3(100, 2, 0)
float AirCSVariables__Pulse_DistOffset = 0;
float AirCSVariables__Pulse_SetColorAmount = 0;
float AirCSVariables__Pulse_ColorGradientGamma = 10;
float3 AirCSVariables__Pulse_Color = float3(1,0,0);
float AirCSVariables__Pulse_Heat = 0;

Texture2D AirCSTextures_PulseTexture;
SamplerState AirCSTextures_PulseSampler : IMMUTABLE
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

