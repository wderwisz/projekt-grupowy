using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Splines;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;

public class DrawingPath3D : MonoBehaviour
{
    public XRBaseController rightController;
    public XRBaseController leftController;
    public SplineContainer splineContainerPrefab;

    [SerializeField][Range(0.0001f, 2.0f)] private float pointSpacing = 0.1f;
    [SerializeField] private Config config;
    [SerializeField][Range(0f, 1f)] private float hapticIntensity = 0.2f;
    [SerializeField] private float hapticDuration = 0.15f;
    [SerializeField] private Material originalMaterial;

    private XRBaseController activeController;
    private SplineContainer currentSpline;
    private bool isDrawing = false;

    [SerializeField] private Canvas optionsMenu;

    private SplineSegmentMeshExtruder extruder;
    private List<Segment3D> segments;
    private FirstSegmentVisualHelper visualHelper;
    public List<Spline> listOfSplines = new List<Spline>();

    private Vector3 lastKnotPosition = Vector3.zero;

    private GameState currentGameState;

    //wykrycie ukonczenia
    private int totalSegments = 0;
    private int coloredSegments = 0;
    private bool isHandlerSubscribed = false;

    //celnosc
    private Coroutine samplingCoroutine;
    private int totalSamples = 0;
    private int hitSamples = 0;
    private float accuracy = 0f;
    private bool isColoring = false;
    private float maxAllowedDistance = 0.02f;
    private float deltaMeasureTime = 0.3f;
    private float waitInSecondsAfterFinishing = 1f;

    //miara czasu
    private float startTime = -1f; // Czas rozpoczęcia rysowania
    private float totalDrawingTime = 0f; // Całkowity czas rysowania
    private float pauseStartTime = 0f;
    private float totalPauseDuration = 0f;
    private bool isCurrentlyPaused = false;


    public int counter = 0;
    void Awake()
    {
        GameManager.onGameStateChanged += GameManagerOnGameStateChanges; //DrawingPath subskrybuje GameManager
        visualHelper = this.GetComponent<FirstSegmentVisualHelper>(); // Pobraine visual helpera
    }

    private void OnDestroy()
    {
        GameManager.onGameStateChanged -= GameManagerOnGameStateChanges;
        if (isHandlerSubscribed)
        {
            Segment3D.OnSegmentColored -= OnSegmentColoredHandler;
            isHandlerSubscribed = false;
        }
    }

    private void GameManagerOnGameStateChanges(GameState newState)
    {
        currentGameState = newState;
    }

    public SplineContainer GetCurrentSpline()
    {
        return currentSpline;
    }

    public SplineContainer GetSplineContainerPrefab()
    {
        return splineContainerPrefab;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            HandlePauseToggle();
        }

        if (GameManager.instance.isPaused || currentGameState != GameState.DOCTOR_MODE) return; // sprawdzenie trybu gry oraz pauzy

        // Dynamiczne ustawienie aktywnego kontrolera
        if (!isDrawing)
        {
            if (rightController.activateInteractionState.active) activeController = rightController;
            else if (leftController.activateInteractionState.active) activeController = leftController;
        }

        if (activeController == null) return;

