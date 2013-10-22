/*
 * Tutorial
 * XNA Shader programming
 * www.gamecamp.no
 * 
 * by: Petri T. Wilhelmsen
 * e-mail: petriw@gmail.com
 * 
 * Feel free to ask me a question, give feedback or correct mistakes!
 * 
 */


// Global variables
float4x4 World;
float4x4 ViewProj;
float4x4 InverseWorld;
float3 vLightDirection = normalize(float3(1,1,1));

texture ColorMap;
sampler ColorMapSampler = sampler_state
{
   Texture = <ColorMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;
   AddressU  = Clamp;
   AddressV  = Clamp;
};

// The texture that contains the celmap
texture CelMap;
sampler2D CelMapSampler = sampler_state
{
	Texture	  = <CelMap>;
	MIPFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
};

struct OUT
{
	float4 Pos	: POSITION;
	float2 Tex	: TEXCOORD0;
	float3 L	: TEXCOORD1;
	float3 N	: TEXCOORD2;
};

OUT VertexShaderGo( float4 Pos: POSITION, float2 Tex : TEXCOORD, float3 N: NORMAL )
{
	OUT Out = (OUT) 0;
	Out.Pos = mul(mul(Pos, World), ViewProj);
	Out.Tex = Tex;
	Out.L = normalize(vLightDirection);
	Out.N = normalize(mul(InverseWorld, N));
	
	return Out;
}

float4 PixelShaderGo(float2 Tex: TEXCOORD0,float3 L: TEXCOORD1, float3 N: TEXCOORD2) : COLOR
{
	// Calculate normal diffuse light but use Tex.x as color in stead.
	float4 Color = tex2D(ColorMapSampler, Tex);	
	float Ai = 0.8f;
	float4 Ac = float4(0.075, 0.075, 0.2, 1.0);
	float Di = 1.0f;
	float4 Dc = float4(1.0, 1.0, 1.0, 1.0);
	
	Tex.y = 0.0f;
	Tex.x = saturate(dot(L, N));
	
	float4 CelColor = tex2D(CelMapSampler, Tex);
	
	return (Ai*Ac*Color)+(Color*Di*CelColor);
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
NormalDepthVSOutput VSDepth(NormalDepthVSInput vin)
{

    NormalDepthVSOutput output;
	
    output.Position = mul(mul(vin.Position, World), ViewProj);
    float3 worldNormal = mul(vin.Normal, World);

    // The output color holds the normal, scaled to fit into a 0 to 1 range.
    output.Color.rgb = (worldNormal + 1) / 2;

    // The output alpha holds the depth, scaled to fit into a 0 to 1 range.
    output.Color.a = output.Position.z / output.Position.w;
    
    return output;    
}

// Simple pixel shader for rendering the normal and depth information.
float4 PSDepth(float4 color : COLOR0) : COLOR0
{
    return color;
}

technique Toon
{
	pass P0
	{
		Sampler[0] = (ColorMapSampler);	
		Sampler[1] = (CelMapSampler);	
		
		VertexShader = compile vs_2_0 VertexShaderGo();
		PixelShader = compile ps_2_0 PixelShaderGo();
	}
}

// Technique draws the object as normal and depth values.
technique NormalDepth
{
    pass P0
    {
        VertexShader = compile vs_2_0 VSDepth();
        PixelShader = compile ps_2_0 PSDepth();
    }
}