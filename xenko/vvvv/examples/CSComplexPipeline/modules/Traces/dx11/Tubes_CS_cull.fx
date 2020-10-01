//@author: dottore

struct tube
{
	float id;
	float Ucoord;
	float length;
	float thickness;
};
StructuredBuffer<tube> Input;

AppendStructuredBuffer<tube> Output : BACKBUFFER;

StructuredBuffer<float3> SplinesBuffer ;

float4x4 tV : VIEW ;
float4x4 tVI : VIEWINVERSE ;

//float4x4 Cull_tVP : CULL_VIEWPROJECTION;
float timeStep : TIMESTEP;
float4x4 tW : WORLD ;
float4x4 tVP : VIEWPROJECTION ;
float3 CullPosition : CULL_POS;
float3 CullDirection : CULL_DIR;

int splRes = 64;

//==============================================================================
// FUNCTIONS ===================================================================
//==============================================================================

float DotCullingThresholdValue = 0.55;
float DotCulling(float3 pos)
{
	float3 vec = pos - CullPosition;
	float dotValue = dot(normalize(vec), CullDirection);
	
	return dotValue > DotCullingThresholdValue;
}
/*
float Culling(float3 pos)
{
	float4 projected = mul(float4(pos, 1), tVP);
	projected.xyz /= projected.w;
	float result = 0;
	
	if(	all(projected.xy > -1.05) && 
		all(projected.xy < 1.05) && 
		projected.z > 0 && 
		projected.z < 1)
	{	result = 1;	}
	return result;
}

float CullingWide(float3 pos)
{
	float4 projected = mul(float4(pos, 1), tVP);
	projected.xyz /= projected.w;
	float result = 0;
	
	if(	all(projected.xy > -1.5) && 
		all(projected.xy < 1.5) && 
		projected.z > 0 && 
		projected.z < 1)
	{	result = 1;	}
	return result;
}
*/

//##############################################################################
//##############################################################################
//##############################################################################


[numthreads(64, 1, 1)]
void CS_DotCull( uint3 DTid : SV_DispatchThreadID )
{	
	tube Data = Input[DTid.x];	

	float Ucoord = Data.Ucoord;
	float TubeLength = Data.length;
	
	// Spline Index
	int splID = Data.id ;
	
	float Ucoord_A = 0;	
	float Ucoord_B = 0.5;	
	float Ucoord_C = 1;	
	
	Ucoord_A = Ucoord_A + Ucoord;
	Ucoord_B = Ucoord_B * TubeLength + Ucoord;
	Ucoord_C = Ucoord_C * TubeLength + Ucoord;
	//Out.Ucoord_AB = float2(Ucoord_A, Ucoord_B);
	
	int startctrl = splID * splRes;
	int endctrl = startctrl + splRes -1;

	int Uindex_A = floor(Ucoord_A*splRes) + startctrl;
	int Uindex_B = floor(Ucoord_B*splRes) + startctrl;
	int Uindex_C = floor(Ucoord_C*splRes) + startctrl;

	float3 ctrlPos_A = SplinesBuffer[clamp(Uindex_A, startctrl, endctrl)];
	float3 ctrlPos_B = SplinesBuffer[clamp(Uindex_B, startctrl, endctrl)];
	float3 ctrlPos_C = SplinesBuffer[clamp(Uindex_C, startctrl, endctrl)];
	
	// start or end of the spline
	bool SplineBoundsCull = (Ucoord_C >= 0) && (Ucoord_A <= 1);
	
	bool Cull = DotCulling(ctrlPos_A) || DotCulling(ctrlPos_B) || DotCulling(ctrlPos_C);
	
	
	//Out.MeanDist = distance(ctrlPos_B, CullPos);

	
	
	
	
//	float3 PosW = txData.xyz;
//	float4 PosWVP = mul(float4(PosW, 1), tVP);
		
//	uint index = DTid.x + DTid.y*texRes.x;
	
	
	
	if(SplineBoundsCull && Cull)
	{
		Output.Append(Data);	
	}
}	

//==============================================================================
//TECHNIQUES ===================================================================
//==============================================================================

technique11 DotCull
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, CS_DotCull() ) );
	}
}
