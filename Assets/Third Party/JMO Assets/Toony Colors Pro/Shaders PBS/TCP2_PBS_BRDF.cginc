#ifndef TCP2_STANDARD_BRDF_INCLUDED
#define TCP2_STANDARD_BRDF_INCLUDED

// TOONY COLORS PRO 2
// Main BRDF function, based on UnityStandardBRDF.cginc

//-------------------------------------------------------------------------------------

// TCP2 Tools

inline half WrapRampNL(half nl, fixed threshold, fixed smoothness)
{
	#ifndef TCP2_DISABLE_WRAPPED_LIGHT
	//TCP2 Note: disabling wrapped lighting to save 1 instruction, else the shader fails to compile on SM2
	  #if SHADER_TARGET >= 30
		nl = nl * 0.5 + 0.5;
	  #endif
	#endif
	#if TCP2_RAMPTEXT
		nl = tex2D(_Ramp, fixed2(nl, nl)).r;
	#else
		nl = smoothstep(threshold - smoothness*0.5, threshold + smoothness*0.5, nl);
	#endif
	
	return nl;
}

inline half StylizedSpecular(half specularTerm, fixed specSmoothness)
{
	return smoothstep(specSmoothness*0.5, 0.5 + specSmoothness*0.5, specularTerm);
}

inline half3 StylizedFresnel(half nv, half roughness, UnityLight light, fixed3 rimParams)
{
	half rim = 1-nv;
	rim = smoothstep(rimParams.x, rimParams.y, rim) * rimParams.z * saturate(1.33-roughness);
	return rim * light.ndotl * light.color;
}

//-------------------------------------------------------------------------------------

// Note: BRDF entry points use oneMinusRoughness (aka "smoothness") and oneMinusReflectivity for optimization
// purposes, mostly for DX9 SM2.0 level. Most of the math is being done on these (1-x) values, and that saves
// a few precious ALU slots.

// Main Physically Based BRDF
// Derived from Disney work and based on Torrance-Sparrow micro-facet model
//
//   BRDF = kD / pi + kS * (D * V * F) / 4
//   I = BRDF * NdotL
//
// * NDF (depending on UNITY_BRDF_GGX):
//  a) Normalized BlinnPhong
//  b) GGX
// * Smith for Visiblity term
// * Schlick approximation for Fresnel
half4 BRDF1_TCP2_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi,
	//TCP2 added properties
	fixed tcp2RampThreshold, fixed tcp2RampSmoothness,
	fixed4 tcp2HighlightColor, fixed4 tcp2ShadowColor,
	fixed tcp2specSmooth, fixed tcp2specBlend,
	fixed3 rimParams,
	half atten)
{
	half roughness = 1-oneMinusRoughness;
	half3 halfDir = Unity_SafeNormalize (light.dir + viewDir);

#if UNITY_BRDF_GGX 
	// NdotV should not be negative for visible pixels, but it can happen due to perspective projection and normal mapping
	// In this case we will modify the normal so it become valid and not cause weird artifact (other game try to clamp or abs the NdotV to prevent this trouble).
	// The amount we shift the normal toward the view vector is define by the dot product.
	// This correction is only apply with smithJoint visibility function because artifact are more visible in this case due to highlight edge of rough surface
	half shiftAmount = dot(normal, viewDir);
	normal = shiftAmount < 0.0f ? normal + viewDir * (-shiftAmount + 1e-5f) : normal;
	// A re-normalization should be apply here but as the shift is small we don't do it to save ALU.
	//normal = normalize(normal);

	// As we have modify the normal we need to recalculate the dot product nl. 
	// Note that  light.ndotl is a clamped cosine and only the ForwardSimple mode use a specific ndotL with BRDF3
	half nl = DotClamped(normal, light.dir);
#else
	half nl = light.ndotl;
#endif
	
	//TCP2 Ramp N.L
	nl = WrapRampNL(nl, tcp2RampThreshold, tcp2RampSmoothness);
	
	half nh = BlinnTerm (normal, halfDir);
	half nv = DotClamped(normal, viewDir);

	half lv = DotClamped (light.dir, viewDir);
	half lh = DotClamped (light.dir, halfDir);

#if UNITY_BRDF_GGX
	half V = SmithJointGGXVisibilityTerm (nl, nv, roughness);
	half D = GGXTerm (nh, roughness);
#else
	half V = SmithBeckmannVisibilityTerm (nl, nv, roughness);
	half D = NDFBlinnPhongNormalizedTerm (nh, RoughnessToSpecPower (roughness));
#endif

	half nlPow5 = Pow5 (1-nl);
	half nvPow5 = Pow5 (1-nv);
	half Fd90 = 0.5 + 2 * lh * lh * roughness;
	half disneyDiffuse = (1 + (Fd90-1) * nlPow5) * (1 + (Fd90-1) * nvPow5);
	
	// HACK: theoretically we should divide by Pi diffuseTerm and not multiply specularTerm!
	// BUT 1) that will make shader look significantly darker than Legacy ones
	// and 2) on engine side "Non-important" lights have to be divided by Pi to in cases when they are injected into ambient SH
	// NOTE: multiplication by Pi is part of single constant together with 1/4 now
	half specularTerm = (V * D) * (UNITY_PI/4); // Torrance-Sparrow model, Fresnel is applied later (for optimization reasons)

#if TCP2_SPEC_TOON
	//TCP2 Stylized Specular
	half r = roughness * 0.85;
	r += 1e-4h;
	specularTerm = lerp(specularTerm, StylizedSpecular(specularTerm, tcp2specSmooth) * (1/r), tcp2specBlend);
#endif
	if (IsGammaSpace())
		specularTerm = sqrt(max(1e-4h, specularTerm));
	specularTerm = max(0, specularTerm * nl);
	
	half diffuseTerm = disneyDiffuse * nl;
	
	// surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(realRoughness^2+1)
	half realRoughness = roughness*roughness;		// need to square perceptual roughness
	half surfaceReduction;
	if (IsGammaSpace()) surfaceReduction = 1.0 - 0.28*realRoughness*roughness;		// 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
	else surfaceReduction = 1.0 / (realRoughness*realRoughness + 1.0);			// fade \in [0.5;1]
	
	//TCP2 Colored Highlight/Shadows
	tcp2ShadowColor = lerp(tcp2HighlightColor, tcp2ShadowColor, tcp2ShadowColor.a);	//Shadows intensity through alpha
	diffuseTerm *= atten;
	half3 diffuseTermRGB = lerp(tcp2ShadowColor.rgb, tcp2HighlightColor.rgb, diffuseTerm);
	half3 diffuseTCP2 = diffColor * (gi.diffuse + light.color * diffuseTermRGB);
	//original: diffColor * (gi.diffuse + light.color * diffuseTerm)
	
	//TCP2: atten contribution to specular since it was removed from light calculation
	specularTerm *= atten;
	
	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));
    half3 color =	diffuseTCP2
                    + specularTerm * light.color * FresnelTerm (specColor, lh)
					+ surfaceReduction * gi.specular * FresnelLerp (specColor, grazingTerm, nv);

