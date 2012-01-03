

// IMPORTANT!!
//
// Some notes to pow! (taken from here http://forums.create.msdn.com/forums/t/32167.aspx)
// -------------
//    When compiling code that looks like the following:
//    pow(max(0, f), e)
//    If the result of max(0,f) is 0, the statement will be evaluated as exp(-inf * e), which may cause the following error message: (error X4579: NaN and infinity literals not allowed by shader model). This is because pow(0, e) will be expanded to exp(log(0) * e), which will evaluated as exp(-inf * e).
//    The workaround for compiling code like this, for a shader model that does not allow NaN or infinity literals, is to change 0 in max(0, f) to 0.00001f, or some other acceptably small non-zero value.

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
		result.Specular += DirLight0SpecularColor * pow(max(NEARLY_NULL_VALUE,dot(H,N)), SpecularPower);

	// Light1 supports linear decay of light. This works as follows: At the height
	// of y=0, the light is amplified by the factor DirLight1BottomAmpStrength.
	// Afterwards, it gradually decays until the height of y=DirLight1BottomAmpMaxY
	// where it takes its actual specified strength and remains on that level as the height
	// increases.
	L = -DirLight1Direction;
	H = normalize(E + L);
	dt = max(0,dot(L,N));
	float heightCoefficient = saturate(1-abs((y-130)/DirLight1BottomAmpMaxY));
	//float a = max(0,(DirLight1BottomAmpMaxY-y)/DirLight1BottomAmpMaxY);
	
	float multiplier = CutLight1 ? lerp(DirLight1MinMultiplier, DirLight1MaxMultiplier, heightCoefficient) : 1;
    result.Diffuse += DirLight1DiffuseColor * dt * multiplier;
    //result.Diffuse += float3(1,1,1)* dt * 1;
    if (dt != 0)
	    result.Specular += DirLight1SpecularColor * pow(max(NEARLY_NULL_VALUE,dot(H,N)), SpecularPower);
    
	// Light2
	L = -DirLight2Direction;
	H = normalize(E + L);
	dt = max(0,dot(L,N));
    result.Diffuse += DirLight2DiffuseColor * dt;
    if (dt != 0)
	    result.Specular += DirLight2SpecularColor * pow(max(NEARLY_NULL_VALUE,dot(H,N)), SpecularPower);
    
    result.Diffuse *= DiffuseColor;
    result.Diffuse += EmissiveColor;
    result.Specular *= SpecularColor;
		
	return result;
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
	color.rgb = lerp(color.rgb, FogColor, fogFactor* FogEnabled);
	color.rgb = lerp(color.rgb, ToneColor, BlinkingState);
}

inline void ComputeDiffSpecColorTx(out float4 color, in float2 texCoord, in ColorPair lightResult, in float fogFactor)
{
	float4 diffuse, specular;
	ComputeDiffuseTx(diffuse, texCoord, lightResult);
	ComputeSpecularTx(specular, texCoord, lightResult);	
	color = diffuse + float4(specular.rgb, 0);
	color.rgb = lerp(color.rgb, FogColor, fogFactor* FogEnabled);
	color.rgb = lerp(color.rgb, ToneColor, BlinkingState);
}
