#include <packs\dx11.particles\nodes\modules\Core\fxh\Core.fxh>

struct Particle {
	float4 Pos;		// float3 pos (default 0,0,0) + float rnd (random value in 0 to 1 space)
	float4 Vel;		// float3 vel (default 0,0,0) + float rnd (random value in 0 to 1 space)
	float4 Col;		// float3 col (default 1,1,1) + float lifeT (default 0)
	float4 RndDir;	// float3 random direction values (gaussian distribution) + float rnd (random value in 0 to 1 space)
	float4 Info;	// float heat (default 0) + float dynamic (default 0) + 0 + 0
};
StructuredBuffer<Particle> ParticleBuffer;

int ringLength = 1;
int ringBufferPhase;

float currentTime;

struct trace
{
	float3 pos;
	float bornTime;
	uint refIndex;
};
RWStructuredBuffer<trace> Output : BACKBUFFER;


[numthreads(64, 1, 1)]
void Write_Particles_CS( uint3 DTid : SV_DispatchThreadID )
{		
	//if (DTid.x >= (uint)ParticlesCount) return;
	
	uint id = (DTid.x * ringLength) + ringBufferPhase ;
	
	uint refIndex = Output[id].refIndex;
	
	Output[id].pos = ParticleBuffer[refIndex].Pos.xyz;
	Output[id].bornTime = currentTime;
}


//==============================================================================
//==============================================================================
//==============================================================================

technique11 Write_FromParticles
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, Write_Particles_CS() ) );
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
