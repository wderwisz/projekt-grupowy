using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Splines;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System;

public class DrawingPath3D : MonoBehaviour
{
    //public XRController controller; 
    public XRBaseController controller;
    public SplineContainer splineContainerPrefab;
    [SerializeField]
    private float pointSpacing = 0.1f;

    private SplineContainer currentSpline;
    private bool isDrawing = false;

    private SplineSegmentMeshExtruder extruder;

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

    void StartDrawing()
    {
        currentSpline = Instantiate(splineContainerPrefab, Vector3.zero, Quaternion.identity);
        extruder = currentSpline.gameObject.GetComponent<SplineSegmentMeshExtruder>();
        isDrawing = true;
    }

    void AddPoint()
    {
        Vector3 newPosition = controller.transform.position;
        newPosition = currentSpline.transform.InverseTransformPoint(newPosition);
        BezierKnot knot = new BezierKnot(newPosition);

        if (currentSpline.Spline.Count != 0)
        {
            BezierKnot lastKnot = currentSpline.Spline[currentSpline.Spline.Count - 1];
            lastKnotPosition = (Vector3)lastKnot.Position;
        }

        if (Vector3.Distance(newPosition, lastKnotPosition) > pointSpacing)
        {
            currentSpline.Spline.Add(knot);
            lastKnotPosition = newPosition;
            extruder.ExtrudeSingleSegment(currentSpline.Spline, currentSpline.Spline.Count - 1);
        }
    }

    void StopDrawing()
    {
        isDrawing = false;
        //ExtrudeSpline();
    }

    void ExtrudeSpline()
    {
        extruder.ExtrudeAndApplyMaterials(currentSpline.Spline);
        //extruder.AddCollidersToSpline(currentSpline);
    }

}
