using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RecolorPath3D : MonoBehaviour
{
    public Material newMaterial;
    private Material originalMaterial;
    private MeshRenderer[] meshRenderers;
    private Config config;

    private void Awake()
    {
        // Dodawanie ScriptableObject z konfiguracj� do skryptu segmentu
        string[] configFile = AssetDatabase.FindAssets("MainConfig", new[] { "Assets/Configuration" });
        string path = AssetDatabase.GUIDToAssetPath(configFile[0]);
        config = AssetDatabase.LoadAssetAtPath<Config>(path);
    }

    // Funkcja wywo�ywana po dotkni�ciu przez collider kontrolera (tylko gdy liveDrawingMode = false)
    private void OnTriggerEnter(Collider other)
     {  
        if (other.CompareTag("Controller"))
        {
            if (config.getDrawingMode()) return;

            Debug.Log($"Kolorowanie segmentu spline'a: {gameObject.name}");

            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = newMaterial;
        }
     }

}
