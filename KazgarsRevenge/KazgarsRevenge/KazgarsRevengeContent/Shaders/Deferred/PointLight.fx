#include "../Common/CommonLight.fxh"

float3 LightPosition;
float ToonThresholds[2] = { 0.8, 0.4 };
float ToonBrightnessLevels[3] = { 1.3, 0.9, 0.5 };

float4 PixelShaderFunction(LightVertexShaderOutput input) : COLOR0
{
	//Retrieve Normal Value
	int invalid;
	float3 normal = GetNormalWithClip(input.TexCoord, invalid);

	if (!invalid)
		return float4(1,1,1,0);

	//Retrieve Color and Specular Intensity, Power
	float4 colorData = tex2D(colorSampler,input.TexCoord);
	//float specularIntensity = colorData.a;
	//float specularPower = tex2D(highlightSampler,input.TexCoord).a * 255;

	//Calculate world position of point to be lit.
	float4 position = GetFragmentWorldPosition(input.TexCoord);

	//Calculate Diffuse light
	float3 lightVector = normalize(LightPosition - position);
	float NdL = saturate(dot(normal,lightVector));

	
    float light;

    if (NdL> ToonThresholds[0])
        light = ToonBrightnessLevels[0];
    else if (NdL > ToonThresholds[1])
        light = ToonBrightnessLevels[1];
    else
        light = ToonBrightnessLevels[2];
                
	float3 finalColor = light * Color;
	return float4(finalColor, 0);





	//float3 diffuseColor = NdL * Color;

	//Calculate Specular Light
	//float3 directionToCamera = normalize(CameraPosition - position);
	//float3 halfVector = normalize(lightVector + directionToCamera);

	//float NdH = saturate(dot(normal,halfVector));
	//float specularLight = specularIntensity * pow(NdH, specularPower);

	//return float4(diffuseColor,specularLight);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 LightVertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
