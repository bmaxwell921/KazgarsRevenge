#include "../../Common/Structures.fxh"
#include "../../Common/NormalEncoding.fxh"

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;

float3 DiffuseColor = float3(1,1,1);
float3 EmissiveColor = float3(0,0,0);
float SpecularIntensity = 1.0f;
float SpecularPower = 0.5f;

texture NormalMap;

sampler normalSampler
{
	Texture=NormalMap;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

GBufferVSOutput_Normal GBufferVS_DEFAULT(GBufferVSInput_Bi_Ta input)
{
    GBufferVSOutput_Normal output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.TexCoord = input.TexCoord;

	output.Depth.xy = output.Position.zw;

	//output.TangentToWorld = float3x3( input.Tangent , cross(input.Tangent,input.Normal) , input.Normal);
	//output.TangentToWorld = mul(output.TangentToWorld,World);

	output.TangentToWorld[0] = mul(input.Tangent,World);
	output.TangentToWorld[1] = mul(input.Binormal,World);
	output.TangentToWorld[2] = mul(input.Normal,World);
    
	return output;
}

GBufferPSOutput GBufferPS_DEFAULT(GBufferVSOutput_Normal input)
{
	GBufferPSOutput output;

	output.Albedo.rgb = DiffuseColor;
	output.Albedo.a = SpecularIntensity;

	float3 normalValue = normalize(2 * tex2D(normalSampler, input.TexCoord).xyz - 1);
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
        VertexShader = compile vs_2_0 GBufferVS_DEFAULT();
        PixelShader = compile ps_2_0 GBufferPS_DEFAULT();
    }
}
