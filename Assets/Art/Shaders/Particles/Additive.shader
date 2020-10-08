// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AvalonStudios/Particles/Additive"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Main Tex", 2D) = "white" {}
		_Intensity("Intensity", Range( 0 , 10)) = 1.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		ZWrite Off
		Blend One One
		
		CGPROGRAM
		#pragma target 3.0
		#pragma exclude_renderers xbox360 xboxone ps4 psp2 n3ds wiiu 
		#pragma surface surf Unlit keepalpha noshadow nofog 
		struct Input
		{
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
		};

		uniform float _Intensity;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 colorRGBA7 = ( _Intensity * _Color );
			float2 uv0_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float mainTex4 = tex2D( _MainTex, uv0_MainTex ).a;
			float colorAlpha11 = _Color.a;
			float4 tintColor20 = ( colorRGBA7 * i.vertexColor * 2.0 * mainTex4 * i.vertexColor.a * colorAlpha11 );
			o.Emission = tintColor20.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "AvalonStudios/Mobile/Particles/Additive"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
0;0;1440;879;2737.954;-1087.98;1.312972;True;True
Node;AmplifyShaderEditor.CommentaryNode;5;-2225.458,-361.6745;Inherit;False;1240.082;609.4243;Comment;4;1;2;3;4;Main Texture;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;12;-2217.693,344.8871;Inherit;False;1377.324;653.3196;Comment;5;6;8;9;7;11;Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;1;-2175.458,-311.6745;Inherit;True;Property;_MainTex;Main Tex;2;0;Create;True;0;0;False;0;8734a3a6cfedbad4a9b061abbcf7830e;b03bedf7d205c284eafbe91cb1cc9757;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-1899.648,-51.25013;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;6;-2189.288,480.475;Inherit;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;9;-1710.233,408.8785;Inherit;False;Property;_Intensity;Intensity;3;0;Create;True;0;0;False;0;1.5;1.43;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-1619.955,-310.7701;Inherit;True;Property;_TextureSample0;Texture Sample 0;2;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-1360.409,457.7361;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-1943.395,714.9122;Inherit;True;colorAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;21;-2197.819,1119.704;Inherit;False;1522.067;1153.242;Comment;7;20;16;30;18;13;17;14;Apply Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;7;-1076.747,452.3649;Inherit;True;colorRGBA;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;4;-1228.377,-310.1469;Inherit;True;mainTex;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;14;-2131.507,1823.691;Inherit;True;4;mainTex;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;17;-2145.694,1205.019;Inherit;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;13;-2136.595,1394.906;Inherit;True;7;colorRGBA;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;18;-2141.031,2037.177;Inherit;True;11;colorAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-2119.861,1593.646;Inherit;True;Constant;_Value;Value;6;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-1552.509,1487.661;Inherit;True;6;6;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;23;-341.7588,-50;Inherit;False;865.6782;543.7803;Comment;2;0;22;Output;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;20;-1079.782,1481.505;Inherit;True;tintColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;22;-291.7588,38.80527;Inherit;True;20;tintColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;230.5993,-0.4439051;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;AvalonStudios/Particles/Additive;False;False;False;False;False;False;False;False;False;True;False;False;False;False;True;False;False;False;False;False;False;Off;2;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Transparent;;Transparent;All;8;d3d9;d3d11_9x;d3d11;glcore;gles;gles3;metal;vulkan;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;4;1;False;-1;1;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;AvalonStudios/Mobile/Particles/Additive;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;2;1;0
WireConnection;3;0;1;0
WireConnection;3;1;2;0
WireConnection;8;0;9;0
WireConnection;8;1;6;0
WireConnection;11;0;6;4
WireConnection;7;0;8;0
WireConnection;4;0;3;4
WireConnection;16;0;13;0
WireConnection;16;1;17;0
WireConnection;16;2;30;0
WireConnection;16;3;14;0
WireConnection;16;4;17;4
WireConnection;16;5;18;0
WireConnection;20;0;16;0
WireConnection;0;2;22;0
ASEEND*/
//CHKSM=F2A1451C0321352A1437ED88A2827F15C81080A1