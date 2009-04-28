//-----------------------------------------------------------------------------
// Compute per-pixel lighting.
// When compiling for pixel shader 2.0, the lit intrinsic uses more slots
// than doing this directly ourselves, so we don't use the intrinsic.
// E: Eye-Vector
// N: Unit vector normal in world space
//-----------------------------------------------------------------------------
ColorPair ComputePerPixelLights(float3 E, float3 N, float y)
{
	ColorPair result;
	
	result.Diffuse = AmbientLightColor;
	result.Specular = 0;
	
	// Light0
	float3 L = -DirLight0Direction;
	float3 H = normalize(E + L);
	float dt = max(0,dot(L,N));
    result.Diffuse += DirLight0DiffuseColor * dt;
    if (dt != 0)
		result.Specular += DirLight0SpecularColor * pow(max(0,dot(H,N)), SpecularPower);

	// Light1 supports linear decay of light. This works as follows: At the height
	// of y=0, the light is amplified by the factor DirLight1BottomAmpStrength.
	// Afterwards, it gradually decays until the height of y=DirLight1BottomAmpMaxY
	// where it takes its actual specified strength and remains on that level as the height
	// increases.
	L = -DirLight1Direction;
	H = normalize(E + L);
	dt = max(0,dot(L,N));
	float a = max(1,(DirLight1BottomAmpMaxY-y)/DirLight1BottomAmpMaxY*DirLight1BottomAmpStrength);
    result.Diffuse += DirLight1DiffuseColor * dt * a;
    if (dt != 0)
	    result.Specular += DirLight1SpecularColor * pow(max(0,dot(H,N)), SpecularPower);
    
	// Light2
	L = -DirLight2Direction;
	H = normalize(E + L);
	dt = max(0,dot(L,N));
    result.Diffuse += DirLight2DiffuseColor * dt;
    if (dt != 0)
	    result.Specular += DirLight2SpecularColor * pow(max(0,dot(H,N)), SpecularPower);
    
    result.Diffuse *= DiffuseColor;
    result.Diffuse += EmissiveColor;
    result.Specular *= SpecularColor;
		
	return result;
}


//-----------------------------------------------------------------------------
// Compute fog factor
//-----------------------------------------------------------------------------
float ComputeFogFactor(float d)
{
    return clamp((d - FogStart) / (FogEnd - FogStart), 0, 1) * FogEnabled;
}


//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
inline void ComputeLighting(out ColorPair lightResult, out float3 N, in float4 position, in float3 normal)
{
	float3 posToEye = EyePosition - position.xyz;
	float3 E = normalize(posToEye);
	N = normalize(normal);
	lightResult = ComputePerPixelLights(E, N, position.y);
}

inline void ComputeDiffuseTx(out float4 diffuse, in float2 texCoord, in ColorPair lightResult)
{
	float4 texDiffuseColor = tex2D(DiffuseSampler, texCoord);
	float4 uniDiffuseColor = float4(lightResult.Diffuse * DiffuseColor, Alpha);
	diffuse = texDiffuseColor * uniDiffuseColor;
}


inline void ComputeSpecularTx(out float4 specular, in float2 texCoord, in ColorPair lightResult)
{
	float4 texSpecularColor = tex2D(SpecularSampler, texCoord);
	float4 uniSpecularColor = float4(lightResult.Specular * SpecularColor, Alpha);
	specular = texSpecularColor * uniSpecularColor;
}

inline void ComputeDiffColorUni(out float4 color, in ColorPair lightResult, in float fogFactor)
{
	float4 uniDiffuseColor = float4(lightResult.Diffuse * DiffuseColor, Alpha);
	color = uniDiffuseColor + float4(lightResult.Specular, 0);
	color.rgb = lerp(color.rgb, FogColor, fogFactor);
}

inline void ComputeDiffSpecColorTx(out float4 color, in float2 texCoord, in ColorPair lightResult, in float fogFactor)
{
	float4 diffuse, specular;
	ComputeDiffuseTx(diffuse, texCoord, lightResult);
	ComputeSpecularTx(specular, texCoord, lightResult);	
	color = diffuse + float4(specular.rgb, 0);
	color.rgb = lerp(color.rgb, FogColor, fogFactor);
}