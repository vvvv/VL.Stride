
//##################################################
//##################################################
// PROVIDED FROM PIPELINE:
//##################################################
//##################################################

int AirCSVariables__Count; // particles count
float AirCSVariables__tStep;	// provided by UE (computational time in milliseconds of the previous frame)
static float sTepFactor = 20;
static float FPSfactor = AirCSVariables__tStep * sTepFactor;

struct p
{
	float4 Pos;		// float3 pos (default 0,0,0) + float rnd (random value in 0 to 1 space)
	float4 Vel;		// float3 vel (default 0,0,0) + float rnd (random value in 0 to 1 space)
	float4 Col;		// float3 col (default 1,1,1) + float lifeT (default 0)
	float4 RndDir;	// float3 random direction values (gaussian distribution) + float rnd (random value in 0 to 1 space)
	float4 Info;	// float heat (default 0) + float dynamic (default 0) + 0 + 0
};

RWStructuredBuffer<p> BufferInput : BACKBUFFER;
//RWStructuredBuffer<p> BufferOutput ;

// INITIALIZATION RANDOM VALUES
StructuredBuffer<float4> Random; // random values in 0 to 1 space
StructuredBuffer<float3> RndDir; // random gaussian values in -1 to 1 space




//##################################################
//##################################################
// GLOBAL
//##################################################
//##################################################

// PARTICLES:
float AirCSVariables__ParticlesRadius = 0.01;

// AIR FRICTION
float2 AirCSVariables__AirFriction_MinMax = float2(0.03, 0.48);
static float2 AirCSVariables__AirFriction_MinDelta = float2(AirCSVariables__AirFriction_MinMax.x,
															AirCSVariables__AirFriction_MinMax.y-AirCSVariables__AirFriction_MinMax.x);
float AirCSVariables__AirFriction_Gamma = 0.5;

// RANDOM MASS:
float AirCSVariables__RandomMass = 0.1;

// GRAVITY:
float3 AirCSVariables__Gravity = float3(0,0,0);

// HEAT:
float AirCSVariables__HeatDecreaseFactor = 0.9;

// TARGET COLOR
uint AirCSVariables__TrgCol_Count = 1;
float AirCSVariables__TrgColLerp = 0.01;
StructuredBuffer<float3> AirCSVariables__TrgCol;	// default = 1,1,1

// PLANE TARGET
bool AirCSVariables__PlaneTarget_Enable = 1;
float4x4 AirCSVariables__PlaneTarget_Transform = float4x4(7,0,0,0,	0,9,0,0,	0,0,0,0,	0,0,0,1); 
float4x4 AirCSVariables__PlaneTarget_TransformInv = float4x4(0,0,0,0,	0,0,0,0,	0,0,30819264,0,	0,0,0,0);
float AirCSVariables__PlaneTarget_Strength = 0.03;

// GROUND COLLISION:
bool AirCSVariables__GroundCollision_Enable = 1;
float AirCSVariables__GroundCollision_Elastic = 0.5;
float AirCSVariables__GroundCollision_Friction = 0.5;

// PERLIN:
#include "h_Perlin.fxh"

// BROWNIAN:
bool AirCSVariables__Brownian_Enable = 0;
int AirCSVariables__Brownian_IndexShift = 0;
float AirCSVariables__Brownian_Strenght = 0;
float AirCSVariables__Brownian_HeatInfluence = 0;

// VOLUME FIELD:
#include "h_VolumeField.fxh"

//##################################################
//##################################################
// FORCE OBJECTS
//##################################################
//##################################################

// ATTRACTOR:
#include "h_Attractor.fxh"

// POINT TARGET:
#include "h_PointTarget.fxh"

// BRUSH:
#include "h_Brush.fxh"

// CYMATIC:
#include "h_Cymatic.fxh"

// FLOW:
#include "h_Flow.fxh"

// PULSE:
#include "h_Pulse.fxh"

//==============================================================================
//==============================================================================
//==============================================================================

#define pi 3.14159265358979

//==============================================================================
//==============================================================================
//==============================================================================
//==============================================================================
//==============================================================================
//COMPUTE SHADER ===============================================================
//==============================================================================
//==============================================================================
//==============================================================================
//==============================================================================
//==============================================================================

