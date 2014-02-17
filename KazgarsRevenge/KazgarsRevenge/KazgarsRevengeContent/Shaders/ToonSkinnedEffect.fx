#include "Skinning.fxh"



float alpha = 1;
float lineIntensity = 1;

float3 lightPositions[30];
float LightAttenuation = 300;
float LightFalloff = 2;
float3 lightColors[30];

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
float ToonBrightnessLevels[3] = { 1.3, 0.9, 0.16 };

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
	
	float light = 0;
	float3 totalColor = float3(1,1,1);
	float curActiveLights = 1;

	float3 lightDir;
	float amt;
	float att;
	float tmp;
	for(int i=0; i<30; ++i)
	{
		if(lightPositions[i].x != -10000)
		{
			//get direction of light
			lightDir = normalize(lightPositions[i] - pin.worldPos);

			//get amount of light on this vertex
			amt = saturate(dot(pin.normal, lightDir));

			//get attenuation
			att = 1 - pow(saturate(distance(lightPositions[i], pin.worldPos) / LightAttenuation), LightFalloff);
		
			//send resulting light to pixel shader
			tmp = amt * att;
			light += tmp;

			totalColor += lightColors[i];

			++curActiveLights;
		}
	}
	
	totalColor /= curActiveLights;

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
	
    Color.rgb *= light * totalColor;
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
// used as an input for the edge detection filter in PostprocessEffect.fx.
NormalDepthVSOutput VSDepth(VSInputNmTxWeights vin)
{
	Skin(vin, 4);

    NormalDepthVSOutput output;
	
    output.Position = mul(vin.Position, WorldViewProj);
    float3 worldNormal = mul(vin.Normal, World);
    output.Color.rgb = ((worldNormal + 1) / 2) * lineIntensity;
    output.Color.a = output.Position.z / output.Position.w;
    
    return output;
}

// Simple pixel shader for rendering the normal and depth information.
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

