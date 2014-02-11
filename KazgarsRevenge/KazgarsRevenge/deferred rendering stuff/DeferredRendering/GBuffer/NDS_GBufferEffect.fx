#include "../../Common/Structures.fxh"
#include "../../Common/NormalEncoding.fxh"

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;

float3 DiffuseColor = float3(1,1,1);
float3 EmissiveColor = float3(0,0,0);
float SpecularIntensity = 1.0f;
float SpecularPower = 0.5f;

texture DiffuseTexture;
texture NormalMap;
texture SpecularMap;

sampler diffuseSampler
{
	Texture = DiffuseTexture;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = ANISOTROPIC;
	MinFilter = ANISOTROPIC;
	MipFilter = LINEAR;
};

sampler normalSampler
{
	Texture = NormalMap;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	MipFilter = LINEAR;
};

sampler specularSampler
{
	Texture = SpecularMap;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	MipFilter = LINEAR;
};

GBufferVSOutput_Normal VertexShaderFunction(GBufferVSInput_Bi_Ta input)
{
    GBufferVSOutput_Normal output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.TexCoord = input.TexCoord;

	output.Depth.xy = output.Position.zw;

	output.TangentToWorld[0] = mul(input.Tangent,World);
	output.TangentToWorld[1] = mul(input.Binormal,World);
	output.TangentToWorld[2] = mul(input.Normal,World);

    return output;
}

GBufferPSOutput PixelShaderFunction(GBufferVSOutput_Normal input)
{
	GBufferPSOutput output;

	output.Albedo.rgb = DiffuseColor * tex2D(diffuseSampler, input.TexCoord);
	output.Albedo.a = SpecularIntensity * tex2D(specularSampler, input.TexCoord).r;

	float3 normalValue = (2 * tex2D(normalSampler, input.TexCoord).xyz - 1);
	normalValue = mul(normalValue, input.TangentToWorld);
	normalValue = normalize(normalValue);

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
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
