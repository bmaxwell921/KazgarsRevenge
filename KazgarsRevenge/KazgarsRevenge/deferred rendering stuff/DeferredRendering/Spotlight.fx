#include "../Common/CommonLight.fxh"

float3 LightPosition;
float3 LightDirection;
float LightIntensity;
float CosFieldOfView;

float4 PixelShaderFunction(LightVertexShaderOutput input) : COLOR0
{
	//Retrieve Normal Value
	int invalid;
	float3 normal = GetNormalWithClip(input.TexCoord, invalid);

	if (!invalid)
		return float4(1,1,1,0);

	//Retrieve Color and Specular Intensity, Power
	float4 colorData = tex2D(colorSampler,input.TexCoord);
	float specularIntensity = colorData.a;
	float specularPower = tex2D(highlightSampler,input.TexCoord).a * 255;

	//Calculate world position of point to be lit.
	float4 position = GetFragmentWorldPosition(input.TexCoord);

	//Calculate Diffuse light
	float3 lightVector = LightPosition - position.xyz;

	float distance = length(lightVector);
	float attenuation = clamp(LightIntensity / (distance * distance),0,LightIntensity);
	lightVector = normalize(lightVector);
	
	// Cos(angle between LightDirection,lightVector)
	// If they're exactly the same, then it should be 1.
	// If they're orthogonal, then it will be 0.
	float lightAngle = saturate(dot(LightDirection,-lightVector));
	if (lightAngle < CosFieldOfView)
		discard;

	float spotAttenuation = (lightAngle - CosFieldOfView) / (1 - CosFieldOfView);
	float NdL = saturate( spotAttenuation *  dot(normal,lightVector));
	

	float3 diffuseColor = NdL * Color;

	//Calculate Specular Light
	float3 directionToCamera = normalize(CameraPosition - position);
	float3 halfVector = normalize(lightVector + directionToCamera);

	float NdH = saturate(dot(normal,halfVector));
	float specularLight = specularIntensity * pow(NdH, specularPower);

	return attenuation * float4(diffuseColor,specularLight);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 LightVertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
