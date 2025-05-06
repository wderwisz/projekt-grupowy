using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class RecolorPath3D : MonoBehaviour
{
    public Material newMaterial;
    private Material originalMaterial;
    private MeshRenderer[] meshRenderers;
    private Config config;
    private List<GameObject> segments;
    private GameState currentGameState;
    private GameObject previousSegment = null;
    private GameObject currentSegment;

    [SerializeField][Range(0f,1f)] private float hapticIntensity = 0.3f;
    [SerializeField] private float hapticDuration = 0.1f;

    private void Awake()
    {
        //currentGameState = GameManager.instance.state;
        //GameManager.onGameStateChanged += GameManagerOnGameStateChanges; //RecolorPath subskrybuje GameManager

        // Dodawanie ScriptableObject z konfiguracj¹ do skryptu segmentu
        //string[] configFile = AssetDatabase.FindAssets("MainConfig", new[] { "Assets/Configuration" });
        //string path = AssetDatabase.GUIDToAssetPath(configFile[0]);
        //config = AssetDatabase.LoadAssetAtPath<Config>(path);
    }

    public void setPreviousSegment(GameObject segment)
    {
        previousSegment = segment;
    }

    public void setCurrentSegment(GameObject segment)
    {
        currentSegment = segment;
    }

    private void GameManagerOnGameStateChanges(GameState newState)
    {
        currentGameState = newState;
    }



    // Funkcja wywo³ywana po dotkniêciu przez collider kontrolera 
    private void OnTriggerEnter(Collider other)
     {

        currentGameState = GameManager.instance.state;

        if (currentGameState != GameState.PATIENT_MODE) return; //Sprawdzenie trybu gry

        if (other.CompareTag("Controller"))
        {
            // Sprawdzenie czy poprzedni segment pokolorowany
            if(previousSegment != null && !previousSegment.GetComponent<Segment3D>().isColored()) return;

            // Sprawdzenie czy obecny jest ju¿ pomalowany
            if(currentSegment.GetComponent<Segment3D>().isColored()) return;

            currentSegment.GetComponent<Segment3D>().setColored(true);

            Debug.Log($"Kolorowanie segmentu spline'a: {gameObject.name}");

            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = newMaterial;

            XRBaseController controller = other.GetComponentInParent<XRBaseController>();
            HapticController.SendHaptics(controller, hapticIntensity, hapticDuration);

            
        }
     }

}
