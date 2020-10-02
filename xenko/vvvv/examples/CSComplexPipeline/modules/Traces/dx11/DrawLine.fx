//@author: dottore
//@help: ringBuffer to traces GS
//@tags: traces
//@credits: 

cbuffer cbForce : register( b1 )
{
	float4x4 tW : WORLD ;
	float4x4 tVP : VIEWPROJECTION ;

	float thickness = 0.1;
	int lineRes = 1;
	int IndexShift;
	
};


struct data
{
	float3 pos;
	float3 vel;
};
//StructuredBuffer<data> ringBufferData;
StructuredBuffer<float3> ringBufferData;
StructuredBuffer<uint> perEmitterOffset;


struct VS_IN
{
	uint iv : SV_VertexID;
};

struct VS_OUT
{
    float4 PosW: SV_POSITION ;
	float3 dir : NORMAL ;
    int segmID: TEXCOORD0 ;
	float size : TEXCOORD1 ;
};

VS_OUT VS(VS_IN In)
{
    //inititalize all fields of output struct with 0
    VS_OUT Out = (VS_OUT)0;

	uint LineID = In.iv / (lineRes);
	uint segmID = In.iv % (lineRes);
	uint id = (segmID + IndexShift)%lineRes + LineID*lineRes;
	uint id_A = (segmID + IndexShift)%lineRes + LineID*lineRes;
	uint id_B = (segmID+1 + IndexShift)%lineRes + LineID*lineRes;

    //position (projected)
    //Out.PosW  = mul(float4(ringBufferData[id].pos,1),tW);
    Out.PosW  = mul(float4(ringBufferData[id],1),tW);

	//uint groupID = (float)id / (float)lineRes;

	uint idL = segmID;
	uint idR = min(segmID+1, lineRes-1) + LineID * lineRes;


	//float3 posL = ringBufferData[idL].pos;
	//float3 posR = ringBufferData[idR].pos;
	float3 posL = ringBufferData[id_A];
	float3 posR = ringBufferData[id_B];
	float3 dir = posL-posR;
	if(segmID == (uint)lineRes-1) dir *= -1;
	
    Out.dir = normalize(mul(float4(dir,0), tVP)).xyz;

	Out.segmID = In.iv % lineRes;

	Out.size = 	thickness;

    return Out;
}

float aspectRatioXinv = 1;

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
	int segmID1 = In[0].segmID;
	int segmID2 = In[1].segmID;
	
	if(segmID1!=lineRes-1)// don't write if pos1 or pos2 are the first;
	{
	
	    for(int i=0; i<2; i++)
	    {
	     	float edge = i ?  -1.0f : 1.0f ;
	        float2 offset = 0;
	   	
	    	// Point 0:
	
	    	float angle = atan2(dir1.x , dir1.y);
	    	offset.x = cos(angle) * edge * aspectRatioXinv;
	    	offset.y = -sin(angle) * edge;
	    	offset *= In[0].size;    	
	    	
			Out.PosW = mul(pos1, tVP) + float4(offset,0,0);
	    	Out.dir = In[0].dir;
	    	Out.segmID = segmID1;
	    	Out.size = In[0].size;
	        
	        TriangleOutputStream.Append(Out);
	    	
	    	// Point 1:
	        offset = 0;
	    	angle = atan2(dir2.x , dir2.y);
	    	offset.x = cos(angle) * edge * aspectRatioXinv;
	    	offset.y = -sin(angle) * edge;
	    	offset *= In[1].size;
	
			Out.PosW = mul(pos2, tVP)+ float4(offset,0,0);
	    	Out.dir = In[1].dir;
	    	Out.segmID = segmID2;
	    	Out.size = In[1].size;
	        
	        TriangleOutputStream.Append(Out);
	    }
		    TriangleOutputStream.RestartStrip();

	}
}

float4 PS_Tex(VS_OUT In): SV_Target
{
    return 1;//float4(In.dir*RGBscale*0.5+0.5+RGBoffset,1)* In.size;
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
