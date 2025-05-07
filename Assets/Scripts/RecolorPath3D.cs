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

    private GameObject hitboxObject;
    internal Collider controllerInZone = null;
    private bool isTouchingSegment = false;
    private bool isPaused = false;

    [SerializeField][Range(0f,1f)] private float hapticIntensity = 0.3f;
    [SerializeField] private float hapticDuration = 0.1f;

    // Zmienne do okre�lenia dok�adno�ci
    private float timeTakenToRecolor = 0f; // w sekundach
    private float accuracy = 0f; // w procentach

    private void Awake()
    {
        setSegments(segments);
        
        //currentGameState = GameManager.instance.state;
        //GameManager.onGameStateChanged += GameManagerOnGameStateChanges; //RecolorPath subskrybuje GameManager

        // Dodawanie ScriptableObject z konfiguracj� do skryptu segmentu
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
        CreateUnifiedHitbox();
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
                return true; // Controller nadal dotyka jakiego� segmentu
            }
        }
        return false; // Brak kolizji z segmentami
    }

    private void CreateUnifiedHitbox()
    {
        if (segments == null || segments.Count == 0) return;

       
        Bounds bounds = new Bounds(segments[0].transform.position, Vector3.zero);
        foreach (GameObject segment in segments)
        {
            bounds.Encapsulate(segment.GetComponent<Renderer>().bounds);
        }

        
        hitboxObject = new GameObject("UnifiedHitbox");
        hitboxObject.transform.SetParent(this.transform); 
        hitboxObject.transform.position = bounds.center;

        BoxCollider collider = hitboxObject.AddComponent<BoxCollider>();
        collider.size = bounds.size;
        collider.isTrigger = true;

        hitboxObject.AddComponent<HitboxListener>().Init(this);
    }


    // Funkcja wywo�ywana po dotkni�ciu przez collider kontrolera 
    private void OnTriggerEnter(Collider other)
     {

        currentGameState = GameManager.instance.state;

        //Sprawdzenie trybu gry
        if (currentGameState != GameState.PATIENT_MODE)
        {
            AccuracyManager.instance.Reset();
            return; 
        }


        if (other.CompareTag("Controller"))
        {
            // Rozpocz�cie liczenia czasu kolorowania do obliczania dok�adno�ci
            //AccuracyManager.instance.StopIdleTimer();
            //AccuracyManager.instance.StartRecoloringTimer();
            //Debug.Log("TRAFIASZ!");

            //Sprawdzenie czy pierwszy segment zosta� pokolorowany
            if (previousSegment == null && !currentSegment.GetComponent<Segment3D>().isColored())
            {
                //TimeManager.instance.StartTimer();
                AccuracyManager.instance.Reset();
                AccuracyManager.instance.StartRecoloring();
                //AccuracyManager.instance.StartRecoloringTimer();
            }
     

            // Sprawdzenie czy poprzedni segment pokolorowany
            if (previousSegment != null && !previousSegment.GetComponent<Segment3D>().isColored()) return;

            // Sprawdzenie czy obecny jest ju� pomalowany
            if(currentSegment.GetComponent<Segment3D>().isColored()) return;


            currentSegment.GetComponent<Segment3D>().setColored(true);

            //Debug.Log($"Kolorowanie segmentu spline'a: {gameObject.name}");

            // Sprawdzenie czy pomalowany zosta� ostatni segment
            if (gameObject == segments.Last())
            {
                // Liczenie dok�adno�ci i czasu trwania kolorowania
                AccuracyManager.instance.StopRecoloringTimer();
                AccuracyManager.instance.FinishRecoloring();
                timeTakenToRecolor = AccuracyManager.instance.GetTimeInTotal();
                accuracy = AccuracyManager.instance.GetAccuracy();
                Debug.Log("DOK�ADNO�� KOLOROWANIA " + accuracy + "%");
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
            //AccuracyManager.instance.StopRecoloringTimer();
            //AccuracyManager.instance.StartIdleTimer();
            //Debug.Log("PUD�UJESZ!");
        }

    }

    public void OnUnifiedTriggerEnter(Collider other)
    {
        currentGameState = GameManager.instance.state;
        if (currentGameState != GameState.PATIENT_MODE) return;

        if (IsControllerTouchingAnySegment(other))
        {
            AccuracyManager.instance.StopIdleTimer();
            AccuracyManager.instance.StartRecoloringTimer();
            Debug.Log("Trafiasz!");
        }
    }

    public void OnUnifiedTriggerExit(Collider other)
    {
        currentGameState = GameManager.instance.state;
        if (currentGameState != GameState.PATIENT_MODE) return;

        AccuracyManager.instance.StopRecoloringTimer();
        AccuracyManager.instance.StartIdleTimer();
        Debug.Log("PUD�UJESZ!");
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            AccuracyManager.instance?.Pause();
            Debug.Log("Gra zatrzymana.");
        }
        else
        {
            AccuracyManager.instance?.Resume();
            Debug.Log("Gra wznowiona.");
        }
    }

    private void Update()
    {
        //Prosta symulacja pauzy
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
        //Debug.Log(AccuracyManager.instance.GetAccuracy() + " %");
        ///Debug.Log(AccuracyManager.instance.GetTimeTotal() + " s");

    }


}
