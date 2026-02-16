Shader "UI/Blur"
{
    Properties
    {
        _BlurSize ("Blur Size", Range(0, 10)) = 5
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        
        GrabPass { "_BackgroundTexture" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 grabPos : TEXCOORD0;
            };
            
            sampler2D _BackgroundTexture;
            float _BlurSize;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float4 sum = float4(0, 0, 0, 0);
                float blurPixels = _BlurSize * 0.001;
                
                // 9-tap Gaussian Blur
                sum += tex2Dproj(_BackgroundTexture, i.grabPos + float4(-blurPixels, -blurPixels, 0, 0)) * 0.05;
                sum += tex2Dproj(_BackgroundTexture, i.grabPos + float4(0, -blurPixels, 0, 0)) * 0.09;
                sum += tex2Dproj(_BackgroundTexture, i.grabPos + float4(blurPixels, -blurPixels, 0, 0)) * 0.05;
                
                sum += tex2Dproj(_BackgroundTexture, i.grabPos + float4(-blurPixels, 0, 0, 0)) * 0.09;
                sum += tex2Dproj(_BackgroundTexture, i.grabPos) * 0.44;
                sum += tex2Dproj(_BackgroundTexture, i.grabPos + float4(blurPixels, 0, 0, 0)) * 0.09;
                
                sum += tex2Dproj(_BackgroundTexture, i.grabPos + float4(-blurPixels, blurPixels, 0, 0)) * 0.05;
                sum += tex2Dproj(_BackgroundTexture, i.grabPos + float4(0, blurPixels, 0, 0)) * 0.09;
                sum += tex2Dproj(_BackgroundTexture, i.grabPos + float4(blurPixels, blurPixels, 0, 0)) * 0.05;
                
                return sum;
            }
            ENDCG
        }
    }
}
