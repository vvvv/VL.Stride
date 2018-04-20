#include <packs\dx11.particles\nodes\modules\Core\fxh\Core.fxh>

struct Particle {
	#if defined(COMPOSITESTRUCT)
  		COMPOSITESTRUCT
 	#else
		float3 position;
		float3 scale;
		float3 velocity;
		float3 force;
		float lifespan;
		float age;
		float4 color;
		float mass;
	#endif
};
RWStructuredBuffer<Particle> ParticleBuffer : PARTICLEBUFFER;

int ParticlesCount = 1;
int ringLength = 1;
int ringBufferPhase;
int EmissionIndexOffset;
RWStructuredBuffer<float4> Output : BACKBUFFER;



[numthreads(64, 1, 1)]
void ring_CS( uint3 DTid : SV_DispatchThreadID )
{	
	if (DTid.x >= (uint)ParticlesCount) return;
	
	uint id = (DTid.x * ringLength) + ringBufferPhase ;
	
	float4 Out = float4(ParticlesBuffer[DTid.x].pos, ParticlesBuffer[DTid.x].Ucoord.x);
	Output[id] = Out;
}

[numthreads(1, 1, 1)]
void resetBins_CS( uint3 DTid : SV_DispatchThreadID )
{		
	uint index = (DTid.x+EmissionIndexOffset)% ParticlesCount;
	uint binIndex = index * ringLength;
	 
	for(uint i=0; i<(uint)ringLength; i++)
	{
		float4 Out = float4(ParticlesBuffer[index].pos, ParticlesBuffer[index].Ucoord.x);
		Output[binIndex+i] = Out;
	}
}

[numthreads(128, 1, 1)]
void resetAll_CS( uint3 DTid : SV_DispatchThreadID )
{		
	uint binId = DTid.x/ringLength;

	float4 Out = float4(ParticlesBuffer[binId].pos, ParticlesBuffer[binId].Ucoord.x);

	Output[DTid.x] = Out;
}

//==============================================================================
//==============================================================================
//==============================================================================

technique11 setRingBuffer
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, ring_CS() ) );
	}
}

technique11 resetBins
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, resetBins_CS() ) );
	}
}

technique11 resetAll
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, resetAll_CS() ) );
	}
}
