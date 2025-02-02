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
    [SerializeField] [Range(0.01f, 2.0f)] private float pointSpacing = 0.1f;
    [SerializeField] private Config config;

    private SplineContainer currentSpline;
    private bool isDrawing = false;

    private SplineSegmentMeshExtruder extruder;
    private List<Segment3D> segments;

    private Vector3 lastKnotPosition = Vector3.zero;

    void Update()
    {
        //if (controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool isPressed) && isPressed)
        if(controller.activateInteractionState.active)
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

    // Rozpoczêcie rysowania 
    void StartDrawing()
    {
        currentSpline = Instantiate(splineContainerPrefab, Vector3.zero, Quaternion.identity);
        extruder = currentSpline.gameObject.GetComponent<SplineSegmentMeshExtruder>();
        isDrawing = true;
    }


    // Funkcja tworz¹ca pojedynczy wêze³ krzywej Beziera
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

        // Sprawdzenie czy punkt jest w odpowiedniej odleg³oœci
        if (Vector3.Distance(newPosition, lastKnotPosition) > pointSpacing)
        {
            // Dodanie punktu do krzywej
            currentSpline.Spline.Add(knot);
            lastKnotPosition = newPosition;

            if (config.getDrawingMode() && currentSpline.Spline.Count > 1)
            {
                // Ekstrudowanie pojedynczego segmentu wyznaczonego miêdzy obecnym a poprzednim wêz³em
                extruder.ExtrudeSingleSegment(currentSpline.Spline, currentSpline.Spline.Count - 1);
            }
        }
    }

    // Funkcja wykonywana po puszczeniu przycisku rysowania 
    void StopDrawing()
    {
        isDrawing = false;
        if(!config.getDrawingMode()) ExtrudeSpline();
    }

    // Ekstrudowanie ca³ego spline'a po skoñczeniu rysowania (liveDrawingMode = false) do testowania kolorowania
    void ExtrudeSpline()
    {
        extruder.ExtrudeAndApplyMaterials(currentSpline.Spline);
        //extruder.AddCollidersToSpline(currentSpline);
    }

}
