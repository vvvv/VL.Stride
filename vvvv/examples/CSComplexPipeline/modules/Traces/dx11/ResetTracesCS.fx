#include <packs\dx11.particles\nodes\modules\Core\fxh\Core.fxh>

struct Particle {
	float4 Pos;		// float3 pos (default 0,0,0) + float rnd (random value in 0 to 1 space)
	float4 Vel;		// float3 vel (default 0,0,0) + float rnd (random value in 0 to 1 space)
	float4 Col;		// float3 col (default 1,1,1) + float lifeT (default 0)
	float4 RndDir;	// float3 random direction values (gaussian distribution) + float rnd (random value in 0 to 1 space)
	float4 Info;	// float heat (default 0) + float dynamic (default 0) + 0 + 0
};

StructuredBuffer<Particle> ParticleBuffer;

int ParticlesCount = 1;
int ringLength = 1;
int EmissionIndexOffset;
float currentTime;
int TracesCount;
uint tracesPointerOffset;


struct trace
{
	float3 pos;
	float bornTime;
	uint refIndex;
};
RWStructuredBuffer<trace> Output : BACKBUFFER;


[numthreads(1, 1, 1)]
void Reset_Particles_CS( uint3 DTid : SV_DispatchThreadID )
{		
	uint pIndex = (DTid.x+EmissionIndexOffset)% ParticlesCount;
	uint binIndex = ((tracesPointerOffset+DTid.x)%TracesCount) * ringLength;
	
	trace Out;
	
	Out.pos =ParticleBuffer[pIndex].Pos.xyz;
	Out.bornTime = currentTime;
	Out.refIndex = pIndex;
	
	for(uint i=0; i<(uint)ringLength; i++)
	{
		Output[binIndex+i] = Out;
	}
}


//==============================================================================
//==============================================================================
//==============================================================================

technique11 Reset_FromParticles
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, Reset_Particles_CS() ) );
	}
}
/*
technique11 FromRopes
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, Reset_Ropes_CS() ) );
	}
}
*/
