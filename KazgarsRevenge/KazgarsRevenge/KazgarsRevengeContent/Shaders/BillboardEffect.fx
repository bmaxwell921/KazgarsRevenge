float4x4 World;
float4x4 View;
float4x4 Projection;
float3 colorTint = float3(1,1,1);
float alpha = 1;

texture Texture;

sampler Sampler = sampler_state
{
    Texture = (Texture);
    
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
    
    AddressU = Mirror;
    AddressV = Mirror;
};

struct VSIn
{
    float4 Position : SV_Position;
    float3 Normal   : NORMAL;
    float2 TexCoord : TEXCOORD0;
    float Time : TEXCOORD1;
};

struct VSOut
{
    float4 Position : POSITION0;
    float2 TexCoord   : TEXCOORD0;
};

VSOut VS(VSIn input)
{
    VSOut output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.TexCoord = input.TexCoord;

    return output;
}

float4 PS(VSOut input) : COLOR0
{
	return tex2D(Sampler, input.TexCoord) * float4(colorTint, alpha);
}

technique Technique0
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}
