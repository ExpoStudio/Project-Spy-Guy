Shader "TextMeshPro/Distance Field (Shake)"
{
    Properties
    {
        _FaceColor("Face Color", Color) = (1,1,1,1)
        _FaceDilate("Face Dilate", Range(-1,1)) = 0
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Thickness", Range(0,1)) = 0
        _OutlineSoftness("Outline Softness", Range(0,1)) = 0
        _MainTex("Font Atlas", 2D) = "white" {}
        _GradientScale("Gradient Scale", float) = 5
        _WeightNormal("Weight Normal", float) = 0
        _WeightBold("Weight Bold", float) = .5
        _ScaleRatioA("Scale RatioA", float) = 1
        _ShakeAmount("Shake Amount", Float) = 2.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Cull Off
        ZWrite Off
        Lighting Off
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ OUTLINE_ON

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _FaceColor;
            float _FaceDilate;
            float4 _OutlineColor;
            float _OutlineWidth;
            float _OutlineSoftness;
            float _GradientScale;
            float _WeightNormal;
            float _WeightBold;
            float _ScaleRatioA;
            float _ShakeAmount;

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 faceColor : COLOR0;
                fixed4 outlineColor : COLOR1;
                float weight : TEXCOORD1;
            };

            v2f vert(appdata_t v)
            {
                v2f o;

                // Shake logic: only shake if red channel > 0.5
                float4 pos = v.vertex;
                if (v.color.r > 0.5)
                {
                    float timeShake = _Time.y * 30.0 + pos.x * 10.0;
                    float offsetX = sin(timeShake) * _ShakeAmount * 0.5;
                    float offsetY = cos(timeShake) * _ShakeAmount * 0.5;
                    pos.xy += float2(offsetX, offsetY);
                }

                o.vertex = UnityObjectToClipPos(pos);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                // The text color is controlled by v.color (from the TMP component) and _FaceColor (the shader property)
                o.faceColor = v.color * _FaceColor;
                o.outlineColor = _OutlineColor;

                // Calculate weight for distance field
                float weight = lerp(_WeightNormal, _WeightBold, v.color.b) / _GradientScale;
                weight += _FaceDilate * _ScaleRatioA * 0.5;
                o.weight = weight;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample distance field
                float d = tex2D(_MainTex, i.uv).a * _GradientScale;
                float sd = saturate(d - i.weight);
                float outline = saturate(d - (i.weight + _OutlineWidth * _ScaleRatioA));

                // Remove anti-aliasing and soft edge blending for sharp, opaque text
                //float softness = _OutlineSoftness * _ScaleRatioA;
                //float antialias = _GradientScale * (1.0 - softness);
                //float blend = smoothstep(0, antialias, sd) * i.faceColor.a;

                // Instead, use a hard threshold for alpha
                float threshold = 0.5;
                float alpha = sd > threshold ? i.faceColor.a : 0.0;

                // Outline (optional, keep if you want outline)
                float outlineAlpha = outline > threshold ? i.outlineColor.a : 0.0;
                fixed4 color = lerp(i.outlineColor, i.faceColor, alpha);

                // Combine alpha for outline and face
                color.a = max(alpha, outlineAlpha);

                return color;
            }
            ENDCG
        }
    }
}