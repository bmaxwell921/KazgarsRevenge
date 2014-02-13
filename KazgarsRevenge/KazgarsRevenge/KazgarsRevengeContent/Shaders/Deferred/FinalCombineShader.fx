float2 halfPixel;
float lightMultiplier = 3.0;

float3 AmbientColor = float3(0.2,0.2,0.2);

texture ColorMap;
texture HighlightMap;
texture LightMap;

sampler colorSampler
{
	Texture = ColorMap;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

sampler lightSampler
{
	Texture = LightMap;
	MagFilter = POINT;
	MinFilter = POINT;
	MipFilter = POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

sampler highlightSampler
{
	Texture = HighlightMap;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

struct FinalCombineVSInput
{
	float4 Position : POSITION0;
	float2 TexCoords : TEXCOORD0;
};

struct FinalCombineVSOutput
{
	float4 Position : POSITION0;
	float2 TexCoords : TEXCOORD0;
};

FinalCombineVSOutput FinalCombineVS(FinalCombineVSInput input)
{
	FinalCombineVSOutput output;

	output.Position = input.Position;
	output.TexCoords = input.TexCoords - halfPixel;

	return output;
}

float4 FinalCombinePS(FinalCombineVSOutput input) : COLOR0
{
	float3 diffuseData = tex2D(colorSampler,input.TexCoords).rgb;
	float4 lightData = tex2D(lightSampler,input.TexCoords);
	float3 highlights = tex2D(highlightSampler,input.TexCoords).rgb;

	float3 light = lightData.rgb;
	float spec = lightData.a;

	float3 ambientContribution = diffuseData * AmbientColor;
	float3 diffuse = diffuseData;

	return float4(ambientContribution + light * diffuse + light * spec + highlights,1);
	//return float4(ambientContribution + light * diffuse + highlights,1);
}

technique FinalCombine
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 FinalCombineVS();
		PixelShader = compile ps_2_0 FinalCombinePS();
	}
}