        if (activeController.activateInteractionState.active)
        {
            if (!isDrawing)
            {
                StartDrawing();
            }
            AddPoint();
        }
        else if (isDrawing)
        {
            StopDrawing();
        }
    }

    private void HandlePauseToggle()
    {
        GameManager.instance.isPaused = !GameManager.instance.isPaused;
        Debug.Log($"Pauza: {(GameManager.instance.isPaused ? "Włączona" : "Wyłączona")}");

        if (GameManager.instance.isPaused)
        {
            // Rozpocznij mierzenie czasu pauzy
            pauseStartTime = Time.time;
            isCurrentlyPaused = true;
        }
        else if (isCurrentlyPaused)
        {
            // Zakończ mierzenie czasu pauzy i dodaj do sumy
            float pauseDuration = Time.time - pauseStartTime;
            totalPauseDuration += pauseDuration;
            isCurrentlyPaused = false;
            Debug.Log($"Czas trwania pauzy: {pauseDuration} sekundy. Łącznie: {totalPauseDuration}");
        }
    }

    // Rozpocz�cie rysowania 
    void StartDrawing()
    {
        //ClearRecoloring();
        currentSpline = Instantiate(splineContainerPrefab, Vector3.zero, Quaternion.identity);
        extruder = currentSpline.gameObject.GetComponent<SplineSegmentMeshExtruder>();
        isDrawing = true;
        

        if (!isHandlerSubscribed)
        {
            Segment3D.OnSegmentColored += OnSegmentColoredHandler;
            isHandlerSubscribed = true;
        }
    }

    private IEnumerator SampleAccuracyRoutine()
    {
        while (isColoring)
        {
            SampleAccuracyPoint();
            yield return new WaitForSeconds(deltaMeasureTime); 
        }
    }

    private IEnumerator HandleFinishedColoring()
    {
        yield return new WaitForSeconds(waitInSecondsAfterFinishing);
        isColoring = false;
        if (samplingCoroutine != null)
            StopCoroutine(samplingCoroutine);

        accuracy = CalculateColoringAccuracy();

        extruder.ClearTrail();
        extruder.restoreSettings();
    }

    private void SampleAccuracyPoint()
{
    if (GameManager.instance.isPaused || activeController == null || currentSpline == null) return;

    Vector3 pos = activeController.transform.position;
    float3 nearestPoint;
    float t;

    var spline = currentSpline.Spline;
    SplineUtility.GetNearestPoint(spline, (float3)pos, out nearestPoint, out t);

    float distance = Vector3.Distance(pos, (Vector3)nearestPoint);
    totalSamples++;

    if (distance <= maxAllowedDistance)
    {
        hitSamples++;
    }
}


    private float CalculateColoringAccuracy()
    {
        if (totalSamples == 0)
        {
            return 0f;
        }

        float acc = (float)hitSamples / totalSamples * 100f;
        Debug.Log($"Celność: {acc}%");
        return acc;
    }

    private void OnSegmentColoredHandler(Segment3D seg)
    {
        if (GameManager.instance.isPaused) return;

        coloredSegments++;
        if(totalSegments == 0)
            totalSegments = extruder.getSegmentList().Count;

        if (coloredSegments == 1)
        {
            isColoring = true;
            samplingCoroutine = StartCoroutine(SampleAccuracyRoutine());
            startTime = Time.time;
            pauseStartTime = 0f;
            totalPauseDuration = 0f;
            totalSamples = 0;
            hitSamples = 0;
            accuracy = 0f;
            isColoring = false;
        }
        else if (coloredSegments >= totalSegments && coloredSegments > 0)
        {
            Debug.Log("Pokolorowano " + coloredSegments + "/" + totalSegments);
            Debug.Log("Wszystkie segmenty pokolorowane – automatyczne zakończenie rysowania.");
            Segment3D.OnSegmentColored -= OnSegmentColoredHandler;
            isHandlerSubscribed = false;
            totalSegments = 0;
            coloredSegments = 0;
            StartCoroutine(HandleFinishedColoring());
            if (startTime >= 0f)
            {
                totalDrawingTime = Time.time - startTime - totalPauseDuration;
                Debug.Log($"Czas rysowania: {totalDrawingTime} sekund.");
            }
        }
        else
        {
            Debug.Log("Pokolorowano " + coloredSegments + "/" + totalSegments);
        }
    }


    // Funkcja tworz�ca pojedynczy w�ze� krzywej Beziera
    void AddPoint()
    {
        // Wyznaczanie pozycji punktu
        Vector3 newPosition = activeController.transform.position;
        newPosition = currentSpline.transform.InverseTransformPoint(newPosition);
        BezierKnot knot = new BezierKnot(newPosition);

        // Przechowanie pozycji ostatnio dodanego punktu
        if (currentSpline.Spline.Count != 0)
        {
            BezierKnot lastKnot = currentSpline.Spline[currentSpline.Spline.Count - 1];
            lastKnotPosition = (Vector3)lastKnot.Position;
        }

        // Sprawdzenie czy punkt jest w odpowiedniej odleg�o�ci
        if (Vector3.Distance(newPosition, lastKnotPosition) > pointSpacing)
        {
            // Dodanie punktu do krzywej
            currentSpline.Spline.Add(knot);
            //

            lastKnotPosition = newPosition;

            if (config.getDrawingMode() && currentSpline.Spline.Count > 2)
            {
                // Ekstrudowanie pojedynczego segmentu mi�dzy dwoma poprzednimi w�z�ami(currentNode - 1 i currentNode - 2)
                extruder.ExtrudeSingleSegment(currentSpline.Spline, currentSpline.Spline.Count - 2);
                HapticController.SendHaptics(activeController, hapticIntensity, hapticDuration);
            }
        }
    }

    // Funkcja wykonywana po puszczeniu przycisku rysowania 
    void StopDrawing()
    {
        isDrawing = false;
        if (!config.getDrawingMode())
        {
            ExtrudeSpline();

            //extruder.Save(listOfSplines);
            //extruder.Load();
            //extruder.GenerateCirclePoints(0.1f,controller);
            //extruder.GeneratePolygonPoints(5, 0.1f, controller);
        }
        else
        {
            // Ekstrudowanie ostatniego segmentu natepuje po zako�czoniu rysowania aby dorysowa� �ciane kra�cow�
            extruder.ExtrudeSingleSegment(currentSpline.Spline, currentSpline.Spline.Count - 1, true);
            extruder.restoreSettings();
            // listOfSplines.Add(currentSpline.Spline);
        }
        listOfSplines.Add(currentSpline.Spline);

        var list = extruder.getSegmentList();
        if (list.Count >= 2)
        {
            visualHelper.setSegments(list);
            visualHelper.activateBlinkingSegment();
        }
    }



    // Ekstrudowanie ca�ego spline'a po sko�czeniu rysowania (liveDrawingMode = false) do testowania kolorowania
    void ExtrudeSpline()
    {
        extruder.ExtrudeAndApplyMaterials(currentSpline.Spline);
        //extruder.AddCollidersToSpline(currentSpline);
    }



    public void ClearRecoloring()
    {
        
        foreach (Spline spline in listOfSplines)
        {
            foreach (var knotIndex in System.Linq.Enumerable.Range(0, spline.Count - 1))
            {
                string segmentName = $"SplineSegmentMesh_{knotIndex}";
                GameObject segmentObj = GameObject.Find(segmentName);

                if (segmentObj != null)
                {
                    Segment3D segment = segmentObj.GetComponent<Segment3D>();
                    MeshRenderer renderer = segmentObj.GetComponent<MeshRenderer>();

                    if (segment != null && renderer != null)
                    {
                        segment.setColored(false);
                        renderer.sharedMaterial = originalMaterial;
                    }
                }
            }
        }

        coloredSegments = 0;
        totalSegments = 0;
        totalSamples = 0;
        hitSamples = 0;
        accuracy = 0f;
        isColoring = false;


        Debug.Log("Wyczyszczono pokolorowane segmenty.");
    }

}