Shader "Special/TroopWidget" {
	Properties {
	    _MainTex ("Base (RGBA)", 2D) = "white" {}
		_Alpha ("Alpha", Range (0.01, 1)) = 0.5
		_Color1 ("Color1", Color) = (1,1,1,1)
        _Color2 ("Color2", Color) = (1,1,1,1)
        _Color3 ("Color3", Color) = (1,1,1,1)
        _Color4 ("Color4", Color) = (1,1,1,1)
        _Ammount1 ("Ammount1", float) = 25
        _Ammount2 ("Ammount2", float) = 25
        _Ammount3 ("Ammount3", float) = 25
        _Ammount4 ("Ammount4", float) = 25
	}
	SubShader {
	  Tags {"Queue" = "Transparent" }
      Pass{
	   CGPROGRAM
	     sampler2D _MainTex;
	     float _Alpha;
	     float _Ammount1;
		 float _Ammount2;
		 float _Ammount3;
		 float _Ammount4;
		 float4 _Color1;
         float4 _Color2;
         float4 _Color3;
         float4 _Color4;

		 #pragma vertex vert_img
         #pragma fragment frag

         #include "UnityCG.cginc"

         float4 frag(v2f_img i) : COLOR {
           float sum = _Ammount1 + _Ammount2 + _Ammount3 + _Ammount4;
		     
		   if(i.uv.y > ((_Ammount2 + _Ammount3 + _Ammount4) / sum))
		   {
             return (1 - _Alpha) * tex2D(_MainTex, i.uv) + _Alpha * _Color1;
		   }
		   if(i.uv.y > ((_Ammount3 + _Ammount4) / sum))
		   {
			 return (1 - _Alpha) * tex2D(_MainTex, i.uv) + _Alpha * _Color2;
		   }
		   if(i.uv.y > ((_Ammount4) / sum))
		   {
             return (1 - _Alpha) * tex2D(_MainTex, i.uv) + _Alpha * _Color3;
		   }
		   return (1 - _Alpha) * tex2D(_MainTex, i.uv) + _Alpha * _Color4;
         }

	   ENDCG
      }
	}
	
  Fallback "Particles/Additive" 
}
