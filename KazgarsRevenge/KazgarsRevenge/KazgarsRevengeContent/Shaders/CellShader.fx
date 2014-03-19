float alpha = 1;
float lineIntensity = 1;

float3 lightPositions[30];
float LightAttenuation = 300;
float LightFalloff = 2;
float3 lightColors[30];

float3 colorTint = float3(1,1,1);

float ToonThresholds[2] = { 0.8, 0.4 };
float ToonBrightnessLevels[3] = { 1.3, 0.9, 0.65 };

// Global variables
float4x4 World;
float4x4 ViewProj;
float4x4 InverseWorld;
float3 vLightDirection = normalize(float3(1,0,1));

texture ColorMap;
sampler ColorMapSampler = sampler_state
{
   Texture = <ColorMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;
   AddressU  = Wrap;
   AddressV  = Wrap;
};

struct ToonVSOutput
{
    float2 TexCoord   : TEXCOORD0;
    float4 PositionPS : SV_Position;
	float3 normal     : TEXCOORD1;
	float3 worldPos   : TEXCOORD2;
};





// Vertex shader: vertex lighting, four bones.
ToonVSOutput ToonVS( float4 Pos: POSITION, float2 Tex : TEXCOORD, float3 N: NORMAL)
{

    ToonVSOutput output;
    
    output.TexCoord = Tex;
    output.PositionPS = mul(mul(Pos, World), ViewProj);

	output.worldPos = mul(Pos, World);
	output.normal = mul(N, World);
    
    return output;
}

// Pixel shader: vertex lighting.
float4 ToonPS(ToonVSOutput pin) : COLOR
{
	float4 Color = tex2D(ColorMapSampler, pin.TexCoord);
	
	
	
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
	
	/*
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
	*/

	light += float3(.15f, .15f, .15f);

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
// used as an input for the edge detection filter in PostprocessEffect.fx.
NormalDepthVSOutput VSDepth(NormalDepthVSInput vin)
{

    NormalDepthVSOutput output;
	
    output.Position = mul(mul(vin.Position, World), ViewProj);
    float3 worldNormal = mul(vin.Normal, World);

    // The output color holds the normal, scaled to fit into a 0 to 1 range.
    output.Color.rgb = float3(0,0,0);

    // The output alpha holds the depth, scaled to fit into a 0 to 1 range.
    output.Color.a = lerp(1, output.Position.z / output.Position.w, lineIntensity);
    
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
		
		VertexShader = compile vs_3_0 ToonVS();
		PixelShader = compile ps_3_0 ToonPS();
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