#if TCP2_STYLIZED_FRESNEL
	//TCP2 Enhanced Rim/Fresnel
	color += StylizedFresnel(nv, roughness, light, rimParams);
#endif

	//return half4(gi.specular, 1);
	
	return half4(color, 1);
}


// Based on Minimalist CookTorrance BRDF
// Implementation is slightly different from original derivation: http://www.thetenthplanet.de/archives/255
//
// * BlinnPhong as NDF
// * Modified Kelemen and Szirmay-​Kalos for Visibility term
// * Fresnel approximated with 1/LdotH
half4 BRDF2_TCP2_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi,
	//TCP2 added properties
	fixed tcp2RampThreshold, fixed tcp2RampSmoothness,
	fixed4 tcp2HighlightColor, fixed4 tcp2ShadowColor,
	fixed tcp2specSmooth, fixed tcp2specBlend,
	fixed3 rimParams,
	half atten)
{
	half3 halfDir = Unity_SafeNormalize (light.dir + viewDir);
	
	half nl = light.ndotl;
	//TCP2 Ramp N.L
	nl = WrapRampNL(nl, tcp2RampThreshold, tcp2RampSmoothness);
	
	half nh = BlinnTerm (normal, halfDir);
	half nv = DotClamped (normal, viewDir);
	half lh = DotClamped (light.dir, halfDir);
	
	half roughness = 1-oneMinusRoughness;
	half specularPower = RoughnessToSpecPower (roughness);
	// Modified with approximate Visibility function that takes roughness into account
	// Original ((n+1)*N.H^n) / (8*Pi * L.H^3) didn't take into account roughness 
	// and produced extremely bright specular at grazing angles
	
	// HACK: theoretically we should divide by Pi diffuseTerm and not multiply specularTerm!
	// BUT 1) that will make shader look significantly darker than Legacy ones
	// and 2) on engine side "Non-important" lights have to be divided by Pi to in cases when they are injected into ambient SH
	// NOTE: multiplication by Pi is cancelled with Pi in denominator
	
	half invV = lh * lh * oneMinusRoughness + roughness * roughness; // approx ModifiedKelemenVisibilityTerm(lh, 1-oneMinusRoughness);
	half invF = lh;
	half specular = ((specularPower + 1) * pow (nh, specularPower)) / (8 * invV * invF + 1e-4h);
	if (IsGammaSpace())
		specular = sqrt(max(1e-4h, specular));
	
	// surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(realRoughness^2+1)
	half realRoughness = roughness*roughness;		// need to square perceptual roughness
	// 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
	// 1-x^3*(0.6-0.08*x)   approximation for 1/(x^4+1)
	half surfaceReduction = IsGammaSpace() ? 0.28 : (0.6 - 0.08*roughness);
	surfaceReduction = 1.0 - realRoughness*roughness*surfaceReduction;
	
	// Prevent FP16 overflow on mobiles
#if SHADER_API_GLES || SHADER_API_GLES3
	specular = clamp(specular, 0.0, 100.0);
#endif
	
#if TCP2_SPEC_TOON
	//TCP2 Stylized Specular
	half r = roughness * 0.33;
	if (IsGammaSpace()) r = roughness * 1.5;
	r += 1e-4h;
	specular = lerp(specular, StylizedSpecular(specular, tcp2specSmooth) * (1/r), tcp2specBlend);
#endif
	
	//TCP2 Colored Highlight/Shadows
	tcp2ShadowColor = lerp(tcp2HighlightColor, tcp2ShadowColor, tcp2ShadowColor.a);	//Shadows intensity through alpha
	half3 diffuseTermRGB = lerp(tcp2ShadowColor.rgb, tcp2HighlightColor.rgb, nl * atten);
	half3 diffuseTCP2 = (diffColor + specular * specColor) * light.color * diffuseTermRGB;
	//original: (diffColor + specular * specColor) * light.color * nl
	
	//TCP2: atten contribution to specular since it was removed from light calculation
	specular *= atten;
	
	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));
    half3 color =	diffuseTCP2
    				+ gi.diffuse * diffColor
					+ surfaceReduction * gi.specular * FresnelLerpFast (specColor, grazingTerm, nv);
	
