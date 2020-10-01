
int ringLength = 1;

struct posVel
{
	float3 pos;
	float bornTime;
};
StructuredBuffer<posVel> Input;
int ringBufferPhase;

RWTexture2D<float4> OutputTexture :BACKBUFFER;
RWStructuredBuffer<posVel> OutputBuffer : BACKBUFFER;

// =============================================================================
// =============================================================================
// =============================================================================
// =============================================================================
// =============================================================================
// =============================================================================

[numthreads(128, 1, 1)]
void WriteTextureCS(uint3 DTid : SV_DispatchThreadID )
{
	uint lineID = DTid.x / ringLength;
	uint traceSegmID = DTid.x % ringLength;
	int index = (traceSegmID + ringBufferPhase+1) % ringLength + lineID * ringLength;
		
	OutputTexture[float2(traceSegmID, lineID)] = 
	float4(Input[index].pos,Input[index].bornTime);
}

[numthreads(128, 1, 1)]
void WriteTextureRawCS(uint3 DTid : SV_DispatchThreadID )
{
	uint lineID = DTid.x / ringLength;
	uint traceSegmID = DTid.x % ringLength;
		
	OutputTexture[float2(traceSegmID, lineID)] = 
	float4(Input[DTid.x].pos,Input[DTid.x].bornTime);
}

[numthreads(128, 1, 1)]
void WriteBufferCS(	uint3 DTid : SV_DispatchThreadID )
{
	uint lineID = DTid.x / ringLength;
	uint traceSegmID = DTid.x % ringLength;
	int index = (traceSegmID + ringBufferPhase+1) % ringLength + lineID * ringLength;
		
	OutputBuffer[DTid.x] = Input[index];
}

// =============================================================================
// =============================================================================
// =============================================================================
// =============================================================================
// =============================================================================
// =============================================================================

technique11 RingBuffer_Reordered_to_texture
{
	pass P0
	{
		SetComputeShader(CompileShader( cs_5_0, WriteTextureCS() ) );
	}
}

technique11 RingBuffer_Raw_to_texture
{
	pass P0
	{
		SetComputeShader(CompileShader( cs_5_0, WriteTextureRawCS() ) );
	}
}

technique11 RingBuffer_Reordered_to_buffer
{
	pass P0
	{
		SetComputeShader(CompileShader( cs_5_0, WriteBufferCS() ) );
	}
}