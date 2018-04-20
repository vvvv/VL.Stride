
// =============================================================================
// ATTRACTORS HEADER ===========================================================
// =============================================================================

bool AirCSVariables__Attractor_Enable = 0;
uint AirCSVariables__Attractor_Count = 0;
StructuredBuffer<float3> AirCSVariables__Attractor_Pos; // default = float3(0,0,0)
StructuredBuffer<float2> AirCSVariables__Attractor_RadiusStrength; // default = float2(600, 0.01)
StructuredBuffer<float> AirCSVariables__Attractor_GaussianOffset; // default = 50
StructuredBuffer<float3> AirCSVariables__Attractor_ForceRotation; // default = float3(0,0,0)
float AirCSVariables__Attractor_RandomRotationAmount = 0;
float AirCSVariables__Attractor_Gamma = 3;
float4x4 rotateVVVV(float rotX, float rotY, float rotZ)
{
	 float sx = sin(rotX);
	 float cx = cos(rotX);
	 float sy = sin(rotY);
	 float cy = cos(rotY);
	 float sz = sin(rotZ);
	 float cz = cos(rotZ);
	
   return float4x4( cz * cy + sz * sx * sy, sz * cx, cz * -sy + sz * sx * cy , 0,
                   -sz * cy + cz * sx * sy, cz * cx, sz *  sy + cz * sx * cy , 0,
                    cx * sy				  ,-sx     , cx * cy                 , 0,
                    0                     , 0      , 0                       , 1);
}

//Multiple attractors
float3 Attractor(float3 p, float3 Rnd, float3 RndDir)
{
	float3 vec = 0;
	for(uint i=0 ; i<AirCSVariables__Attractor_Count; i++)
	{
		float3 TrgPos = AirCSVariables__Attractor_Pos[i] + 
						RndDir * AirCSVariables__Attractor_GaussianOffset[i];
		float3 attrVec = TrgPos - p;
		float3 Rot = AirCSVariables__Attractor_ForceRotation[i] + (Rnd-0.5)*AirCSVariables__Attractor_RandomRotationAmount;
		Rot *= 6.283185307179;
		attrVec = mul(float4(attrVec,1), rotateVVVV(Rot.x, Rot.y, Rot.z)).xyz;

		float attrRadius = AirCSVariables__Attractor_RadiusStrength[i].x;
		float attrStrength = AirCSVariables__Attractor_RadiusStrength[i].y;

		float attrForce = length(attrVec) / attrRadius;
		attrForce = 1 - attrForce;
		attrForce = saturate(attrForce);
		attrForce = pow(attrForce, AirCSVariables__Attractor_Gamma);
		attrVec = attrVec * attrForce * attrStrength;
		//transform attraction vector:
	//	attrVec = mul(float4(attrVec,1), AirCSVariables__Attractor__VectorTransform).xyz;
		vec += attrVec;
	}
	return vec;
}