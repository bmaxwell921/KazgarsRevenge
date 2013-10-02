#include "Skinning.fxh"



float4x4	matInverseWorld;
float4		vLightDirection;

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
	float3 L          : TEXCOORD1;
	float3 N          : TEXCOORD2;
};



// Vertex shader: vertex lighting, one bone.
ToonVSOutput VSSkinnedVertexLightingOneBone(VSInputNmTxWeights vin)
{
    Skin(vin, 1);

    ToonVSOutput output;
    
    output.PositionPS = mul(vin.Position, WorldViewProj);
	output.L = normalize(vLightDirection);
	output.N = normalize(mul(matInverseWorld, vin.Normal));
    
    output.TexCoord = vin.TexCoord;
    
    return output;
}


// Vertex shader: vertex lighting, two bones.
ToonVSOutput VSSkinnedVertexLightingTwoBones(VSInputNmTxWeights vin)
{
    
    Skin(vin, 2);

    ToonVSOutput output;
    
    output.PositionPS = mul(vin.Position, WorldViewProj);
	output.L = normalize(vLightDirection);
	output.N = normalize(mul(matInverseWorld, vin.Normal));
    
    output.TexCoord = vin.TexCoord;
    
    return output;
}


// Vertex shader: vertex lighting, four bones.
ToonVSOutput VSSkinnedVertexLightingFourBones(VSInputNmTxWeights vin)
{
    
    Skin(vin, 4);

    ToonVSOutput output;
    
    output.PositionPS = mul(vin.Position, WorldViewProj);
	output.L = normalize(vLightDirection);
	output.N = normalize(mul(matInverseWorld, vin.Normal));
    
    output.TexCoord = vin.TexCoord;
    
    return output;
}

// Pixel shader: vertex lighting.
float4 PSSkinnedVertexLighting(ToonVSOutput pin) : SV_Target0
{
	float4 Color = SAMPLE_TEXTURE(Texture, pin.TexCoord);
	float2 celTexCoord = float2(saturate(dot(pin.L, pin.N)), 0.0f);
	float4 CelColor = tex2D(CelMapSampler, celTexCoord);

	float4 Ac = float4(0.075, 0.075, 0.2, 1.0);
	return (Ac*Color)+(Color*CelColor);
}

VertexShader VSArray[3] =
{
    compile vs_2_0 VSSkinnedVertexLightingOneBone(),
    compile vs_2_0 VSSkinnedVertexLightingTwoBones(),
    compile vs_2_0 VSSkinnedVertexLightingFourBones(),
};


int VSIndices[3] =
{
	0,      //vertex lighting, one bone
    1,      // vertex lighting, two bones
    2,      // vertex lighting, four bones
};

int ShaderIndex = 0;

Technique SkinnedEffect
{
    Pass
    {
        VertexShader = (VSArray[VSIndices[ShaderIndex]]);
        PixelShader  = compile ps_2_0 PSSkinnedVertexLighting();
    }
}
