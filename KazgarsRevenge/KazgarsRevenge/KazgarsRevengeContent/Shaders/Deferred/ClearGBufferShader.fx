#include "../Common/Structures.fxh"

//Cornflower blue.
float3 ClearColor = float3(0.390625,0.58431372,0.929412);

struct ClearVSInput
{
    float4 Position : POSITION0;
};

struct ClearVSOutput
{
    float4 Position : POSITION0;
};

ClearVSOutput ClearBufferVS(ClearVSInput input)
{
    ClearVSOutput output;

	output.Position = input.Position;

    return output;
}

GBufferPSOutput ClearBufferPS(ClearVSOutput input)
{
	GBufferPSOutput output;

	output.Albedo = 0;
	output.Highlights = float4(ClearColor,0);
	output.Depth = 1;
	output.Normal = 0;

    return output;
}

technique ClearGBuffer
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 ClearBufferVS();
        PixelShader = compile ps_2_0 ClearBufferPS();
    }
}