int EmissionCSVariables__Count; // particles count
int AirCSVariables__Emit_indexOffset = 0;

StructuredBuffer<uint> AirCSVariables__Emit_PtEmitterID; //default 0

StructuredBuffer<float3> AirCSVariables__Emit_EmitPos; //default float3(0,0,0) 
StructuredBuffer<float3> AirCSVariables__Emit_EmitVel; //default float3(0,0,0)
StructuredBuffer<float3> AirCSVariables__Emit_EmitCol; //default float3(1,1,1)
float AirCSVariables__Emit_PosRndOffsetAmount = 0;
float AirCSVariables__Emit_VelRndAmount = 0;

struct p
{
	float4 Pos;		// float3 pos (default 0,0,0) + float rnd (random value in 0 to 1 space)
	float4 Vel;		// float3 vel (default 0,0,0) + float rnd (random value in 0 to 1 space)
	float4 Col;		// float3 col (default 1,1,1) + float lifeT (default 0)
	float4 RndDir;	// float3 random direction values (gaussian distribution) + float rnd (random value in 0 to 1 space)
	float4 Info;	// float heat (default 0) + float dynamic (default 0) + 0 + 0
};
RWStructuredBuffer<p> BufferInput : BACKBUFFER;

//==============================================================================
//COMPUTE SHADER ===============================================================
//==============================================================================

[numthreads(1, 1, 1)]
void CS_Emit( uint3 DTid : SV_DispatchThreadID )
{
	// Emitters Data:
	uint emitIndex = AirCSVariables__Emit_PtEmitterID[DTid.x];

	// Indices of emitted particles:
	uint index =  DTid.x + AirCSVariables__Emit_indexOffset;
	index = index % EmissionCSVariables__Count;
	
	// Random Position Offset:
	float3 rndEmitPos = AirCSVariables__Emit_PosRndOffsetAmount * BufferInput[index+1].RndDir.xyz;

	// Random emission direction component:
	float3 rndEmitVel = AirCSVariables__Emit_VelRndAmount * BufferInput[index].RndDir.xyz;

	// Write emitted particles:
	BufferInput[index].Pos.xyz = AirCSVariables__Emit_EmitPos[emitIndex] + rndEmitPos;
	BufferInput[index].Vel.xyz = AirCSVariables__Emit_EmitVel[emitIndex] + rndEmitVel;
	BufferInput[index].Col.w = 0;
	BufferInput[index].Col.xyz = AirCSVariables__Emit_EmitCol[emitIndex];
}

//==============================================================================
//TECHNIQUES ===================================================================
//==============================================================================

technique11 Emit
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, CS_Emit() ) );
	}
}
