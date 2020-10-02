
// =============================================================================
// FLOW ========================================================================
// =============================================================================

bool AirCSVariables__Flow_Enable = 0;
uint AirCSVariables__Flow_Count = 0;
StructuredBuffer<float3> AirCSVariables__Flow_Pos; // default = float3(0,0,0)
StructuredBuffer<float3> AirCSVariables__Flow_RadiusGamma;  // default = float2(200,2)
StructuredBuffer<float3> AirCSVariables__Flow_Force; // default = float3(0,0,0)

float3 Flow(float3 Pos)
{	
	float3 Force = 0;
	
	for(uint i=0 ; i<AirCSVariables__Flow_Count; i++)
	{
		float3 Vec = AirCSVariables__Flow_Pos[i] - Pos;
		float Radius = AirCSVariables__Flow_RadiusGamma[i].x;

		float gradient = length(Vec) / Radius;
		gradient = 1 - gradient;
		gradient = saturate(gradient);
		gradient = pow(gradient, AirCSVariables__Flow_RadiusGamma[i].y);
		
		Force += gradient * AirCSVariables__Flow_Force[i];
	}
	return Force;
}