#if TCP2_STYLIZED_FRESNEL
	//TCP2 Enhanced Rim/Fresnel
	color += StylizedFresnel(nv, roughness, light, rimParams);
#endif
	
	return half4(color, 1);
}


half3 BRDF3_Direct_TCP2(half3 diffColor, half3 specColor, half rlPow4, half oneMinusRoughness,
	//TCP2
	half atten, fixed specSmooth, fixed specBlend)
{
	half LUT_RANGE = 16.0; // must match range in NHxRoughness() function in GeneratedTextures.cpp
	// Lookup texture to save instructions
	half specular = tex2D(unity_NHxRoughness, half2(rlPow4, 1-oneMinusRoughness)).UNITY_ATTEN_CHANNEL * LUT_RANGE;
#if TCP2_SPEC_TOON
	//TCP2 Stylized Specular
	half r = (1-oneMinusRoughness) * 0.85;
	if (IsGammaSpace()) r = (1-oneMinusRoughness);
	r += 1e-4h;
	specular = lerp(specular, StylizedSpecular(specular, specSmooth) * (1/r), specBlend);
#endif

	//TCP2: atten contribution to specular since it was removed from light calculation
	specular *= atten;
	
	return diffColor + specular * specColor;
}

/*
half3 BRDF3_Indirect(half3 diffColor, half3 specColor, UnityIndirect indirect, half grazingTerm, half fresnelTerm)
{
	half3 c = indirect.diffuse * diffColor;
	c += indirect.specular * lerp (specColor, grazingTerm, fresnelTerm);
	return c;
}
*/

// Old school, not microfacet based Modified Normalized Blinn-Phong BRDF
// Implementation uses Lookup texture for performance
//
// * Normalized BlinnPhong in RDF form
// * Implicit Visibility term
// * No Fresnel term
//
// TODO: specular is too weak in Linear rendering mode
half4 BRDF3_TCP2_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi,
	//TCP2 added properties
	fixed tcp2RampThreshold, fixed tcp2RampSmoothness,
	fixed4 tcp2HighlightColor, fixed4 tcp2ShadowColor,
	fixed tcp2specSmooth, fixed tcp2specBlend,
	fixed3 rimParams,
	half atten)
{
	half3 reflDir = reflect (viewDir, normal);
	
	half nl = light.ndotl;
	//TCP2 Ramp N.L
	nl = WrapRampNL(nl, tcp2RampThreshold, tcp2RampSmoothness);
	
	half nv = DotClamped (normal, viewDir);
	
	// Vectorize Pow4 to save instructions
	half2 rlPow4AndFresnelTerm = Pow4 (half2(dot(reflDir, light.dir), 1-nv));  // use R.L instead of N.H to save couple of instructions
	half rlPow4 = rlPow4AndFresnelTerm.x; // power exponent must match kHorizontalWarpExp in NHxRoughness() function in GeneratedTextures.cpp
	half fresnelTerm = rlPow4AndFresnelTerm.y;
	
	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));
	
	half3 color = BRDF3_Direct_TCP2(diffColor, specColor, rlPow4, oneMinusRoughness,
									atten, tcp2specSmooth, tcp2specBlend);
	
	//TCP2 Colored Highlight/Shadows
	half3 diffuseTermRGB = lerp(tcp2ShadowColor.rgb, tcp2HighlightColor.rgb, nl * atten);
	color *= light.color * diffuseTermRGB;
	//original: color *= light.color * nl;
	
	color += BRDF3_Indirect(diffColor, specColor, gi, grazingTerm, fresnelTerm);
	
#if TCP2_STYLIZED_FRESNEL
	//TCP2 Enhanced Rim/Fresnel
	color += StylizedFresnel(nv, 1-oneMinusRoughness, light, rimParams);
#endif
	
	return half4(color, 1);
}

#endif // TCP2_STANDARD_BRDF_INCLUDED
