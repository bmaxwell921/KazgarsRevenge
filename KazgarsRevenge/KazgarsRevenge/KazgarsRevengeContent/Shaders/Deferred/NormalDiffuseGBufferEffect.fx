#include "../Common/Structures.fxh"
#include "../Common/NormalEncoding.fxh"

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;

float3 EmissiveColor = float3(0,0,0);
float SpecularIntensity = .5f;
float SpecularPower = 0.1f;

texture Tex;

sampler diffuseSampler
{
	Texture=<Tex>;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

GBufferVSOutput_Common GBufferVS_DEFAULT(GBufferVSInput_Bi_Ta input)
{
    GBufferVSOutput_Common output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.NormalWS = mul(input.Normal,World);
	output.TexCoord = input.TexCoord;

	output.Depth.xy = output.Position.zw;

    return output;
}

GBufferPSOutput GBufferPS_DEFAULT(GBufferVSOutput_Common input)
{
	GBufferPSOutput output;

	output.Albedo.rgb = tex2D(diffuseSampler, input.TexCoord);
	output.Albedo.a = SpecularIntensity;

	float3 normalValue = input.NormalWS;

	output.Normal = 0;
	output.Normal.xy = encodeNormals(normalValue);

	//Sample emissive color
	output.Highlights.rgb = EmissiveColor;
	output.Highlights.a = SpecularPower;

	output.Depth = input.Depth.x / input.Depth.y;

	return output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 GBufferVS_DEFAULT();
        PixelShader = compile ps_2_0 GBufferPS_DEFAULT();
    }
}
