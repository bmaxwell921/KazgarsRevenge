#include "../Common/CommonLight.fxh"

float3 LightDirection = float3(1,1,1);

float ToonThresholds[2] = { 0.8, 0.4 };
float ToonBrightnessLevels[3] = { 1.3, 0.9, 0.5 };

float4 PixelShaderFunction(LightVertexShaderOutput input) : COLOR0
{
	//Retrieve Normal Value
	int invalid;
	float3 normal = GetNormalWithClip(input.TexCoord, invalid);

	if (!invalid)
		return float4(1,1,1,0);

	//Calculate Diffuse light
	float3 lightVector = -LightDirection;
	float NdL = saturate(dot(normal,lightVector));

	float3 diffuseColor = NdL * Color;


	//return float4(diffuseColor,specularLight);
	//return float4(diffuseColor, 0);

	
    float light;

    if (NdL> ToonThresholds[0])
        light = ToonBrightnessLevels[0];
    else if (NdL > ToonThresholds[1])
        light = ToonBrightnessLevels[1];
    else
        light = ToonBrightnessLevels[2];
                

	float3 finalColor = light * Color;
	return float4(finalColor, 0);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 LightVertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
