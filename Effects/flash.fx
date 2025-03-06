sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;
float x;
float y;
float intensity;
float baseIntensity;
float aspect;
float colorVal;
// float FlashRadius;  // Controls the expansion
// float FlashIntensity; // Controls the brightness

float random(float2 coords) {
    return frac(sin(dot(coords.xy, float2(12.9898,78.233))) * 43758.5453);
}

float NOISE_GRANULARITY = 0.5/255.0;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);

    float2 center = float2(x, y);

    float2 scaledCoords = float2((coords.x - center.x) * aspect, coords.y - center.y);
    
    float dist = length(scaledCoords);

    // highp vec2 coordinates = gl_FragCoord.xy / resolution;

    // float fragmentColor = mix(0.05, 0.35, 1.0 - coordinates.y);
    
    // fragmentColor += mix(-NOISE_GRANULARITY, NOISE_GRANULARITY, random(coordinates));
    // gl_FragColor = vec4(vec3(fragmentColor), 1.0);
    //

    // Calculate flash intensity
    // float flashIntensity = intensity / (dist+baseIntensity);
    // float flashIntensity = pow(max(0, 1.0 - dist / colorVal), baseIntensity) * intensity;
    float flashIntensity = intensity / (dist+baseIntensity);
    // Create flash color (bright white)
    float4 flashColor = float4(1.0, 1.0, 1.0, flashIntensity);
    
    
    float4 fragmentColor = lerp(color, flashColor, flashIntensity);
    // fragmentColor += lerp(-NOISE_GRANULARITY, NOISE_GRANULARITY, random(coords));
    // Blend flash with original color
    return fragmentColor + lerp(-NOISE_GRANULARITY, NOISE_GRANULARITY, random(coords));
}

technique flash
{
    pass skill3_flash
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}