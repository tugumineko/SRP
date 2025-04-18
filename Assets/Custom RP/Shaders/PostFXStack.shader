﻿Shader "Hidden/Custom RP/Post FX Stack"
{
    SubShader
    {
        Cull Off
        ZTest Always
        ZWrite Off
        
        HLSLINCLUDE
        #include "../ShaderLibrary/Common.hlsl"
        #include "PostFXStackPasses.hlsl"
        ENDHLSL

        Pass
        {
            Name "Bloom Horizontal"
            
            HLSLPROGRAM
                #pragma target 3.5
                #pragma vertex DefaultPassVertex
                #pragma fragment BloomHorizontalPassFragment
            ENDHLSL
            
        }
        
        Pass
        {
            Name "Bloom Vertical"
            
            HLSLPROGRAM
                #pragma target 3.5
                #pragma vertex DefaultPassVertex
                #pragma fragment BloomVerticalPassFragment
            ENDHLSL
            
        }

        Pass
        {
            Name "Bloom Add"
            
            HLSLPROGRAM
                #pragma target 3.5
                #pragma vertex DefaultPassVertex
                #pragma fragment BloomAddPassFragment
            ENDHLSL
            
        }

        Pass
        {
            Name "Bloom Scatter"
            
            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment BloomScatterPassFragment
            
            ENDHLSL
            
        }

        Pass
        {
            Name "Bloom Scatter Final"
            
            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment BloomScatterFinalPassFragment 
            
            ENDHLSL
        }

        Pass
        {
            Name "Bloom Prefilter"
            
            HLSLPROGRAM

                #pragma target 3.5
                #pragma vertex DefaultPassVertex
                #pragma fragment BloomPrefilterPassFragment
            
            ENDHLSL     
            
        }

        Pass
        {
            Name "Bloom Prefilter Fireflies"
            
            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment BloomPrefilterFirefliesPassFragment

            ENDHLSL
        }

        Pass
        {
            Name "Copy"
            
            HLSLPROGRAM

                #pragma target 3.5
                #pragma vertex DefaultPassVertex
                #pragma fragment CopyPassFragment
            
            ENDHLSL     
            
        }
        
        Pass
        {
            Name "Reduce Color"
            
            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment ReduceColorPassFragment 

            ENDHLSL
        }

        Pass
        {
            Name "Dither Bayer"
            
            HLSLPROGRAM

            #pragma target 3.5
            #pragma multi_compile _ _BAYER4 _BAYER8
            #pragma vertex DefaultPassVertex
            #pragma fragment DitherBayerPassFragment 
            
            ENDHLSL
            
            
        }
        
        Pass
        {
            Name "Halftone"
            
            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment HalftonePassFragment 
            
            ENDHLSL
            
            
        }

        Pass
        {
            Name "Color Grading None"
            
            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment ColorGradingNonePassFragment  
            
            ENDHLSL 
            
        }

        Pass
        {
            Name "Color Grading ACES"
            
            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment ColorGradingACESPassFragment 
            
            ENDHLSL
            
            
        }

        Pass
        {
            Name "Color Grading Neutral"
            
            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment ColorGradingNeutralPassFragment 
            
            ENDHLSL 
            
            
        }

        Pass
        {
            Name "Color Grading Reinhard"
            
            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment ColorGradingReinhardPassFragment   
            
            ENDHLSL
        }

        Pass
        {
            Name "Final"
            
            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment FinalPassFragment 
            
            ENDHLSL 
            
        }

    }
    
    
}