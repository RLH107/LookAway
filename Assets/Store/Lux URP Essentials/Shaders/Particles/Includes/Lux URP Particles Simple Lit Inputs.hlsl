// Only needed to define our own CBUFFER

#ifndef LIGHTWEIGHT_PARTICLES_SIMPLE_LIT_INPUT_INCLUDED
#define LIGHTWEIGHT_PARTICLES_SIMPLE_LIT_INPUT_INCLUDED

//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Particles.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _SoftParticleFadeParams;
    float4 _CameraFadeParams;
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _EmissionColor;
    half4 _BaseColorAddSubDiff;

//  needed...
    half _Cutoff;

    half4 _SpecColor;
    //half _Smoothness;

    half _DistortionStrengthScaled;
    half _DistortionBlend;

//  Custom inputs
    float _SampleOffset;
    half _Transmission;
    half _TransmissionDistortion;

//  Tessellation
    float _Tess;
    float2 _TessRange;

CBUFFER_END

TEXTURE2D(_SpecGlossMap); SAMPLER(sampler_SpecGlossMap);

float4 _CameraDepthTexture_TexelSize;


#define SOFT_PARTICLE_NEAR_FADE _SoftParticleFadeParams.x
#define SOFT_PARTICLE_INV_FADE_DISTANCE _SoftParticleFadeParams.y

#define CAMERA_NEAR_FADE _CameraFadeParams.x
#define CAMERA_INV_FADE_DISTANCE _CameraFadeParams.y


//  ----------------------------------------------------------------------------------
//  Fixed soft particles single pass instanced

// Soft particles - returns alpha value for fading particles based on the depth to the background pixel
float Lux_SoftParticles(float near, float far, float4 projection)
{
    float fade = 1;
    if (near > 0.0 || far > 0.0)
    {
        //float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, projection.xy / projection.w), _ZBufferParams);
float sceneDepth = LOAD_TEXTURE2D_X_LOD(_CameraDepthTexture, _CameraDepthTexture_TexelSize.zw * (projection.xy / projection.w), 0).x;
float sceneZ = LinearEyeDepth(sceneDepth, _ZBufferParams);
        float thisZ = LinearEyeDepth(projection.z / projection.w, _ZBufferParams);
        fade = saturate (far * ((sceneZ - near) - thisZ));
    }
    return fade;
}

half4 Lux_SampleAlbedo(float2 uv, float3 blendUv, half4 color, float4 particleColor, float4 projectedPosition, TEXTURE2D_PARAM(albedoMap, sampler_albedoMap))
{
    half4 albedo = BlendTexture(TEXTURE2D_ARGS(albedoMap, sampler_albedoMap), uv, blendUv) * color;

    half4 colorAddSubDiff = half4(0, 0, 0, 0);
#if defined (_COLORADDSUBDIFF_ON)
    colorAddSubDiff = _BaseColorAddSubDiff;
#endif
    // No distortion Support
    albedo = MixParticleColor(albedo, particleColor, colorAddSubDiff);

    AlphaDiscard(albedo.a, _Cutoff);

#if defined(_SOFTPARTICLES_ON)
        ALBEDO_MUL *= Lux_SoftParticles(SOFT_PARTICLE_NEAR_FADE, SOFT_PARTICLE_INV_FADE_DISTANCE, projectedPosition);
#endif

 #if defined(_FADING_ON)
     ALBEDO_MUL *= CameraFade(CAMERA_NEAR_FADE, CAMERA_INV_FADE_DISTANCE, projectedPosition);
 #endif

    return albedo;
}



half4 SampleAlbedo(float2 uv, float3 blendUv, half4 color, float4 particleColor, float4 projectedPosition, TEXTURE2D_PARAM(albedoMap, sampler_albedoMap))
{
    half4 albedo = BlendTexture(TEXTURE2D_ARGS(albedoMap, sampler_albedoMap), uv, blendUv) * color;

    half4 colorAddSubDiff = half4(0, 0, 0, 0);
#if defined (_COLORADDSUBDIFF_ON)
    colorAddSubDiff = _BaseColorAddSubDiff;
#endif
    albedo = MixParticleColor(albedo, particleColor, colorAddSubDiff);
    
    AlphaDiscard(albedo.a, _Cutoff);
    
 #if defined(_SOFTPARTICLES_ON)
     ALBEDO_MUL *= SoftParticles(SOFT_PARTICLE_NEAR_FADE, SOFT_PARTICLE_INV_FADE_DISTANCE, projectedPosition);
 #endif
 
 #if defined(_FADING_ON)
     ALBEDO_MUL *= CameraFade(CAMERA_NEAR_FADE, CAMERA_INV_FADE_DISTANCE, projectedPosition);
 #endif

    return albedo;
}

half4 SampleSpecularSmoothness(float2 uv, float3 blendUv, half alpha, half4 specColor, TEXTURE2D_PARAM(specGlossMap, sampler_specGlossMap))
{
    half4 specularGloss = half4(0.0h, 0.0h, 0.0h, 1.0h);
#ifdef _SPECGLOSSMAP
    specularGloss = BlendTexture(TEXTURE2D_ARGS(specGlossMap, sampler_specGlossMap), uv, blendUv);
#elif defined(_SPECULAR_COLOR)
    specularGloss = specColor;
#endif

#ifdef _GLOSSINESS_FROM_BASE_ALPHA
    specularGloss.a = alpha;
#endif
    specularGloss.a = exp2(10 * specularGloss.a + 1);

    return specularGloss;
}

#endif // LIGHTWEIGHT_PARTICLES_SIMPLE_LIT_INPUT_INCLUDED
