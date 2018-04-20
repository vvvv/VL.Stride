
// =============================================================================
// POINT TARGET HEADER ===========================================================
// =============================================================================

bool AirCSVariables__Brush_Enable = 0;
uint AirCSVariables__Brush_Count = 0;
StructuredBuffer<float3> AirCSVariables__Brush_Pos; // default = float3(0,0,0)
StructuredBuffer<float3> AirCSVariables__Brush_RadiusGammaStrength; // default = float3(100,2,0)
StructuredBuffer<float4> AirCSVariables__Brush_ColorHeat; // default = float4(1,0,0,0)

//Multiple attractors
float4 Brush(float3 Pos, float4 ColorHeat)
{	
	for(uint i=0 ; i<AirCSVariables__Brush_Count; i++)
	{
		float3 Vec = AirCSVariables__Brush_Pos[i] - Pos;
		float Radius = AirCSVariables__Brush_RadiusGammaStrength[i].x;
		float gradient = length(Vec) / Radius;
		float Strength = AirCSVariables__Brush_RadiusGammaStrength[i].z;
		float3 Col = AirCSVariables__Brush_ColorHeat[i].xyz;
		gradient = 1 - gradient;
		gradient = saturate(gradient);
		gradient = pow(gradient, AirCSVariables__Brush_RadiusGammaStrength[i].y);
		ColorHeat.xyz = lerp(ColorHeat.xyz, Col, saturate(gradient * Strength));
		ColorHeat.w += AirCSVariables__Brush_ColorHeat[i].w * gradient;
	}
	return ColorHeat;
}