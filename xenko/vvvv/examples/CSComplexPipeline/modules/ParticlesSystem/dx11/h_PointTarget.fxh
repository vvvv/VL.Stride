
// =============================================================================
// POINT TARGET HEADER ===========================================================
// =============================================================================

bool AirCSVariables__PointTarget_Enable = 0;
uint AirCSVariables__PointTarget_Count = 0;	
StructuredBuffer<float3> AirCSVariables__PointTarget_Pos; // default = float3(0,0,0)
StructuredBuffer<float> AirCSVariables__PointTarget_Strength; // default = 0
StructuredBuffer<float> AirCSVariables__PointTarget_GaussianOffset; // default = 50

StructuredBuffer<float2> AirCSVariables__PointTarget_MinMaxIndices;	//CONVERT TO UINT2 ON EXPORT!!!!!!!!!!!!!!!!!!!

float4x4 AirCSVariables__PointTarget_VectorTransform = float4x4(1,0,0,0,	0,1,0,0,	0,0,1,0,	0,0,0,1);

// =============================================================================

float3 PointTarget(float3 Pos, uint id, float3 RndDir)
{
	float3 vec = 0;
	
	for(uint i=0 ; i< (uint)AirCSVariables__PointTarget_Count; i++)
	{
		bool indexCheck = 	id > (uint)AirCSVariables__PointTarget_MinMaxIndices[i].x
							&& id < (uint)AirCSVariables__PointTarget_MinMaxIndices[i].y;
		if(indexCheck)
		{
			float3 TrgPos = AirCSVariables__PointTarget_Pos[i] + 
							RndDir * AirCSVariables__PointTarget_GaussianOffset[i];
			float3 attrVec = TrgPos - Pos;
	
			attrVec *=  AirCSVariables__PointTarget_Strength[i];
			
			//transform attraction vector:
			attrVec = mul(float4(attrVec,1), AirCSVariables__PointTarget_VectorTransform).xyz;
			vec += attrVec;
		}
	}
	return vec;
}