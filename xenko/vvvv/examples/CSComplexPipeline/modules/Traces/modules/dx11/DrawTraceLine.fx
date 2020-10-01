//@author: dottore
//@help: ringBuffer to traces GS
//@tags: traces
//@credits: 

cbuffer cbForce : register( b1 )
{
	float4x4 tW : WORLD ;
	float4x4 tVP : VIEWPROJECTION ;

	float thickness = 0.1;
	int ringLength = 1;
	
	float currentTime;
	float4 fadeInOut; //x=fadeIn start; y=fadeIn finish; z=fadeOut start; w=fadeOut finish

	float3 RGBscale = 0.5;
	float3 RGBoffset = 0.5;
};


struct data
{
	float3 pos;
	float bornTime;
};
StructuredBuffer<data> ringBufferData;
int ringBufferPhase;


struct VS_IN
{
	uint iv : SV_VertexID;
};

struct VS_OUT
{
    float4 PosW: SV_POSITION ;
	float3 dir : NORMAL ;
    int traceSegmID: TEXCOORD0 ;
	float size : TEXCOORD1 ;
};

VS_OUT VS(VS_IN input)
{
    //inititalize all fields of output struct with 0
    VS_OUT Out = (VS_OUT)0;
	
	uint traceID = input.iv/ringLength;
	uint traceSegmID = input.iv % ringLength+1;
	int id = (traceSegmID + ringBufferPhase)% ringLength + traceID*ringLength;

    //position (projected)
    Out.PosW  = mul(float4(ringBufferData[id].pos,1),tW);
    Out.dir = normalize(mul(Out.PosW-float4(ringBufferData[id+1].pos,0), tVP)).xyz;
	Out.traceSegmID = traceSegmID;

	float lifeTime = currentTime - ringBufferData[id].bornTime ;

	Out.size = 	smoothstep(fadeInOut.x,fadeInOut.y,lifeTime) *
				smoothstep(fadeInOut.w,fadeInOut.z,lifeTime);
	
    return Out;
}

[maxvertexcount(4)]
void GS(line VS_OUT In[2], inout TriangleStream<VS_OUT> TriangleOutputStream)
{
    VS_OUT Out;
    
    //
    // Emit two new triangles
    //
	
	float4 pos1 = In[0].PosW ;
	float4 pos2 = In[1].PosW ;
	float3 dir1 = In[0].dir;//mul(float4(In[0].dir,0), tV).xyz;
	float3 dir2 = In[1].dir;//mul(float4(In[1].dir,0), tV).xyz;
	int traceSegmID1 = In[0].traceSegmID;
	int traceSegmID2 = In[1].traceSegmID;
	
	if(traceSegmID2 != 1)// pos1=pos2;
	{
	
	    for(int i=0; i<2; i++)
	    {
	     	float edge = i ?  -1.0f : 1.0f ;
	        float2 offset = 0;
	   	
	    	// Point 0:
	
	    	float angle = atan2(dir1.x , dir1.y);
	    	offset.x = cos(angle) * edge;
	    	offset.y = -sin(angle) * edge;
	    	offset *= thickness * In[0].size;    	
	    	
			Out.PosW = mul(pos1, tVP) + float4(offset,0,0);
	    	Out.dir = In[0].dir;
	    	Out.traceSegmID = traceSegmID1;
	    	Out.size = In[0].size;
	        
	        TriangleOutputStream.Append(Out);
	    	
	    	// Point 1:
	        offset = 0;
	    	angle = atan2(dir2.x , dir2.y);
	    	offset.x = cos(angle) * edge;
	    	offset.y = -sin(angle) * edge;
	    	offset *= thickness * In[1].size;
	
			Out.PosW = mul(pos2, tVP)+ float4(offset,0,0);
	    	Out.dir = In[1].dir;
	    	Out.traceSegmID = traceSegmID2;
	    	Out.size = In[1].size;
	        
	        TriangleOutputStream.Append(Out);
	    }
	}
    TriangleOutputStream.RestartStrip();
}
/*
[maxvertexcount(4)]
void GS_Line(line VS_OUT input[2], inout TriangleStream<VS_OUT> TriangleOutputStream)
{
    VS_OUT output;
    
    //
    // Emit two new triangles
    //
	
	float4 pos1 = input[0].PosW ;
	float4 pos2 = input[1].PosW ;
	float3 dir1 = mul(float4(input[0].dir,0), tV).xyz;
	float3 dir2 = mul(float4(input[1].dir,0), tV).xyz;
	int traceSegmID1 = input[0].traceSegmID;
	int traceSegmID2 = input[1].traceSegmID;
	
	if(traceSegmID2 != 1)// pos1=pos2;
	{
	
	    for(int i=0; i<2; i++)
	    {
	     	float edge = i ?  -1.0f : 1.0f ;
	        float2 offset = 0;
	   	
	    	// Point 0:
	
	    	float angle = atan2(dir1.x , dir1.y);
	    	offset.x = cos(angle) * edge;
	    	offset.y = -sin(angle) * edge;
	    	offset *= thickness;    	
	    	
			output.PosW = mul(pos1 + float4(offset,0,0), tP);
	    	output.dir = input[0].dir;
	    	output.traceSegmID = traceSegmID1;
	        
	        TriangleOutputStream.Append(output);
	    	
	    	// Point 1:
	        offset = 0;
	    	angle = atan2(dir2.x , dir2.y);
	    	offset.x = cos(angle) * edge;
	    	offset.y = -sin(angle) * edge;
	    	offset *= thickness;
	
			output.PosW = mul(pos2 + float4(offset,0,0), tP);
	    	output.dir = input[1].dir;
	    	output.traceSegmID = traceSegmID2;
	        
	        TriangleOutputStream.Append(output);
	    }
	}
    TriangleOutputStream.RestartStrip();
}
*/

float4 PS_Tex(VS_OUT In): SV_Target
{
    return float4(In.dir*RGBscale*0.5+0.5+RGBoffset,1)* In.size;
}



technique10 Constant
{
	pass P0
	{
		SetHullShader( 0 );
		SetDomainShader( 0 );
		SetVertexShader( CompileShader( vs_5_0, VS() ) );
		SetGeometryShader( CompileShader( gs_5_0, GS() ) );
		SetPixelShader( CompileShader( ps_5_0, PS_Tex() ) );
	}
}
/*
technique10 ConstantLine
{
	pass P0
	{
		SetHullShader( 0 );
		SetDomainShader( 0 );
		SetVertexShader( CompileShader( vs_5_0, VS_line() ) );
		//SetGeometryShader( CompileShader( gs_5_0, GS_Line() ) );
		SetPixelShader( CompileShader( ps_5_0, PS_Tex() ) );
	}
}

*/