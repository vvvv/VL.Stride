
StructuredBuffer<uint> AirCS__Emit_EmissionRateCoef; // default 0
AppendStructuredBuffer<uint> AirCS__Emit_PtEmitterID : BACKBUFFER;

// =============================================================================
// CS ==========================================================================
// =============================================================================

[numthreads(1, 1, 1)]
void CS_PtEmitterID( uint3 DTid : SV_DispatchThreadID )
{	
	int Coef = AirCS__Emit_EmissionRateCoef[DTid.x];
		
	if(Coef>0)
	{
		for (int i=0; i<Coef; i++)
		{
			AirCS__Emit_PtEmitterID.Append(DTid.x);	
		}
	}
}

// =============================================================================
// TECHNIQUES ==================================================================
// =============================================================================

technique11 PtEmitterID
{
	pass P0
	{
		SetComputeShader( CompileShader( cs_5_0, CS_PtEmitterID() ) );
	}
}