[numthreads(128, 1, 1)]
void CSConstantForce( uint3 DTid : SV_DispatchThreadID )
{
	if(DTid.x >= (uint)AirCSVariables__Count) return;
	
	float3 Pos = BufferInput[DTid.x].Pos.xyz;
	float3 Vel = BufferInput[DTid.x].Vel.xyz;
	float3 Col = BufferInput[DTid.x].Col.xyz;
	float LifeT = BufferInput[DTid.x].Col.w;
	float Heat = BufferInput[DTid.x].Info.x * AirCSVariables__HeatDecreaseFactor;	
	float Dynamic = BufferInput[DTid.x].Info.x;	
	float3 Rnd = float3(BufferInput[DTid.x].Pos.w, 
						BufferInput[DTid.x].Vel.w,
						BufferInput[DTid.x].RndDir.w);
	float3 RndDir = BufferInput[DTid.x].RndDir.xyz;

	float3 Force = 0;
	
	// AIR FRICTION:
	float AirFr = pow(abs(Rnd.y), AirCSVariables__AirFriction_Gamma);
	AirFr *= AirCSVariables__AirFriction_MinDelta.y;
	AirFr += AirCSVariables__AirFriction_MinDelta.x;		
	// Drag Force:
	Vel += -Vel * AirFr * FPSfactor;

	

	// ##################################################
	// ##################################################
	// GLOBAL
	// ##################################################
	// ##################################################

	// PERLIN:
	float3 PerlinForce = 0;
	if(AirCSVariables__Perlin_Enable)
	{
		PerlinForce = float3(	fBm(float4(Pos+float3(51,2.36,-5),AirCSVariables__Perlin_Time), AirCSVariables__Perlin_Oct, AirCSVariables__Perlin_Freq, AirCSVariables__Perlin_Lacun, AirCSVariables__Perlin_Pers),
								fBm(float4(Pos+float3(98.2,-9,-36),AirCSVariables__Perlin_Time), AirCSVariables__Perlin_Oct, AirCSVariables__Perlin_Freq, AirCSVariables__Perlin_Lacun, AirCSVariables__Perlin_Pers),
								fBm(float4(Pos+float3(0,10.69,6),AirCSVariables__Perlin_Time), AirCSVariables__Perlin_Oct, AirCSVariables__Perlin_Freq, AirCSVariables__Perlin_Lacun, AirCSVariables__Perlin_Pers));
		Force += PerlinForce * max(AirCSVariables__Perlin_Strenght,
									(AirCSVariables__Perlin_HeatInfluence * Heat));
	}
	
	// BROWNIAN:
	if(AirCSVariables__Brownian_Enable)
	{
		uint rndIndex = DTid.x + AirCSVariables__Brownian_IndexShift;
		rndIndex = rndIndex % AirCSVariables__Count;
		float3 BrwnForce = BufferInput[rndIndex].RndDir.xyz;
		Force += BrwnForce * max(AirCSVariables__Brownian_Strenght,
								(AirCSVariables__Brownian_HeatInfluence * Heat));
	}

	// PLANE TARGET FORCE:
	if(AirCSVariables__PlaneTarget_Enable)
	{
		float3 TrgPos = mul(float4(Rnd.xy-0.5, 0, 1),AirCSVariables__PlaneTarget_Transform).xyz;
		float3 trgF = (TrgPos - Pos) * AirCSVariables__PlaneTarget_Strength;
		Force += trgF;	
	}

	// VOLUME FIELD:	
	if(AirCSVariables__Vol_Enable)
	{
		float4 VelHeat = VolField(Pos);
		Force += VelHeat.xyz * AirCSVariables__Vol_ForceInfluence;
		Heat += VelHeat.w * AirCSVariables__Vol_HeatInfluence;
	}
	
	// ##################################################
	// ##################################################
	// FORCE OBJECTS
	// ##################################################
	// ##################################################

	
	// ATTRACTOR:
	if(AirCSVariables__Attractor_Enable)
	Force += Attractor(Pos, Rnd, RndDir);

	// POINT TARGET:
	if(AirCSVariables__PointTarget_Enable)
	Force += PointTarget(Pos, DTid.x, RndDir);

	// BRUSH:
	if(AirCSVariables__Brush_Enable)
	{
		float4 ColHeat = Brush(Pos, float4(Col, Heat));
		Col = ColHeat.xyz;
		Heat += ColHeat.w;
	}

	// CYMATIC -----------------------------------------------------------------	
	if(AirCSVariables__Cymatic_Enable)
	for(uint i=0 ; i<AirCSVariables__Cymatic_Count; i++)
	{
		float3 attrVec = AirCSVariables__Cymatic_Pos[i] - Pos;
		float attrRadius = AirCSVariables__Cymatic_RadiusStrength[i].x;
		float Strength = AirCSVariables__Cymatic_RadiusStrength[i].y;
		float dist = length(attrVec);
		float gradient = dist / attrRadius;
		float wave = sin((dist * AirCSVariables__Cymatic_FreqPhase[i].x) 
					+ RndDir.y * AirCSVariables__Cymatic_RandomShiftAmount
					+ AirCSVariables__Cymatic_FreqPhase[i].y * 6.2831853071796);
		gradient = 1 - gradient;
		gradient = saturate(gradient);
		gradient = pow(gradient, AirCSVariables__Cymatic_RadiusGamma);
		attrVec = normalize(attrVec) * gradient * wave * Strength;

		Force += attrVec;
		//Col = lerp(Col, AirCSVariables__Cymatic_Color, pow(wave*0.5+0.5, AirCSVariables__Cymatic_ColorGradientGamma) * gradient * Strength * AirCSVariables__Cymatic_SetColorAmount);
		//Heat += AirCSVariables__Cymatic_Heat * gradient;
	}
	
	

	// FLOW:
	if(AirCSVariables__Flow_Enable)
	Force += Flow(Pos);

	// PULSE -------------------------------------------------------------------
	if(AirCSVariables__Pulse_Enable)
	{
		float PulseGradient = 0;		
		float pixSizeY = 1/(float)AirCSVariables__Pulse_Count;
		
		[allow_uav_condition]
		for(uint i=0 ; i<AirCSVariables__Pulse_Count; i++)
		{
			float3 pulseVec = AirCSVariables__Pulse_Pos[i] - Pos;
			float pulseRadius = AirCSVariables__Pulse_RadiusGammaStrength[i].x;
			float pulseTexCdX = (length(pulseVec)+AirCSVariables__Pulse_DistOffset) / pulseRadius;
			if(pulseTexCdX < 1)
			{
				float pulseStrength = AirCSVariables__Pulse_RadiusGammaStrength[i].z;
		
				pulseTexCdX = pow(saturate(pulseTexCdX), AirCSVariables__Pulse_RadiusGammaStrength[i].y);
				pulseTexCdX += Rnd.y*0.05;
				float pulseTexCdY = pixSizeY*0.5 + pixSizeY*i;
				float pulseForce = AirCSTextures_PulseTexture.SampleLevel(AirCSTextures_PulseSampler, float2(pulseTexCdX,pulseTexCdY),0).x;
		
				float Gradient = pulseForce * pulseStrength * (1-pulseTexCdX);
				
				PulseGradient += Gradient;
				
				Force += -normalize(pulseVec) * Gradient;// * Cymatics_addDirVector;
				Col = lerp(Col, AirCSVariables__Pulse_Color, pow(saturate(pulseForce), AirCSVariables__Pulse_ColorGradientGamma) * (1-pulseTexCdX) * AirCSVariables__Pulse_SetColorAmount);
				Heat += AirCSVariables__Pulse_Heat * Gradient;

			}
		}
	}
	
	

	// GRAVITY:
	Vel += AirCSVariables__Gravity;// * GroundCollisionSmooth;

	// GROUND COLLISION:
	float BuoyancyLevel = 0;//AirCSVariables__ParticlesRadius * 0.5;
	// Friction:
	float WaterProximity = Pos.y - BuoyancyLevel;
	WaterProximity *= 0.1; 	// from 0 level to 10: proximity gradient
	WaterProximity = 1 - saturate(WaterProximity);
	Vel += -Vel * (WaterProximity*0.001);	// water friction strength
	
	
	// ##################################################
	// ##################################################
	// INTEGRATION
	// ##################################################
	// ##################################################

	float3 TrgCol = AirCSVariables__TrgCol[DTid.x % AirCSVariables__TrgCol_Count].xyz;
	BufferInput[DTid.x].Col.xyz = max(lerp(Col, TrgCol, AirCSVariables__TrgColLerp),0);

	float3 Acc = Force / (1 + Rnd.z * AirCSVariables__RandomMass);
	
	BufferInput[DTid.x].Vel.xyz = Vel + Acc *FPSfactor;
	BufferInput[DTid.x].Pos.xyz = Pos + Vel * FPSfactor;
	
	// GROUND COLLISION:
	// Collision:
	if(BufferInput[DTid.x].Pos.y < BuoyancyLevel && AirCSVariables__GroundCollision_Enable) 
	{
		BufferInput[DTid.x].Vel.xyz = reflect(BufferInput[DTid.x].Vel.xyz, float3(0,AirCSVariables__GroundCollision_Elastic,0));	
		BufferInput[DTid.x].Pos.y = BuoyancyLevel;//abs(BufferInput[DTid.x].Pos.z-AirCSVariables__ParticlesRadius) + AirCSVariables__ParticlesRadius;	
	}
	
	BufferInput[DTid.x].Col.w += AirCSVariables__tStep;
	BufferInput[DTid.x].Info.x = Heat;
}

[numthreads(128, 1, 1)]
void CS_Reset( uint3 DTid : SV_DispatchThreadID )
{
	float3 trgPos = mul(float4(Random[DTid.x].xy-0.5,0, 1),AirCSVariables__PlaneTarget_Transform).xyz;
	BufferInput[DTid.x].Pos.xyz = trgPos;
	BufferInput[DTid.x].Pos.w = Random[DTid.x].x;
	BufferInput[DTid.x].Vel.w = Random[DTid.x].y;
	BufferInput[DTid.x].Vel.xyz = 0;
	BufferInput[DTid.x].Info = 0;
	BufferInput[DTid.x].Col.xyz = 1;
	BufferInput[DTid.x].RndDir = float4(RndDir[DTid.x], Random[DTid.x].z);

}

//==============================================================================
// TECHNIQUES ==================================================================
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
