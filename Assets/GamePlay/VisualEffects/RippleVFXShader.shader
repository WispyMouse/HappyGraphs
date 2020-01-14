// Shader created with Shader Forge v1.40 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.40;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,cpap:True,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3143,x:33427,y:32620,varname:node_3143,prsc:2|emission-6441-OUT,alpha-9580-OUT;n:type:ShaderForge.SFN_Slider,id:226,x:32028,y:32870,ptovrint:False,ptlb:Radius,ptin:_Radius,varname:node_226,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.8,max:1;n:type:ShaderForge.SFN_TexCoord,id:4671,x:32030,y:32561,varname:node_4671,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_RemapRange,id:1355,x:32248,y:32497,varname:node_1355,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-4671-UVOUT;n:type:ShaderForge.SFN_Length,id:8897,x:32426,y:32514,varname:node_8897,prsc:2|IN-1355-OUT;n:type:ShaderForge.SFN_Subtract,id:5525,x:32430,y:32770,varname:node_5525,prsc:2|A-8897-OUT,B-226-OUT;n:type:ShaderForge.SFN_Divide,id:6344,x:32856,y:32880,varname:node_6344,prsc:2|A-9275-OUT,B-1277-OUT;n:type:ShaderForge.SFN_DDXY,id:1277,x:32663,y:32942,varname:node_1277,prsc:2|IN-5525-OUT;n:type:ShaderForge.SFN_Abs,id:836,x:32625,y:32662,varname:node_836,prsc:2|IN-5525-OUT;n:type:ShaderForge.SFN_Slider,id:720,x:32020,y:33027,ptovrint:False,ptlb:Thickness,ptin:_Thickness,varname:node_720,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.05,max:1;n:type:ShaderForge.SFN_Subtract,id:9275,x:32420,y:32992,varname:node_9275,prsc:2|A-836-OUT,B-720-OUT;n:type:ShaderForge.SFN_Clamp01,id:3871,x:32978,y:32744,varname:node_3871,prsc:2|IN-6344-OUT;n:type:ShaderForge.SFN_OneMinus,id:9580,x:33207,y:32966,varname:node_9580,prsc:2|IN-6441-OUT;n:type:ShaderForge.SFN_Step,id:6441,x:33014,y:32558,varname:node_6441,prsc:2|A-8060-OUT,B-3871-OUT;n:type:ShaderForge.SFN_Slider,id:8060,x:32710,y:32464,ptovrint:False,ptlb:TransparencyCutoff,ptin:_TransparencyCutoff,varname:node_8060,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.2393163,max:1;proporder:226-720-8060;pass:END;sub:END;*/

Shader "Custom/RippleVFXShader" {
    Properties {
        _Radius ("Radius", Range(0, 1)) = 0.8
        _Thickness ("Thickness", Range(0, 1)) = 0.05
        _TransparencyCutoff ("TransparencyCutoff", Range(0, 1)) = 0.2393163
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 100
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma target 3.0
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float, _Radius)
                UNITY_DEFINE_INSTANCED_PROP( float, _Thickness)
                UNITY_DEFINE_INSTANCED_PROP( float, _TransparencyCutoff)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                UNITY_SETUP_INSTANCE_ID( i );
////// Lighting:
////// Emissive:
                float _TransparencyCutoff_var = UNITY_ACCESS_INSTANCED_PROP( Props, _TransparencyCutoff );
                float _Radius_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Radius );
                float node_5525 = (length((i.uv0*2.0+-1.0))-_Radius_var);
                float _Thickness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Thickness );
                float node_6441 = step(_TransparencyCutoff_var,saturate(((abs(node_5525)-_Thickness_var)/fwidth(node_5525))));
                float3 emissive = float3(node_6441,node_6441,node_6441);
                float3 finalColor = emissive;
                return fixed4(finalColor,(1.0 - node_6441));
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
