
float currentTime;

//TARGET
float4x4 tTarget <string uiname="Target Transform";>;
float4x4 tTargetInv <string uiname="Target Transform Inverse";>;
float trgForce;

//SIMULATION
int pCount;
float3 gravity;
StructuredBuffer<float4> random;
float2 damp_minDelta;
float damp_gamma = 0.5;


struct particle
{
	float3 pos;
	float3 vel;
	float bornT;
	float3 col;
	float energy;
	uint tag;
};
RWStructuredBuffer<particle> Output : BACKBUFFER;

#include "h_Attractors.fxh"
//==============================================================================
//COMPUTE SHADER ===============================================================
//==============================================================================

[numthreads(128, 1, 1)]
void CSConstantForce( uint3 DTid : SV_DispatchThreadID )
{
	float3 p = Output[DTid.x].pos;
	float3 v = Output[DTid.x].vel;

	//Velocity Damping:
	float damping = pow(abs(random[DTid.x].w+0.5), damp_gamma);
	damping *= damp_minDelta.y;
	damping += damp_minDelta.x;	
	v *= damping;

	// attractors:
	v += attrVel(p);
	
	//TARGET FORCE:
	float3 trgPos = mul(float4(random[DTid.x].xy,0, 1),tTarget).xyz;
	float3 trgF = (trgPos - p) * trgForce;
	v += trgF;

	v += gravity;
	
	Output[DTid.x].vel = v;
	Output[DTid.x].pos = p + v;
}

[numthreads(128, 1, 1)]
void CS_Reset( uint3 DTid : SV_DispatchThreadID )
{
	float3 trgPos = mul(float4(random[DTid.x].xy,0, 1),tTarget).xyz;
	Output[DTid.x].pos = trgPos;//resetData[DTid.x].xyz;
	Output[DTid.x].vel = 0;
	Output[DTid.x].bornT = currentTime;

	float3 pos;
	float3 vel;
	float bornT;
	float3 col;
	float energy;
	uint tag;

}

//==============================================================================
//TECHNIQUES ===================================================================
//==============================================================================

technique11 simulation
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, CSConstantForce() ) );
	}
}

technique11 reset_state
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, CS_Reset() ) );
	}
}
