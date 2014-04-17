#include "Skinning.fxh"

float alpha = 1;
float lineIntensity = 1;
float3 lineColor = float3(0,0,0);
float3 colorTint = float3(1,1,1);

float3 playerLightPosition = float3(0,0,0);
float3 playerLightColor = float3(1,1,1);
float3 lightPositions[5];
float3 lightColors[5];

// The texture that contains the celmap
texture CelMap;
sampler2D CelMapSampler = sampler_state
{
	Texture	  = <CelMap>;
	MIPFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
};

struct ToonVSOutput
{
    float2 TexCoord   : TEXCOORD0;
    float4 PositionPS : SV_Position;
	float3 normal     : TEXCOORD1;
	float3 worldPos   : TEXCOORD2;
};

float ToonThresholds[2] = { 0.8, 0.4 };
float ToonBrightnessLevels[3] = { 1.3, 0.9, 0.4 };

// Vertex shader: vertex lighting, four bones.
ToonVSOutput VSToon(VSInputNmTxWeights vin)
{
	Skin(vin, 4);

    ToonVSOutput output;
    
    output.TexCoord = vin.TexCoord;
    output.PositionPS = mul(vin.Position, WorldViewProj);

	output.worldPos = mul(vin.Position, World);
	output.normal = mul(vin.Normal, World);
    
    return output;
}

// Pixel shader: vertex lighting.
float4 PSToonPointLight(ToonVSOutput pin) : SV_Target0
{
	float4 Color = SAMPLE_TEXTURE(Texture, pin.TexCoord);
	
	float highestAmt = 0;
	float tmp;
	float tmp2;
	float light = 0;
	float3 totalColor = float3(0,0,0);
	
	float amts[5];
	int i;
	for(i=0; i<5; ++i)
	{
		tmp =  1 - saturate(distance(lightPositions[i], pin.worldPos) / 300);
		light += tmp;
		amts[i] = tmp;

		tmp2 = tmp;
		if(tmp2 > highestAmt)
		{
			highestAmt = tmp2;
			totalColor = lightColors[i];
		}
	}

	//player light location
	float3 lightDir = normalize(playerLightPosition - pin.worldPos);
	tmp = saturate(dot(pin.normal, lightDir));
	tmp *= 1 - pow(saturate(distance(playerLightPosition, pin.worldPos) / 300), 2);
	light += tmp;

	//make the light's color become blacker as it falls off
	totalColor *= highestAmt / light;

	float playerLightAmt = max(0, tmp / light - highestAmt / light);
	totalColor += playerLightColor * playerLightAmt;
	
    if (light> ToonThresholds[0])
	{
        light = ToonBrightnessLevels[0];
	}
    else if (light > ToonThresholds[1])
	{
        light = ToonBrightnessLevels[1];
	}
    else
	{
        light = ToonBrightnessLevels[2];
    }
	
    Color.rgb *= light * totalColor * colorTint;
    Color.a = min(alpha, Color.a);

    return Color;
}



//
//this effect is drawn onto a rendertarget, which is used as an input for the edge detection shader
//

// Vertex shader input structure.
struct NormalDepthVSInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};

// Output structure for the vertex shader that renders normal and depth information.
struct NormalDepthVSOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};


// Alternative vertex shader outputs normal and depth values, which are then
// used as an input for the edge detection shader
NormalDepthVSOutput VSDepth(VSInputNmTxWeights vin)
{
	Skin(vin, 4);

    NormalDepthVSOutput output;
	
    output.Position = mul(vin.Position, WorldViewProj);
    output.Color.rgb = lineColor;
    output.Color.a = lerp(1, output.Position.z / output.Position.w, lineIntensity);
    
    return output;
}

float4 PSDepth(float4 color : COLOR0) : COLOR0
{
    return color;
}




int ShaderIndex = 0;

Technique Toon
{
    Pass
    {
        VertexShader = compile vs_3_0 VSToon();
        PixelShader  = compile ps_3_0 PSToonPointLight();
    }
}


// Technique draws the object as normal and depth values.
technique NormalDepth
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSDepth();
        PixelShader = compile ps_3_0 PSDepth();
    }
}

