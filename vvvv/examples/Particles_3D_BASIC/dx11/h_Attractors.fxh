
// =============================================================================
// ATTRACTORS HEADER ===========================================================
// =============================================================================

float4x4 attrForceT;
//Attractors Position Buffer
StructuredBuffer<float3> attrPos;
//Attractors Data Buffer (X = radius, Y = Strength)
StructuredBuffer<float2> attrData;
//AttractorGamma
float attrGamma = 3;

//Multiple attractors
float3 attrVel(float3 p)
{
	float3 vel = 0;
	uint count,dummy;
	attrPos.GetDimensions(count,dummy);
	for(uint i=0 ; i<count; i++)
	{
		//attrVec = p - attrBuffer[i];
		float3 attrVec = attrPos[i] - p;
		float attrRadius = attrData[i].x;
		float attrStrength = attrData[i].y;

		float attrForce = length(attrVec) / attrRadius;
		attrForce = 1 - attrForce;
		attrForce = saturate(attrForce);
		attrForce = pow(attrForce, attrGamma);
		attrVec = attrVec * attrForce * attrStrength;
		//transform attraction vector:
		attrVec = mul(float4(attrVec,1), attrForceT).xyz;
		vel += attrVec;
	}
	return vel;
}