#include "Skinning.fxh"

float alpha = 1;

float3 vLightDirection = normalize(float3(1,1,1));

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
	float L          : TEXCOORD1;
};

float ToonThresholds[2] = { 0.8, 0.4 };
float ToonBrightnessLevels[3] = { 1.3, 0.9, 0.5 };


// Vertex shader: vertex lighting, one bone.
ToonVSOutput VSSkinnedVertexLightingOneBone(VSInputNmTxWeights vin)
{
    Skin(vin, 1);
	
    ToonVSOutput output;
    
    output.TexCoord = vin.TexCoord;
    output.PositionPS = mul(vin.Position, WorldViewProj);

	float3 worldNormal = mul(vin.Normal, World);
	output.L = dot(worldNormal, vLightDirection);
    
    return output;
}


// Vertex shader: vertex lighting, two bones.
ToonVSOutput VSSkinnedVertexLightingTwoBones(VSInputNmTxWeights vin)
{
    
    Skin(vin, 2);
	
    ToonVSOutput output;
    
    output.TexCoord = vin.TexCoord;
    output.PositionPS = mul(vin.Position, WorldViewProj);

	float3 worldNormal = mul(vin.Normal, World);
	output.L = dot(worldNormal, vLightDirection);
    
    return output;
}


// Vertex shader: vertex lighting, four bones.
ToonVSOutput VSSkinnedVertexLightingFourBones(VSInputNmTxWeights vin)
{
    
    Skin(vin, 4);

    ToonVSOutput output;
    
    output.TexCoord = vin.TexCoord;
    output.PositionPS = mul(vin.Position, WorldViewProj);

	float3 worldNormal = mul(vin.Normal, World);
	output.L = dot(worldNormal, vLightDirection);
    
    return output;
}

// Pixel shader: vertex lighting.
float4 PSSkinnedVertexLighting(ToonVSOutput pin) : SV_Target0
{
	float4 Color = SAMPLE_TEXTURE(Texture, pin.TexCoord);
	
    float light;

    if (pin.L> ToonThresholds[0])
        light = ToonBrightnessLevels[0];
    else if (pin.L > ToonThresholds[1])
        light = ToonBrightnessLevels[1];
    else
        light = ToonBrightnessLevels[2];
                
    Color.rgb *= light;
    Color.a = alpha;

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
NormalDepthVSOutput VSDepthOneBone(VSInputNmTxWeights vin)
{
	Skin(vin, 1);

    NormalDepthVSOutput output;
	
    output.Position = mul(vin.Position, WorldViewProj);
    float3 worldNormal = mul(vin.Normal, World);

    // The output color holds the normal, scaled to fit into a 0 to 1 range.
    output.Color.rgb = (worldNormal + 1) / 2;

    // The output alpha holds the depth, scaled to fit into a 0 to 1 range.
    output.Color.a = output.Position.z / output.Position.w;
    
    return output;    
}

NormalDepthVSOutput VSDepthTwoBone(VSInputNmTxWeights vin)
{
	Skin(vin, 2);

    NormalDepthVSOutput output;
	
    output.Position = mul(vin.Position, WorldViewProj);
    float3 worldNormal = mul(vin.Normal, World);
    output.Color.rgb = (worldNormal + 1) / 2;
    output.Color.a = output.Position.z / output.Position.w;
    
    return output;
}

NormalDepthVSOutput VSDepthFourBone(VSInputNmTxWeights vin)
{
	Skin(vin, 4);

    NormalDepthVSOutput output;
	
    output.Position = mul(vin.Position, WorldViewProj);
    float3 worldNormal = mul(vin.Normal, World);
    output.Color.rgb = (worldNormal + 1) / 2;
    output.Color.a = output.Position.z / output.Position.w;
    
    return output;
}

// Simple pixel shader for rendering the normal and depth information.
float4 PSDepth(float4 color : COLOR0) : COLOR0
{
    return color;
}





VertexShader VSArray[3] =
{
    compile vs_2_0 VSSkinnedVertexLightingOneBone(),
    compile vs_2_0 VSSkinnedVertexLightingTwoBones(),
    compile vs_2_0 VSSkinnedVertexLightingFourBones(),
};

VertexShader VSArrayDepth[3] =
{
    compile vs_2_0 VSDepthOneBone(),
    compile vs_2_0 VSDepthTwoBone(),
    compile vs_2_0 VSDepthFourBone(),
};

int VSIndices[3] =
{
	0,      //vertex lighting, one bone
    1,      // vertex lighting, two bones
    2,      // vertex lighting, four bones
};

int ShaderIndex = 0;

Technique Toon
{
    Pass
    {
        VertexShader = (VSArray[VSIndices[ShaderIndex]]);
        PixelShader  = compile ps_2_0 PSSkinnedVertexLighting();
    }
}


// Technique draws the object as normal and depth values.
technique NormalDepth
{
    pass P0
    {
        VertexShader = (VSArrayDepth[VSIndices[ShaderIndex]]);
        PixelShader = compile ps_2_0 PSDepth();
    }
}

