using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Linq;

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



    // Zmienne do okreœlenia dok³adnoœci
    private float timeTakenToRecolor = 0f; // w sekundach
    private float accuracy = 0f; // w procentach

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

    public void setSegments(List<GameObject> segmentList)
    {
        segments = segmentList;
    }

    private void GameManagerOnGameStateChanges(GameState newState)
    {
        currentGameState = newState;
    }

    private bool IsControllerTouchingAnySegment(Collider controller)
    {
        foreach (GameObject segment in segments)
        {
            if (segment.GetComponent<Collider>().bounds.Intersects(controller.bounds))
            {
                return true; // Controller nadal dotyka jakiegoœ segmentu
            }
        }
        return false; // Brak kolizji z segmentami
    }


    // Funkcja wywo³ywana po dotkniêciu przez collider kontrolera 
    private void OnTriggerEnter(Collider other)
     {

        currentGameState = GameManager.instance.state;

        if (currentGameState != GameState.PATIENT_MODE) return; //Sprawdzenie trybu gry


        if (other.CompareTag("Controller"))
        {
            // Rozpoczêcie liczenia czasu kolorowania do obliczania dok³adnoœci
            AccuracyManager.instance.StopIdleTimer();
            AccuracyManager.instance.StartRecoloringTimer();
            //Debug.Log("TRAFIASZ!");

            //Sprawdzenie czy pierwszy segment zosta³ pokolorowany
            if (previousSegment == null && !currentSegment.GetComponent<Segment3D>().isColored())
            {
                //TimeManager.instance.StartTimer();
                AccuracyManager.instance.Reset();
                AccuracyManager.instance.StartRecoloringTimer();
            }
     

            // Sprawdzenie czy poprzedni segment pokolorowany
            if (previousSegment != null && !previousSegment.GetComponent<Segment3D>().isColored()) return;

            // Sprawdzenie czy obecny jest ju¿ pomalowany
            if(currentSegment.GetComponent<Segment3D>().isColored()) return;


            currentSegment.GetComponent<Segment3D>().setColored(true);

            //Debug.Log($"Kolorowanie segmentu spline'a: {gameObject.name}");

            // Sprawdzenie czy pomalowany zosta³ ostatni segment
            if (gameObject == segments.Last())
            {
                // Liczenie dok³adnoœci i czasu trwania kolorowania
                AccuracyManager.instance.StopRecoloringTimer();
                timeTakenToRecolor = AccuracyManager.instance.GetTimeTotal();
                accuracy = AccuracyManager.instance.GetAccuracy();
                Debug.Log("DOK£ADNOŒÆ KOLOROWANIA " + accuracy + "%");
                Debug.Log("CZAS KOLOROWANIA " + timeTakenToRecolor + "s");
                AccuracyManager.instance.Reset();
            }

            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = newMaterial;

            XRBaseController controller = other.GetComponentInParent<XRBaseController>();
            HapticController.SendHaptics(controller, hapticIntensity, hapticDuration);

            
        }
     }

    private void OnTriggerExit(Collider other)
    {
        currentGameState = GameManager.instance.state;

        if (currentGameState != GameState.PATIENT_MODE) return; //Sprawdzenie trybu gry

        if (other.CompareTag("Controller"))
        {
            AccuracyManager.instance.StopRecoloringTimer();
            AccuracyManager.instance.StartIdleTimer();
            Debug.Log("PUD£UJESZ!");
        }

    }

    private void Update()
    {
        Debug.Log(AccuracyManager.instance.GetAccuracy() + " %");
        //Debug.Log(AccuracyManager.instance.GetTimeTotal() + " s");
    }


}
