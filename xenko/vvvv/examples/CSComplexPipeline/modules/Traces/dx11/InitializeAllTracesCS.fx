
struct trace
{
	float3 pos;
	float bornTime;
	uint refIndex;
};
RWStructuredBuffer<trace> Output : BACKBUFFER;


[numthreads(128, 1, 1)]
void InitializeAll_Particles_CS( uint3 DTid : SV_DispatchThreadID )
{		
	
	trace Out;
	
	Out.pos = -99999;
	Out.bornTime = 0;
	Out.refIndex = DTid.x;
	
	Output[DTid.x] = Out;
}


//==============================================================================
//==============================================================================
//==============================================================================

technique11 InitializeAll
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, InitializeAll_Particles_CS() ) );
	}
}
