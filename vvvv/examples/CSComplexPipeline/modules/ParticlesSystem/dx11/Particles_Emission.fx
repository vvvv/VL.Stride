
int particlesCount;
int offset;

struct emitSelect
{
	uint ppf; //emitter particles per frame
	uint index;
};
StructuredBuffer<emitSelect> SelectedEmitters;


StructuredBuffer<float3> emittersPos;
StructuredBuffer<float3> emittersVel;

StructuredBuffer<float4> random;

struct data
{
	float3 pos;
	float3 vel;
	float energy;
	uint tag;
};
RWStructuredBuffer<data> Output : BACKBUFFER;

float3 resetPos;

// =============================================================================
// EMISSION:

[numthreads(1, 1, 1)]
void emitCS( uint3 DTid : SV_DispatchThreadID )
{
	uint pIndex = (DTid.x + offset) % particlesCount;
	uint emitIndex = SelectedEmitters[DTid.x].index;
	uint emitEnergy = SelectedEmitters[DTid.x].ppf;
	
	Output[pIndex].vel = emittersVel[emitIndex];

	Output[pIndex].pos = emittersPos[emitIndex];
	Output[pIndex].energy = emitEnergy;
	Output[pIndex].tag = emitIndex;
}

[numthreads(256, 1, 1)]
void resetCS( uint3 DTid : SV_DispatchThreadID )
{	
	Output[DTid.x].pos = resetPos;
	Output[DTid.x].vel = 0;
	Output[DTid.x].energy = 0;
	Output[DTid.x].tag = 0;
}

//==============================================================================
//==============================================================================
//==============================================================================

technique11 Emission
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, emitCS() ) );
	}
}

technique11 Reset
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, resetCS() ) );
	}
}
