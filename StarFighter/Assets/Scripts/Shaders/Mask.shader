Shader "Custom/Mask" {
    Properties {
        _Color ("Main Color, Alpha", Color) = (1,1,1,1)
    }

    Category {
        Tags {"RenderType" = "Opaque" "Queue"="Transparent"}

        Cull Off
        ZWrite On
        ZTest LEqual
        Lighting Off
        Color [_Color]

        SubShader {
            Pass {
                Colormask A
                Offset -1, -1
                Cull Back
            }
        }
    } 
}