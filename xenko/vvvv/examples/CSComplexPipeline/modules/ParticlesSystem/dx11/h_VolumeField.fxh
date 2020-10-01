
// =============================================================================
// VOLUME FIELD =================================================================
// =============================================================================

bool AirCSVariables__Vol_Enable = 0;
float4x4 AirCSVariables__Vol_tVolInv;
float AirCSVariables__Vol_ForceInfluence = 1;
float AirCSVariables__Vol_HeatInfluence = 1;

Texture3D VolFieldTexture;
SamplerState Vol_sam : Immutable
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = border;
    AddressV = border;
    AddressW = border;
};

//Multiple attractors
float4 VolField(float3 Pos)
{	
	float3 Coord = mul(float4(Pos,1), AirCSVariables__Vol_tVolInv).xyz;
	Coord += 0.5;
	
	return VolFieldTexture.SampleLevel(Vol_sam, Coord, 0);
}