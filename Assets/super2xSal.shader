Shader "Tutorial/BZR"
{
    SubShader
    {
        Pass
        {

        CGPROGRAM
        #pragma vertex main_vertex
        #pragma fragment main_fragment
        
        struct tex_coords
        {
            float2 pixcoord : TEXCOORD8;
            float4 g1_2 : TEXCOORD1;
            float4 dx_dy : TEXCOORD2;
            
            float4 c0_1 : TEXCOORD3; 
            float4 c2_3 : TEXCOORD4; 
            float4 c4_5 : TEXCOORD5; 
            float4 c6_7 : TEXCOORD6;
            float4 c8_0 : TEXCOORD7;
        };
        
        struct input
        {
            float2 video_size;
            float2 texture_size;
            float2 output_size;
        };
        
        void main_vertex
        (
            float4 position	: POSITION,
            float4 color	: COLOR,
            float2 tex : TEXCOORD0,
            out float2 oTex : TEXCOORD0,
            
            uniform float4x4 modelViewProj,
            
            uniform input IN,
            out float4 oPosition : POSITION,
            out float4 oColor    : COLOR,
            out tex_coords coords
        )
        {
            oPosition = mul(modelViewProj, position);
            oColor = color;
            oTex = tex;
            
            float2 ps = 1.0 / IN.texture_size;
            float2 dx = float2(ps.x, 0.0);
            float2 dy = float2(0.0, ps.y);
            float2 g1 = float2(ps.x, ps.y);
            float2 g2 = float2(-ps.x, ps.y);
            
            coords.pixcoord = tex / IN.texture_size;
            coords.g1_2 = float4(g1, g2);
            coords.dx_dy = float4(dx, dy);
            coords.c0_1 = float4(tex - g1, tex - dy);
            coords.c2_3 = float4(tex - g2, tex - dx);
            coords.c4_5 = float4(tex     , tex + dx);
            coords.c6_7 = float4(tex + g2, tex + dy);
            coords.c8_0 = float4(tex + g1, tex + g2 + dy);
        }
        
        /*  GET_RESULT function                            */
        /*  Copyright (c) 1999-2001 by Derek Liauw Kie Fa  */
        /*  License: GNU-GPL                               */
        int GET_RESULT(float A, float B, float C, float D)
        {
            int x = 0; int y = 0; int r = 0;
            if (A == C) x+=1; else if (B == C) y+=1;
            if (A == D) x+=1; else if (B == D) y+=1;
            if (x <= 1) r+=1; 
            if (y <= 1) r-=1;
            return r;
        } 
        
        const float3 dtt = float3(65536.0, 255.0, 1.0);
        float reduce(half3 color)
        { 
            return dot(color, dtt);
        }
        
        float4 main_fragment(in tex_coords co, float2 tex : TEXCOORD0, uniform sampler2D s0 : TEXUNIT0, uniform input IN) : COLOR
        {
            float2 fp        = frac(co.pixcoord);
            
            float4 d1_2 = float4(tex + co.g1_2.xy + co.g1_2.zw, tex + co.g1_2.xy + co.dx_dy.zw);
            float4 d3_4 = float4(tex - co.g1_2.zw + co.dx_dy.xy, tex + co.g1_2.xy - co.g1_2.zw);
            float4 d5_6 = float4(tex + co.g1_2.xy + co.dx_dy.xy, tex + 2.0 * co.g1_2.xy);
            
            // Reading the texels. (So much! :v)
            half3 C0 = tex2D(s0, co.c0_1.xy).xyz; 
            half3 C1 = tex2D(s0, co.c0_1.zw).xyz;
            half3 C2 = tex2D(s0, co.c2_3.xy).xyz;
            half3 D3 = tex2D(s0, d3_4.xy).xyz;
            half3 C3 = tex2D(s0, co.c2_3.zw).xyz;
            half3 C4 = tex2D(s0, co.c4_5.xy).xyz;
            half3 C5 = tex2D(s0, co.c4_5.zw).xyz;
            half3 D4 = tex2D(s0, co.c4_5.zw).xyz;
            half3 C6 = tex2D(s0, co.c6_7.xy).xyz;
            half3 C7 = tex2D(s0, co.c6_7.zw).xyz;
            half3 C8 = tex2D(s0, co.c8_0.xy).xyz;
            half3 D5 = tex2D(s0, d5_6.xy).xyz;
            half3 D0 = tex2D(s0, co.c8_0.zw).xyz;
            half3 D1 = tex2D(s0, d1_2.xy).xyz;
            half3 D2 = tex2D(s0, d1_2.zw).xyz;
            half3 D6 = tex2D(s0, d5_6.zw).xyz;
            
            float3 p00, p10, p01, p11;
            
            // reducing half3 to float
            float c0 = reduce(C0); float c1 = reduce(C1);
            float c2 = reduce(C2); float c3 = reduce(C3);
            float c4 = reduce(C4); float c5 = reduce(C5);
            float c6 = reduce(C6); float c7 = reduce(C7);
            float c8 = reduce(C8); float d0 = reduce(D0);
            float d1 = reduce(D1); float d2 = reduce(D2);
            float d3 = reduce(D3); float d4 = reduce(D4);
            float d5 = reduce(D5); float d6 = reduce(D6);
            
            /*              Super2xSaI code               */
            /*  Copied from the Dosbox source code        */
            /*  Copyright (C) 2002-2007  The DOSBox Team  */
            /*  License: GNU-GPL                          */
            /*  Adapted by guest(r) on 19.4.2007          */
            
            
            // Oh dear... This will be pure pain to make branchless :D if even possible...
            if (c7 == c5 && c4 != c8) {
                p11 = p01 = C7;
            } else if (c4 == c8 && c7 != c5) {
                p11 = p01 = C4;
            } else if (c4 == c8 && c7 == c5) {
                int r = 0;
                r += GET_RESULT(c5,c4,c6,d1);
                r += GET_RESULT(c5,c4,c3,c1);
                r += GET_RESULT(c5,c4,d2,d5);
                r += GET_RESULT(c5,c4,c2,d4);
                
                if (r > 0)
                 p11 = p01 = C5;
                else if (r < 0)
                 p11 = p01 = C4;
                else {
                 p11 = p01 = 0.5*(C4+C5);
                }
            } else {
                if (c5 == c8 && c8 == d1 && c7 != d2 && c8 != d0)
                 p11 = 0.25*(3.0*C8+C7);
                else if (c4 == c7 && c7 == d2 && d1 != c8 && c7 != d6)
                 p11 = 0.25*(3.0*C7+C8);
                else
                 p11 = 0.5*(C7+C8);
                
                if (c5 == c8 && c5 == c1 && c4 != c2 && c5 != c0)
                 p01 = 0.25*(3.0*C5+C4);
                else if (c4 == c7 && c4 == c2 && c1 != c5 && c4 != d3)
                 p01 = 0.25*(3.0*C4+C5);
                else
                 p01 = 0.5*(C4+C5);
            }
            
            if (c4 == c8 && c7 != c5 && c3 == c4 && c4 != d2)
              p10 = 0.5*(C7+C4);
            else if (c4 == c6 && c5 == c4 && c3 != c7 && c4 != d0)
              p10 = 0.5*(C7+C4);
            else
              p10 = C7;
            
            if (c7 == c5 && c4 != c8 && c6 == c7 && c7 != c2)
              p00 = 0.5*(C7+C4);
            else if (c3 == c7 && c8 == c7 && c6 != c4 && c7 != c0)
              p00 = 0.5*(C7+C4);
            else
              p00 = C4;
            
            
            // Distributing the four products
            
            if (fp.x < 0.50)
            { 
                if (fp.y < 0.50) 
                 p10 = p00;
            }
            else
            { 
                if (fp.y < 0.50) 
                 p10 = p01; 
                else 
                 p10 = p11;
            }
            
            return float4(p10, 1.0);
        }
        
        ENDCG
        }
    }
}