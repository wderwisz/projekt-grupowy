using UnityEngine;
using UnityEngine.XR;
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
    [SerializeField] private FinishBannerController bannerController;

    private SplineSegmentMeshExtruder extruder;
    private List<Segment3D> segments;
    private FirstSegmentVisualHelper visualHelper;
    public List<SplineContainer> listOfSplines = new List<SplineContainer>();

    private Vector3 lastKnotPosition = Vector3.zero;

    private GameState currentGameState;

    private int maxAmountOfSplines = 1;

    //wykrycie ukończenia
    private int totalSegments = 0;
    private int coloredSegments = 0;
    private bool isHandlerSubscribed = false;

    //celność
    private Coroutine samplingCoroutine;
    private int totalSamples = 0;
    private int hitSamples = 0;
    private float accuracy = 0f;
    private bool isColoring = false;
    private float maxAllowedDistance = 0.02f;
    private float deltaMeasureTime = 0.3f;
    private float waitInSecondsAfterFinishing = 0.1f;
    //miara czasu
    private float startTime = -1f; 
    private float totalDrawingTime = 0f; 
    private float pauseStartTime = 0f;
    private float totalPauseDuration = 0f;
    private bool isCurrentlyPaused = false;

    //pauza
    private bool wasPressedLastFrame = false;
    private bool wasMenuOnLastFrame = false;


    public int counter = 0;
    void Awake()
    {
        GameManager.onGameStateChanged += GameManagerOnGameStateChanges; //DrawingPath subskrybuje GameManager
        visualHelper = this.GetComponent<FirstSegmentVisualHelper>(); 
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

        if(currentGameState == GameState.OPTIONS_MENU_OPENED)
        {
            if(wasMenuOnLastFrame == false && GameManager.instance.isPaused == false)
            {
                HandlePauseToggle();
            }
            wasMenuOnLastFrame = true;
        }
        else
        {
            if(wasMenuOnLastFrame == true)
            {
                HandlePauseToggle();
            }
            //włączanie menu poprzez dolny trigger prawego kontrolera space + G 
            bool isPressed = rightController.selectInteractionState.active;
            if (isPressed && !wasPressedLastFrame) // Wykrycie momentu wciśnięcia
            {
                HandlePauseToggle();
            }
            wasPressedLastFrame = isPressed;
            wasMenuOnLastFrame = false;
        }
       

        if (GameManager.instance.isPaused || currentGameState != GameState.DOCTOR_MODE) return; // sprawdzenie trybu gry oraz pauzy

        // Dynamiczne ustawienie aktywnego kontrolera
        if (!isDrawing)
        {
            if (rightController.activateInteractionState.active) activeController = rightController;
            else if (leftController.activateInteractionState.active) activeController = leftController;
        }
        
        if (activeController == null) return;


        if (listOfSplines.Count >= maxAmountOfSplines)  return; // osiagnieto maksymalna liczbe szlakow


        if(listOfSplines.Count >= maxAmountOfSplines)  return; // osiagnieto maksymalna liczbe szlakow

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
    public void StartDrawing()
    {
        ClearRecoloring();
        coloredSegments = 0;
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
        if (optionsMenu.gameObject.activeInHierarchy == false)
        {
            bannerController?.ShowBanner(totalDrawingTime, accuracy);
        }
        
        extruder.restoreSettings();
    }

    private void SampleAccuracyPoint()
    {
        if (GameManager.instance.isPaused) return;

        if (listOfSplines == null || listOfSplines.Count == 0) return;

        Debug.Log("ILOSC SPLINOW NA LISCIE " + listOfSplines.Count);

        SplineContainer currentSplineContainer = listOfSplines[0];
        Spline spline = currentSplineContainer.Spline;

        Vector3 posRight = rightController.transform.position;
        Vector3 posLeft = leftController.transform.position;


        float3 posRightLocal = currentSplineContainer.transform.InverseTransformPoint(posRight);
        float3 posLeftLocal = currentSplineContainer.transform.InverseTransformPoint(posLeft);

        // Najbliższe punkty na spline lokalnie
        float3 nearestRightLocal, nearestLeftLocal;
        float tRight, tLeft;
        SplineUtility.GetNearestPoint(spline, posRightLocal, out nearestRightLocal, out tRight);
        SplineUtility.GetNearestPoint(spline, posLeftLocal, out nearestLeftLocal, out tLeft);

        // Najbliższe punkty względem sceny
        Vector3 nearestRightWorld = currentSplineContainer.transform.TransformPoint((Vector3)nearestRightLocal);
        Vector3 nearestLeftWorld = currentSplineContainer.transform.TransformPoint((Vector3)nearestLeftLocal);

        float distanceRight = Vector3.Distance(posRight, nearestRightWorld);
        float distanceLeft = Vector3.Distance(posLeft, nearestLeftWorld);


        Vector3 selectedPos;
        float distance;
        if (distanceRight < distanceLeft)
        {
            selectedPos = posRight;
            distance = distanceRight;
            activeController = rightController;
        }
        else
        {
            selectedPos = posLeft;
            distance = distanceLeft;
            activeController = leftController;
        }

        totalSamples++;

        float allowedDistance = maxAllowedDistance;

        SplineSegmentMeshExtruder extruder = currentSplineContainer.GetComponent<SplineSegmentMeshExtruder>();

        if (extruder != null)
        {
            float scaleFactor = extruder.getVectorScale();
            allowedDistance = maxAllowedDistance * scaleFactor;
        }

        if (distance <= allowedDistance)
        {
            hitSamples++;
        }

        Debug.Log("Distance!! " + distance);

        // Haptic feedback przy trafieniu
        if (distance <= allowedDistance && hapticIntensity > 0f)
        {
            activeController.SendHapticImpulse(hapticIntensity, hapticDuration);
        }

        float acc = (float)hitSamples / totalSamples * 100f;
        Debug.Log($"Celność: {acc}%");
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

    public void OnSegmentColoredHandler(Segment3D seg)
    {
        if (GameManager.instance.isPaused) return;

        coloredSegments++;
        if (totalSegments == 0)
        {
            //totalSegments = extruder.getSegmentList().Count;
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.StartsWith("SplineSegmentMesh_"))
                {
                    totalSegments++;
                }
            }
        }

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
        }
        else if (coloredSegments >= totalSegments && coloredSegments > 0 && totalSegments > 0)
        {
            Debug.Log("Pokolorowano " + coloredSegments + "/" + totalSegments);
            Debug.Log("Wszystkie segmenty pokolorowane – automatyczne zakończenie rysowania.");
            isColoring = false;
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


    // Funkcja tworzoca pojedynczy wezel krzywej Beziera
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

            lastKnotPosition = newPosition;

            if (config.getDrawingMode() && currentSpline.Spline.Count > 2)
            {
                // Ekstrudowanie pojedynczego segmentu miedzy dwoma poprzednimi wezlami(currentNode - 1 i currentNode - 2)
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
        }
        else
        {
            // Ekstrudowanie ostatniego segmentu natepuje po zako�czoniu rysowania aby dorysowa� �ciane kra�cow�
            extruder.ExtrudeSingleSegment(currentSpline.Spline, currentSpline.Spline.Count - 1, true);
            extruder.restoreSettings();
            // listOfSplines.Add(currentSpline.Spline);
        }
        listOfSplines.Add(currentSpline);

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
        
        foreach (SplineContainer splineContainer in listOfSplines)
        {
            Spline spline = splineContainer.Spline;
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
        isDrawing = false;
        FirstSegment.FindAndRecolor(1);



        Debug.Log("Wyczyszczono pokolorowane segmenty.");
    }

    public void RestartRecoloring()
    {
        coloredSegments = 0;
        totalSegments = 0;
        totalSamples = 0;
        hitSamples = 0;
        accuracy = 0f;
        isColoring = false;
    }

}
