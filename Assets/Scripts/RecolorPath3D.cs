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
    private List<GameObject> segments;

    [SerializeField]
    private GameObject previousSegment = null;
    [SerializeField]
    private GameObject currentSegment;

    private void Awake()
    {
        // Dodawanie ScriptableObject z konfiguracj¹ do skryptu segmentu
        string[] configFile = AssetDatabase.FindAssets("MainConfig", new[] { "Assets/Configuration" });
        string path = AssetDatabase.GUIDToAssetPath(configFile[0]);
        config = AssetDatabase.LoadAssetAtPath<Config>(path);
    }

    public void setPreviousSegment(GameObject segment)
    {
        previousSegment = segment;
    }

    public void setCurrentSegment(GameObject segment)
    {
        currentSegment = segment;
    }

    // Funkcja wywo³ywana po dotkniêciu przez collider kontrolera (tylko gdy liveDrawingMode = false)
    private void OnTriggerEnter(Collider other)
     {  
        if (other.CompareTag("Controller"))
        {
            if (config.getDrawingMode()) return;

            if(previousSegment != null && !previousSegment.GetComponent<Segment3D>().isColored()) return;

            currentSegment.GetComponent<Segment3D>().setColored(true);

            Debug.Log($"Kolorowanie segmentu spline'a: {gameObject.name}");

            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = newMaterial;
        }
     }

}
