float4x4 tWVP: WORLDVIEWPROJECTION;
float4x4 tVI : VIEWINVERSE;

Texture2D spriteTexture; 
Texture2D energyGradient; 

float spriteRadiusMult = 1.f;
float4 spriteColor <bool color=true;> = 1;

float currentTime;

float4 fadeInOut; //x=fadeIn start; y=fadeIn finish; z=fadeOut start; w=fadeOut finish

struct data
{
	float3 pos;
	float bornTime;
};
StructuredBuffer<data> particlebuffer;

SamplerState linearSampler : IMMUTABLE
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

float3 g_positions[4]: IMMUTABLE =
{
	float3( -1, 1, 0 ),
	float3( 1, 1, 0 ),
	float3( -1, -1, 0 ),
	float3( 1, -1, 0 ),
} ;
float2 g_texcoords[4]: IMMUTABLE = 
{ 
	float2(0,1), 
	float2(1,1),
	float2(0,0),
	float2(1,0),
};

struct vsInBuffer
{
	uint iv : SV_VertexID;
};

struct gsIn
{
    float4 pos: POSITION ;
	float size: TEXCOORD0 ;
}; 

struct psIn
{
	float4 pos: SV_POSITION;
    float2 uv: TEXCOORD01 ;
};


gsIn VSBuffer(vsInBuffer input)
{
    gsIn output;
    output.pos  = float4(particlebuffer[input.iv].pos,1.0f);
	float lifeTime = currentTime - particlebuffer[input.iv].bornTime ;
	float scale = 	smoothstep(fadeInOut.x,fadeInOut.y,lifeTime) *
					smoothstep(fadeInOut.w,fadeInOut.z,lifeTime);
	
	output.size = spriteRadiusMult * scale;

    return output;
}

[maxvertexcount(4)]
void GS_Particle(point gsIn input[1], inout TriangleStream<psIn> SpriteStream)
{
    psIn output;
    for(int i=0; i<4; i++)
    {
        float3 position = g_positions[i]*input[0].size;
        position = mul( position, (float3x3)tVI ) + input[0].pos.xyz;
        output.pos = mul( float4(position,1.0), tWVP );
        output.uv = g_texcoords[i];
        SpriteStream.Append(output);
    }
    SpriteStream.RestartStrip();
}


float4 PS_Tex(psIn input): SV_Target
{
    float4 col = spriteTexture.Sample( linearSampler, input.uv) * spriteColor;
	return col;
}

technique10 RenderSpriteBufferTextured
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VSBuffer() ) );
		SetGeometryShader( CompileShader( gs_4_0, GS_Particle() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Tex() ) );
	}
} 



 
