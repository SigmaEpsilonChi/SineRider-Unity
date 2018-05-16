﻿// Copyright (C) 2014 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;


namespace TMPro.EditorUtilities
{

    public class TMPro_ContextMenus : MaterialEditor
    {

        private static Material m_copiedProperties;
        private static Texture m_copiedTexture;


        // Add a Context Menu to allow easy duplication of the Material.
        [MenuItem("CONTEXT/Material/Duplicate Material", false)]
        static void DuplicateMaterial(MenuCommand command)
        {
            Material source_Mat = (Material)command.context;
            string assetPath = AssetDatabase.GetAssetPath(source_Mat).Split('.')[0];

            Material duplicate = new Material(source_Mat);

            // Need to manually copy the shader keywords
            duplicate.shaderKeywords = source_Mat.shaderKeywords;

            AssetDatabase.CreateAsset(duplicate, AssetDatabase.GenerateUniqueAssetPath(assetPath + ".mat"));

            // Assign duplicate Material to selected object (if one is)
            if (Selection.activeGameObject != null)
            {
                TextMeshPro textObject = Selection.activeGameObject.GetComponent<TextMeshPro>();
                textObject.fontSharedMaterial = duplicate;
            }
        }


        [MenuItem("CONTEXT/Material/Copy Material Properties", false)]
        static void CopyMaterialProperties(MenuCommand command)
        {
            Material mat = command.context as Material;
          
            m_copiedProperties = new Material(mat);

            m_copiedProperties.shaderKeywords = mat.shaderKeywords;

            m_copiedProperties.hideFlags = HideFlags.DontSave;
        }


        // PASTE MATERIAL
        [MenuItem("CONTEXT/Material/Paste Material Properties", false)]
        static void PasteMaterialProperties(MenuCommand command)
        {

            if (m_copiedProperties == null)
            {
                Debug.LogWarning("No Material Properties to Paste. Use Copy Material Properties first.");
                return;
            }

            Material mat = (Material)command.context;

            Undo.RecordObject(mat, "Paste Material");
            if (mat.HasProperty(ShaderUtilities.ID_GradientScale))
            {
                // Preserve unique SDF properties from destination material.
                m_copiedProperties.SetTexture(ShaderUtilities.ID_MainTex, mat.GetTexture(ShaderUtilities.ID_MainTex));
                m_copiedProperties.SetFloat(ShaderUtilities.ID_GradientScale, mat.GetFloat(ShaderUtilities.ID_GradientScale));
                m_copiedProperties.SetFloat(ShaderUtilities.ID_TextureWidth, mat.GetFloat(ShaderUtilities.ID_TextureWidth));
                m_copiedProperties.SetFloat(ShaderUtilities.ID_TextureHeight, mat.GetFloat(ShaderUtilities.ID_TextureHeight));
            }

            EditorShaderUtilities.CopyMaterialProperties(m_copiedProperties, mat);

            // Copy ShaderKeywords from one material to the other.
            mat.shaderKeywords = m_copiedProperties.shaderKeywords;

            // Let TextMeshPro Objects that this mat has changed.
            TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(true, mat);
        }


        // Enable Resetting of Material properties without losing unique properties of the font atlas.
        [MenuItem("CONTEXT/Material/Reset", false, 2100)]
        static void ResetSettings(MenuCommand command)
        {
            Material mat = (Material)command.context;
            Undo.RecordObject(mat, "Reset Material");

            Material tmp_mat = new Material(mat.shader);

            if (mat.HasProperty(ShaderUtilities.ID_GradientScale))
            {
                // Copy unique properties of the SDF Material over to the temp material.       
                tmp_mat.mainTexture = mat.mainTexture;
                tmp_mat.SetFloat(ShaderUtilities.ID_GradientScale, mat.GetFloat(ShaderUtilities.ID_GradientScale));
                tmp_mat.SetFloat(ShaderUtilities.ID_GradientScale, mat.GetFloat(ShaderUtilities.ID_GradientScale));
                tmp_mat.SetFloat(ShaderUtilities.ID_GradientScale, mat.GetFloat(ShaderUtilities.ID_GradientScale));

                mat.CopyPropertiesFromMaterial(tmp_mat);

                // Reset ShaderKeywords         
                mat.shaderKeywords = new string[0]; // { "BEVEL_OFF", "GLOW_OFF", "UNDERLAY_OFF" };            
            }
            else
            {
                mat.CopyPropertiesFromMaterial(tmp_mat);
            }

            DestroyImmediate(tmp_mat);

            TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(true, mat);
        }


        /*
        //[MenuItem("CONTEXT/Material/List Materials")]
        static void ListMaterialReferences(MenuCommand command)
        {
            Material mat = (Material)command.context;
            Material[] mats = TextMeshPro_EditorUtility.FindMaterialReferences(mat);

            for (int i = 0; i < mats.Length; i++)
                Debug.Log(i + ": " + mats[i].name);
        }
        */

        
        //This function is used for debugging and fixing potentially broken font atlas links.
        [MenuItem("CONTEXT/Material/Copy Atlas", false, 2000)]
        static void CopyAtlas(MenuCommand command)
        {
            Material mat = command.context as Material;
            
            m_copiedTexture = mat.mainTexture;          
        }
        
    
        // This function is used for debugging and fixing potentially broken font atlas links     
        [MenuItem("CONTEXT/Material/Paste Atlas", false, 2001)]
        static void PasteAtlas(MenuCommand command)
        {
            Material mat = command.context as Material;
            Undo.RecordObject(mat, "Paste Texture");

            mat.mainTexture = m_copiedTexture;
            Debug.Log("Material ID:" + mat.mainTexture.GetInstanceID());
        }


        // Context Menus for TMPro Font Assets
        //This function is used for debugging and fixing potentially broken font atlas links.
        [MenuItem("CONTEXT/TextMeshProFont/Extract Atlas", false, 2000)]
        static void ExtractAtlas(MenuCommand command)
        {
            TextMeshProFont font = command.context as TextMeshProFont;
            Texture2D tex = Instantiate(font.material.mainTexture) as Texture2D;

            string fontPath = AssetDatabase.GetAssetPath(font);
            string texPath = Path.GetDirectoryName(fontPath) + "/" + Path.GetFileNameWithoutExtension(fontPath) + " Atlas.png";
            Debug.Log(texPath);
            // Saving File for Debug
            var pngData = tex.EncodeToPNG();	     
            File.WriteAllBytes(texPath, pngData);

            AssetDatabase.Refresh();
            DestroyImmediate(tex);
        }
        
    }
}