//@author: dottore

// =============================================================================
// PULSE LIGHT HEADER ===========================================================
// =============================================================================

//PulseLight Position Buffer:
StructuredBuffer<float3> pulsePos;
//PulseLight Data Buffer (X = radius, Y = Strength)
StructuredBuffer<float2> pulseData;
//PulseLight distance gamma
float pulseDistGamma = 1;

Texture2D pulseTex <string uiname="Pulse Signal Texture";>;

SamplerState pulseSampler :IMMUTABLE
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

// =============================================================================

//Multiple PulseLight
float pulseValue(float3 p)
{
	float value = 0;
	
	uint count,dummy;
	pulsePos.GetDimensions(count,dummy);
	
	float pixSizeY = 1/(float)count;
	
	for(uint i=0 ; i<count; i++)
	{
		//attrVec = p - attrBuffer[i];
		float3 pulseVec = pulsePos[i] - p;
		float pulseRadius = pulseData[i].x;
		float pulseStrength = pulseData[i].y;

		float pulseTexCdX = length(pulseVec) / pulseRadius;
		pulseTexCdX = pow(saturate(pulseTexCdX), pulseDistGamma);
		float pulseTexCdY = pixSizeY*0.5 + pixSizeY*i;
		//float distFactor = pow(saturate(1-pulseTexCdX), pulseDistGamma);
		float pulseForce = pulseTex.SampleLevel(pulseSampler, float4(pulseTexCdX,pulseTexCdY,0,0),0).x;

		value += pulseForce * pulseStrength * (1-pulseTexCdX);
	}
	return value;
}

//Multiple PulseLight
struct pulse
{
	float value;
	float3 vec;
};

pulse pulseResults(float3 p)
{
	pulse Out;
	Out.value = 0;
	Out.vec = 0;
	
	uint count,dummy;
	pulsePos.GetDimensions(count,dummy);
	
	float pixSizeY = 1/(float)count;
	
	for(uint i=0 ; i<count; i++)
	{
		//attrVec = p - attrBuffer[i];
		float3 pulseVec = pulsePos[i] - p;
		float pulseRadius = pulseData[i].x;
		float pulseStrength = pulseData[i].y;

		float pulseTexCdX = length(pulseVec) / pulseRadius;
		pulseTexCdX = pow(saturate(pulseTexCdX), pulseDistGamma);
		float pulseTexCdY = pixSizeY*0.5 + pixSizeY*i;
		//float distFactor = pow(saturate(1-pulseTexCdX), pulseDistGamma);
		float pulseForce = pulseTex.SampleLevel(pulseSampler, float3(pulseTexCdX,pulseTexCdY,0),0);

		float pulseValue = pulseForce * pulseStrength * (1-pulseTexCdX);
		
		Out.value += pulseValue;
		Out.vec += normalize(pulseVec) * pulseValue;
	}
	return Out;
}