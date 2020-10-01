float4x4 tWVP: WORLDVIEWPROJECTION ;
float tessFactor = 1;
float4 cAmb <bool color=true; string uiname="Color"; > = { 0.2,0.2,0.2,1.0f};

struct VSSceneIn
{
    float2 pos : POSITION;
};

struct VS_OUTPUT
{
    float2 pos : CPOINT;
};

struct HS_CONSTANT_OUTPUT
{
    float edges[2] : SV_TessFactor;
};

struct HS_OUTPUT
{
    float2 pos : CPOINT;
};

struct DS_OUTPUT
{
    float4 pos : SV_POSITION;
};

VS_OUTPUT VS(VSSceneIn input)
{
	//Just pass trough control point
	VS_OUTPUT p = (VS_OUTPUT)0;
	p.pos = input.pos;
    return p;
}

HS_CONSTANT_OUTPUT HSConst()
{
	//Set tesselation factor
    HS_CONSTANT_OUTPUT output;
    output.edges[0] = 1.0f;
    output.edges[1] = tessFactor;

    return output;
}

/* We want to render lines */
[domain("isoline")]
[partitioning("fractional_even")]
[outputtopology("line")]
[outputcontrolpoints(3)]
[patchconstantfunc("HSConst")]
HS_OUTPUT HS(InputPatch<VS_OUTPUT, 3> ip, uint id : SV_OutputControlPointID)
{
	//Pass the control points around
    HS_OUTPUT output;
    output.pos = ip[id].pos;
    return output;
}

[domain("isoline")]
DS_OUTPUT DS(HS_CONSTANT_OUTPUT input, OutputPatch<HS_OUTPUT, 3> op, float2 uv : SV_DomainLocation)
{
    DS_OUTPUT output;

	/*Here we only need the x component (from 0 to 1) 
	of the DomainLocation (which we can use to compute bezier)*/
    float t = uv.x;
	
	//Compute bezier from quadratic formula
	float2 pos = pow(1.0f - t, 2.0f) * op[0].pos 
	+ 2.0f * (1.0f - t) * t * op[1].pos 
	+ (t*t)*op[2].pos ;

	//Project in screen space (if we have a camera
    output.pos = mul(float4(pos,0.0f, 1.0f), tWVP);

    return output;
}

float4 PS(DS_OUTPUT input) : SV_Target
{   
	return cAmb;
}

technique11 Render
{
	pass P0
	{
		SetHullShader( CompileShader( hs_5_0, HS() ) );
		SetDomainShader( CompileShader( ds_5_0, DS() ) );
		SetVertexShader( CompileShader( vs_5_0, VS() ) );
		SetPixelShader( CompileShader( ps_5_0, PS() ) );
	}
}
