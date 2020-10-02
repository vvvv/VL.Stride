
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

//GROUND BOUNCE:
bool bounce;
float bounceElastic = 0.5;
float bounceFriction = 0.5;

//NOISE FORCE:
float noise_amount;
float noise_time;
int noise_oct;
float noise_freq = 1;
float noise_lacun = 1.666;
float noise_pers = 0.666;

//RandomDirectionBuffer
StructuredBuffer<float3> rndDir;
int brwIndexShift;
float brwnStrenght;

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

#include "NoiseFunction.fxh"
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
	
	// Noise Force
	float3 noiseF = float3(	fBm(float4(p+float3(51,2.36,-5),noise_time),noise_oct,noise_freq,noise_lacun,noise_pers),
							fBm(float4(p+float3(98.2,-9,-36),noise_time),noise_oct,noise_freq,noise_lacun,noise_pers),
							fBm(float4(p+float3(0,10.69,6),noise_time),noise_oct,noise_freq,noise_lacun,noise_pers));
	noiseF *= noise_amount;
	v += noiseF;
	
	// Brownian
	uint rndIndex = DTid.x + brwIndexShift;
	rndIndex = rndIndex % pCount;
	float3 brwnForce = rndDir[rndIndex];
	v += brwnForce * brwnStrenght;

	//TARGET FORCE:
	float3 trgPos = mul(float4(random[DTid.x].xy,0, 1),tTarget).xyz;
	float3 trgF = (trgPos - p) * trgForce;
	v += trgF;

	//Ground bounce:
	float bounceSmooth = bounceElastic;
	if(p.y < 0 && bounce) 
	{
		v = reflect(v, float3(0,bounceElastic,0));	
		v.xz *= bounceFriction ;
		//Bounce Smoother:
		//get the y space from 0 to 0.1 and use it attenuate gravity
		bounceSmooth *= saturate(length(v)*0.01); 
		//p = reflect(p, float3(0,1,0));
		p.y = abs(p.y+gravity.y);
		//p.y = 0;
	}
	v += gravity * bounceSmooth;
	
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
