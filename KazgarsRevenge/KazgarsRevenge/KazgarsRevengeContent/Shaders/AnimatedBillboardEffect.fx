float4x4 World;
float4x4 View;
float4x4 Projection;
float3 colorTint = float3(1,1,1);

//changing texture coords (sprite animation)
float rows = 1; //corresponds to spritesheet
float cols = 1;
float totalFrames = 1;
float framesPerSecond = 1000;//sprite frames per second

float CurrentTime;

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
	


	//the initial uv coords
    float2 uv = input.TexCoord;

	//size the coords
	//(rescale x to be between 0 and frameWidth, and y to be between 0 and frameHeight)
	uv.x /= cols;
	uv.y /= rows;

	//get the zero-based index of the frame
	float frame = floor((CurrentTime - 0) * framesPerSecond) % totalFrames;

    //get the upper left coordinate of the frame we're on, and add it to the coords so the frame starts there
    uv.x += (frame % cols) / (float)cols;
    uv.y += floor(frame / cols) / (float)rows;

	//pass it to the pixel shader
	output.TexCoord = uv;
    
	return output;
}

float4 PS(VSOut input) : COLOR0
{
	return tex2D(Sampler, input.TexCoord) * float4(colorTint, 1);
}

technique Technique0
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}
