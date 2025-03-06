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
float blackPercentage;

float black = 70.0/255.0;
float white = 100.0/255.0;
float gamma = 1.0;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);

    if(uProgress != 1.0)
    {
        return color;
    }
    
    float gs = dot(float3(0.2989, 0.5870, 0.1140), color.rgb);

    // return float4(gs, gs, gs, color.a);
    //apply formula
    float final = (gs-black)/(white-black);
    
    // return float4(final, final, final, color.a);

    final  = pow(final, gamma);
    //     inBlack  = np.array([0, 0, 0], dtype=np.float32)
    // inWhite  = np.array([255, 255, 255], dtype=np.float32)
    // inGamma  = np.array([1.0, 1.0, 1.0], dtype=np.float32)
    // outBlack = np.array([0, 0, 0], dtype=np.float32)
    // outWhite = np.array([255, 255, 255], dtype=np.float32)

    // img = np.clip( (img - inBlack) / (inWhite - inBlack), 0, 255 )                            
    // img = ( img ** (1/inGamma) ) *  (outWhite - outBlack) + outBlack
    // img = np.clip( img, 0, 255).astype(np.uint8)

    
    return float4(final, final, final, color.a);
}

technique greyscale
{
    pass skill3_threshold
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}