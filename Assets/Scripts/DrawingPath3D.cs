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
    //public XRController controller; 
    public XRBaseController controller;
    public SplineContainer splineContainerPrefab;

    [SerializeField][Range(0.0001f, 2.0f)] private float pointSpacing = 0.1f;
    [SerializeField] private Config config;

    [SerializeField][Range(0f, 1f)] private float hapticIntensity = 0.2f;
    [SerializeField] private float hapticDuration = 0.15f;

    private SplineContainer currentSpline;
    private bool isDrawing = true;

    private SplineSegmentMeshExtruder extruder;
    private List<Segment3D> segments;

    private Vector3 lastKnotPosition = Vector3.zero;

    private GameState currentGameState;


    void Awake()
    {
        GameManager.onGameStateChanged += GameManagerOnGameStateChanges; //DrawingPath subskrybuje GameManager
    }

    private void OnDestroy()
    {
        GameManager.onGameStateChanged -= GameManagerOnGameStateChanges;
    }

    private void GameManagerOnGameStateChanges(GameState newState)
    {
        currentGameState = newState;
    }

    void Update()
    {
        //currentGameState = GameManager.instance.state;
        if (currentGameState != GameState.DOCTOR_MODE) return; //Sprawdzenie trybu gry

        //if (controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool isPressed) && isPressed)
        if (controller.activateInteractionState.active)
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
        Vector3 newPosition = controller.transform.position;
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
                // Ekstrudowanie pojedynczego segmentu mi�dzy dwoma poprzednimi w�z�ami(currentNode - 1 i currentNode - 2)
                extruder.ExtrudeSingleSegment(currentSpline.Spline, currentSpline.Spline.Count - 2);
                HapticController.SendHaptics(controller, hapticIntensity, hapticDuration);
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
        }

    }



    // Ekstrudowanie ca�ego spline'a po sko�czeniu rysowania (liveDrawingMode = false) do testowania kolorowania
    void ExtrudeSpline()
    {
        extruder.ExtrudeAndApplyMaterials(currentSpline.Spline);
        //extruder.AddCollidersToSpline(currentSpline);
    }

}
