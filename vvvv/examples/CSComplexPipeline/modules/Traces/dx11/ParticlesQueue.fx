int ParticlesCount;
float frameCount;
bool PingPong;

float Verlet_TrgDist = 0.1; //Verlet target distance

struct particle
{
	float3 pos;
	float3 vel;
	float bornT;
	float3 col;
	float energy;
	uint tag;
};
StructuredBuffer<particle> pData;

RWStructuredBuffer<float3> RWValueBuffer : BACKBUFFER;


[numthreads(64,1,1)]
void CS_Queue(uint3 dtid : SV_DispatchThreadID)
{
	//if (dtid.x >= threadCount) { return; }
		
	uint ReadOffset = !PingPong * ParticlesCount * frameCount;
	uint WriteOffset = PingPong * ParticlesCount * frameCount;
	
	float rowIndex = floor(dtid.x / ParticlesCount);
	
	if(rowIndex == 0) RWValueBuffer[dtid.x+WriteOffset]= pData[dtid.x].pos;
	else RWValueBuffer[dtid.x+WriteOffset] = RWValueBuffer[dtid.x-ParticlesCount+ReadOffset];
}

[numthreads(64,1,1)]
void CS_Verlet(uint3 dtid : SV_DispatchThreadID)
{
	//if (dtid.x >= threadCount) { return; }
		
	uint ReadOffset = !PingPong * ParticlesCount * frameCount;
	uint WriteOffset = PingPong * ParticlesCount * frameCount;
	
	float rowIndex = floor(dtid.x / ParticlesCount);
	
	if(rowIndex == 0) RWValueBuffer[dtid.x+WriteOffset]= pData[dtid.x].pos;
	else 
	{
		float3 PosW_Current = RWValueBuffer[dtid.x+ReadOffset];
		float3 PosW_Parent = RWValueBuffer[dtid.x-ParticlesCount+ReadOffset];
		
		float3 vec = PosW_Parent-PosW_Current;
		float3 PosW_New = PosW_Current + ((dot(vec,vec)-(Verlet_TrgDist*Verlet_TrgDist)) * normalize(vec));
		
	RWValueBuffer[dtid.x+WriteOffset] = PosW_New;//RWValueBuffer[dtid.x-ParticlesCount+ReadOffset];
		
	}
}



technique11 Queue
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, CS_Queue() ) );
	}
}

technique11 Verlet
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, CS_Verlet() ) );
	}
}






