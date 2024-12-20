sampler uImage0 : register(s0);

float AppearFactor;

struct PSInput 
{
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

float4 PixelShaderFunction(PSInput input) : COLOR0 {
    // Sample the texture
    float4 color = tex2D(uImage0, input.TexCoord);

    // Apply appear effect by modulating alpha
    color.a *= AppearFactor;
    return color;
}

technique AppearEffect {
    pass P0 {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}


// float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0 {
//     float4 color = tex2D(uImage0, coords);
//     if (!any(color))
//         return color;
//         // 灰度 = r*0.3 + g*0.59 + b*0.11
//     float gs = dot(float3(0.3, 0.59, 0.11), color.rgb);
//     return float4(gs, gs, gs, color.a);
// }

// technique Technique1 {
//     pass Test {
//         PixelShader = compile ps_2_0 PixelShaderFunction();
//     }
// }