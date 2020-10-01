//@author: dottore

int pCount;
float timeStep : TIMESTEP;

//POINTCLOUD TARGET
float trgForce <string uiname="PointCloud Target Strength";>;

//INTEGRATION:
StructuredBuffer<float4> random;
float2 damp_minDelta;
float damp_gamma = 0.5;

//WIND:
float3 wind;

//NOISE FORCE:
float noise_amount;
float noise_time;
int noise_oct;
float noise_freq = 1;
float noise_lacun = 1.666;
float noise_pers = 0.666;

//PULSE LIGHTS:
float pulseLight_forceAmount = 0.1;

//RandomDirectionBuffer
StructuredBuffer<float3> rndDir;
int brwIndexShift;
float brwnStrenght;

struct particle
{
	float3 pos;
	float3 vel;
	float2 info;
};
RWStructuredBuffer<particle> Output : BACKBUFFER;

#include "NoiseFunction.fxh"
#include "h_Attractors.fxh"
#include "h_PulseLights.fxh"

//==============================================================================
//COMPUTE SHADER ===============================================================
//==============================================================================

[numthreads(128, 1, 1)]
void CSConstantForce( uint3 DTid : SV_DispatchThreadID )
{
	float3 p = Output[DTid.x].pos;
	float3 v = Output[DTid.x].vel;
	float2 i = Output[DTid.x].info;

	// Velocity Drag: ----------------------------------------------------------
	float damping = pow(abs(random[DTid.x].w+0.5), damp_gamma);
	damping *= damp_minDelta.y;
	damping += damp_minDelta.x;	
	v *= damping;

	float3 force = 0;
	float2 infoAdd = 0;
	
	// Attractors force: -------------------------------------------------------
	force += attrVel(p);

	// Pulse Lights: --------------------------------------------------------------
	pulse pulseData = pulseResults(p);
	force -= pulseData.vec * pulseLight_forceAmount;
	
	// Wind force: -------------------------------------------------------
	force += wind;

	// Noise Force: ------------------------------------------------------------
	float3 noiseF = float3(	fBm(float4(p+float3(51,2.36,-5),noise_time),noise_oct,noise_freq,noise_lacun,noise_pers),
							fBm(float4(p+float3(98.2,-9,-36),noise_time),noise_oct,noise_freq,noise_lacun,noise_pers),
							fBm(float4(p+float3(0,10.69,6),noise_time),noise_oct,noise_freq,noise_lacun,noise_pers));
	noiseF *= noise_amount;
	force += noiseF;
	
	// Brownian force: ---------------------------------------------------------
	uint rndIndex = DTid.x + brwIndexShift;
	rndIndex = rndIndex % pCount;
	float3 brwnF = rndDir[rndIndex];
	force += brwnF * brwnStrenght;
	force += brwnF * pulseData.vec * pulseLight_forceAmount *10;

	// Pointcloud Target force: ------------------------------------------------
	float3 trgPos = rndDir[DTid.x] * 10;
	float3 trgF = (trgPos - p) * trgForce;
	force += trgF;
	
	// Integrate: --------------------------------------------------------------
	Output[DTid.x].vel = v + force;
	Output[DTid.x].pos = p + v*timeStep;
	Output[DTid.x].info =  float2(pulseData.value, 0);
}

[numthreads(128, 1, 1)]
void CS_Reset( uint3 DTid : SV_DispatchThreadID )
{
	float3 trgPos = rndDir[DTid.x] * 10;
	Output[DTid.x].pos = trgPos;//resetData[DTid.x].xyz;
	Output[DTid.x].vel = 0;
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
