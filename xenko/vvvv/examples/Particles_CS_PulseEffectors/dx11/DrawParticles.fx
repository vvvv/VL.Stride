//@author: dottore
//@help: standard constant shader
//@tags: color
//@credits: 

float4x4 tV : VIEW;
float4x4 tVP : VIEWPROJECTION;
float4x4 tVI : VIEWINVERSE;

float velColMult = 1;

float4 col <bool color=true;String uiname="Color";> = { 1.0f,1.0f,1.0f,1.0f };

struct particle
{
	float3 pos;
	float3 vel;
	float2 info;
};
StructuredBuffer<particle> pData;

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

// particle quad texture:
Texture2D texture2d;
SamplerState g_samLinear : IMMUTABLE
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VS_IN
{
	uint iv : SV_VertexID;
};

struct vs2ps
{
    float4 PosWVP: SV_POSITION ;	
	float2 TexCd : TEXCOORD0 ;
	float4 Vcol : COLOR ;
};

//==============================================================================
// VERTEX SHADER ===============================================================
//==============================================================================

vs2ps VS(VS_IN input)
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;
	
	float3 p = pData[input.iv].pos;
	
    Out.PosWVP = float4(p,1);// mul(float4(po.xyz,1),tVP);

	// SE VUOI USARE IL DATO VELOCITà NEL COLORE:
	float3 v = pData[input.iv].vel;
	Out.Vcol = pData[input.iv].info.x + 0.05;//float4(saturate(v * velColMult)+0.5,1) * col;

    return Out;
}

//==============================================================================
// GEOMETRY SHADER =============================================================
//==============================================================================

[maxvertexcount(4)]
void GS(point vs2ps input[1], inout TriangleStream<vs2ps> SpriteStream)
{
    vs2ps output;
    
    //
    // Emit two new triangles
    //
    for(int i=0; i<4; i++)
    {
        float3 position = g_positions[i]*radius;
        position = mul( position, (float3x3)tVI ) + input[0].PosWVP.xyz;
    	float3 norm = mul(float3(0,0,-1),(float3x3)tVI );
        output.PosWVP = mul( float4(position,1.0), tVP );
        
        output.TexCd = g_texcoords[i];	
        output.Vcol = input[0].Vcol;
    	
        SpriteStream.Append(output);
    }
    SpriteStream.RestartStrip();
}

//==============================================================================
// PIXEL SHADER ================================================================
//==============================================================================

float4 PS_quad(vs2ps In): SV_Target
{
	return In.Vcol;
}

float4 PS_circle(vs2ps In): SV_Target
{
	if(length(In.TexCd-.5)>.5){discard;}

	return In.Vcol;
}

float4 PS_texture(vs2ps In): SV_Target
{
    float4 col = texture2d.Sample( g_samLinear, In.TexCd)*In.Vcol;
	//return In.Vcol;
	return col;
}

//==============================================================================
// TECHIQUES ===================================================================
//==============================================================================

technique11 _quad
{
	pass P0
	{
		
		SetVertexShader( CompileShader( vs_5_0, VS() ) );
		SetGeometryShader( CompileShader( gs_5_0, GS() ) );
		SetPixelShader( CompileShader( ps_5_0, PS_quad() ) );
	}
}

technique11 _circle
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_5_0, VS() ) );
		SetGeometryShader( CompileShader( gs_5_0, GS() ) );
		SetPixelShader( CompileShader( ps_5_0, PS_circle() ) );
	}
}

technique11 _texture
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_5_0, VS() ) );
		SetGeometryShader( CompileShader( gs_5_0, GS() ) );
		SetPixelShader( CompileShader( ps_5_0, PS_texture() ) );
	}
}


