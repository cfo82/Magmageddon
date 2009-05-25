void BlendWithShadow(inout float4 color, in float4 positionWS, in float4 positionLS, in float3 normal)
{
	// compute position where to sample
	float2 projectedTexCoords;	
	projectedTexCoords[0] = (positionLS.x / positionLS.w / 2.0f) + 0.5f;
	projectedTexCoords[1] = (-positionLS.y / positionLS.w / 2.0f) + 0.5f;
	
	// compute depth
	float depth = tex2D(ShadowSampler, projectedTexCoords).r;
	
	// compute distance to light source, assuming directional light in (0,-1,0) direction
	float len = positionWS.y;
	float depthBias = 12.0;
	
	// determine whether or not to draw the shadow
	float cosIncidentAngle = normalize(normal).y;
	bool angleBigEnough = cosIncidentAngle>0.4;
	bool isInShadow = (depth-(len + depthBias))>0;	
	bool drawShadow = angleBigEnough * isInShadow;
	
	color.xyz = drawShadow ? color.xyz * ShadowOpacity : color.xyz;
}