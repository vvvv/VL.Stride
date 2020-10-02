float4x4 tV : VIEW ;
float4x4 tVP : VIEWPROJECTION ;
float4x4 tVI : VIEWINVERSE ;

//float4 col <bool color=true;String uiname="Color";> = { 1.0f,1.0f,1.0f,1.0f };

struct p
{
	float4 Pos;		// float3 pos + float rnd (random value in 0 to 1 space)
	float4 Vel;		// float3 vel + float rnd (random value in 0 to 1 space)
	float4 Col;		// float3 col + float lifeT
	float4 RndDir;	// float3 random direction values (gaussian distribution) + float rnd (random value in 0 to 1 space)
	float4 Info;	// float heat + float dynamic + 0 + 0
};

StructuredBuffer<p> BufferOutput;

float2 lifeTimeScale_MinMax = float2(0, 0.5);

//==============================================================================
// FUNCTIONS ===================================================================
//==============================================================================

float Culling(float3 pos)
{
	float4 projected = mul(float4(pos, 1), tVP);
	projected.xyz /= projected.w;
	float result = 0;
	
	if(	all(projected.xy > -1.05) && 
		all(projected.xy < 1.05) && 
		projected.z > 0 && 
		projected.z < 1)
	{	result = 1;	}
	return result;
}

//==============================================================================
// VERTEX SHADER ===============================================================
//==============================================================================

struct VS_IN
{
	uint iv : SV_VertexID;
};

struct VS_OUT
{
    float3 posW: POSITION ;	
	float3 col : COLOR ;
	float Size : TEXCOORD0 ;
};

VS_OUT VS(VS_IN In)
{
    VS_OUT Out = (VS_OUT)0;
		
    Out.posW = BufferOutput[In.iv].Pos.xyz;
	//Out.col = BufferOutput[In.iv].Vel*0.05 +0.5;//BufferOutput[In.iv].Col.xyz;
	Out.col = BufferOutput[In.iv].Col.xyz ;//* (BufferOutput[In.iv].Vel.xyz*0.05 +0.5);

	Out.Size = smoothstep(lifeTimeScale_MinMax.x, lifeTimeScale_MinMax.y, BufferOutput[In.iv].Col.w);
	
    return Out;
}

//==============================================================================
// GEOMETRY SHADER =============================================================
//==============================================================================

float radius = 0.05f;

float3 g_positions[4]:IMMUTABLE =
{
    float3( -1, 1, 0 ),
    float3( 1, 1, 0 ),
    float3( -1, -1, 0 ),
    float3( 1, -1, 0 ),
};
float2 g_texcoords[4]:IMMUTABLE = 
{ 
    float2(0,1), 
    float2(1,1),
    float2(0,0),
    float2(1,0),
};

struct GS_OUT
{
    float4 posWVP: SV_POSITION ;
	float3 col : COLOR ;
	float2 texCd : TEXCOORD1 ;
};

[maxvertexcount(4)]
void GS(point VS_OUT In[1], inout TriangleStream<GS_OUT> SpriteStream)
{
    GS_OUT Out;
    	
	if(Culling(In[0].posW))
	{		
	    Out.col = In[0].col;		

		for(int i=0; i<4; i++)
		{
			float3 position = g_positions[i] * radius * In[0].Size;
		    position = mul(position, (float3x3)tVI ) + In[0].posW.xyz;
			
		    Out.posWVP = mul(float4(position,1.0), tVP);
		    Out.texCd = g_texcoords[i];	
			
		    SpriteStream.Append(Out);
		}
		SpriteStream.RestartStrip();	
	}
}

//==============================================================================
// PIXEL SHADER ================================================================
//==============================================================================

float4 PS(GS_OUT In): SV_Target
{
	if(length(In.texCd-.5)>.5){discard;}

	return float4(In.col, 1);
}


//==============================================================================
// TECHIQUES ===================================================================
//==============================================================================


technique11 _draw
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_5_0, VS() ) );
		SetGeometryShader( CompileShader( gs_5_0, GS() ) );
		SetPixelShader( CompileShader( ps_5_0, PS() ) );
	}
}


