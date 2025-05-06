using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Splines;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;

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

    public int counter = 0;
    void Awake()
    {
        GameManager.onGameStateChanged += GameManagerOnGameStateChanges; //DrawingPath subskrybuje GameManager
        visualHelper = this.GetComponent<FirstSegmentVisualHelper>(); // Pobraine visual helpera
    }

    private void OnDestroy()
    {
        GameManager.onGameStateChanged -= GameManagerOnGameStateChanges;
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
        if (currentGameState != GameState.DOCTOR_MODE) return; //Sprawdzenie trybu gry

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

    // Rozpocz�cie rysowania 
    void StartDrawing()
    {
        currentSpline = Instantiate(splineContainerPrefab, Vector3.zero, Quaternion.identity);
        extruder = currentSpline.gameObject.GetComponent<SplineSegmentMeshExtruder>();
        isDrawing = true;
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

        Debug.Log("Wyczyszczono pokolorowane segmenty.");
    }

}
