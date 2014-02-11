#include "../../Common/Structures.fxh"
#include "../../Common/NormalEncoding.fxh"

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;

float3 DiffuseColor = float3(1,1,1);
float3 EmissiveColor = float3(0.3,0,0);
float SpecularIntensity = 1.0f;
float SpecularPower = 0.5f;

texture DiffuseTexture;

sampler diffuseSampler
{
	Texture=DiffuseTexture;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

GBufferVSOutput_Common GBufferVS_DEFAULT(GBufferVSInput_Common input)
{
	GBufferVSOutput_Common output;

	output.Position = mul(mul(mul(input.Position,World),View),Projection);
	output.NormalWS = mul(input.Normal, World);
	output.Depth.xy = output.Position.zw;

	output.TexCoord = input.TexCoord;

	return output;
}

GBufferPSOutput GBufferPS_DEFAULT(GBufferVSOutput_Common input)
{
	GBufferPSOutput output;

	output.Albedo.rgb = DiffuseColor * tex2D(diffuseSampler, input.TexCoord);
	output.Albedo.a = SpecularIntensity;

	output.Normal = 0;
	output.Normal.xy = encodeNormals(normalize(input.NormalWS));

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
