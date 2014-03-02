// Settings controlling the edge detection filter.
float EdgeWidth = 1;
float EdgeIntensity = 1;

float DepthThreshold = .018;

// Pass in the current screen resolution.
float2 ScreenResolution;


// This texture contains the main scene image, which the edge detection
// and/or sketch filter are being applied over the top of.
texture SceneTexture;

sampler SceneSampler : register(s0) = sampler_state
{
    Texture = (SceneTexture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};


// This texture contains normals (in the color channels) and depth (in alpha)
// for the main scene image. Differences in the normal and depth data are used
// to detect where the edges of the model are.
texture NormalDepthTexture;

sampler NormalDepthSampler : register(s1) = sampler_state
{
    Texture = (NormalDepthTexture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

// Pixel shader applies the edge detection postprocessing.
float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the original color from the main scene.
    float3 scene = tex2D(SceneSampler, texCoord);
    
    
    // Look up four values from the normal/depth texture, offset along the
    // four diagonals from the pixel we are currently shading.
    float2 edgeOffset = EdgeWidth / ScreenResolution;
        
    float4 n1 = tex2D(NormalDepthSampler, texCoord + float2(-1, -1) * edgeOffset);
    float4 n2 = tex2D(NormalDepthSampler, texCoord + float2( 1,  1) * edgeOffset);
    float4 n3 = tex2D(NormalDepthSampler, texCoord + float2(-1,  1) * edgeOffset);
    float4 n4 = tex2D(NormalDepthSampler, texCoord + float2( 1, -1) * edgeOffset);

    // Work out how much the normal and depth values are changing.
    float4 diagonalDelta = abs(n1 - n2) + abs(n3 - n4);
    float depthDelta = diagonalDelta.w;

	if(depthDelta > DepthThreshold)
	{
		float4 n0 = tex2D(NormalDepthSampler, texCoord);

		float3 col = float3(0,0,0);
		col.r = n0.r + n1.r + n2.r + n3.r + n4.r;
		col.b = n0.b + n1.b + n2.b + n3.b + n4.b;
		col.g = n0.g + n1.g + n2.g + n3.g + n4.g;
		return float4(col,1);
	}
	else
	{
		return float4(scene, 1);
	}
}


// Compile the pixel shader for doing edge detection without any sketch effect.
technique EdgeDetect
{
    pass P0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
