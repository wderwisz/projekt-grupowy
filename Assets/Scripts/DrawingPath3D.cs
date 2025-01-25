using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Splines;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.UIElements;

public class DrawingPath3D : MonoBehaviour
{
    //public XRController controller; 
    public XRBaseController controller;
    public SplineContainer splineContainerPrefab; 
    public float pointSpacing = 0.2f;

    private SplineContainer currentSpline;
    private bool isDrawing = false;

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
            //Debug.Log("Stopped!");
            StopDrawing();
        }
        //else Debug.Log("Not drawing!");
    }

    void StartDrawing()
    {
        currentSpline = Instantiate(splineContainerPrefab, Vector3.zero, Quaternion.identity);
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
        }
        //Debug.Log(knot);
    }

    void AddCollidersToSpline()
    {
        for (int i = 0; i < currentSpline.Spline.Count - 1; i++)
        {
            GameObject colliderSegment = new GameObject($"SplineCollider_{i}");
            colliderSegment.transform.parent = currentSpline.transform;

            BoxCollider boxCollider = colliderSegment.AddComponent<BoxCollider>();

            Vector3 start = (Vector3)currentSpline.Spline[i].Position;
            Vector3 end = (Vector3)currentSpline.Spline[i + 1].Position;

            Vector3 midPoint = (start + end) / 2;
            colliderSegment.transform.position = midPoint;

            float segmentLength = Vector3.Distance(start, end);
            boxCollider.size = new Vector3(0.015f, 0.015f, segmentLength);

            colliderSegment.transform.LookAt(end);
            colliderSegment.tag = "SplineSegment";
        }
    }

    void StopDrawing()
    {
        isDrawing = false;
        ExtrudeSpline();
        AddCollidersToSpline();
    }

    void ExtrudeSpline()
    {
        SplineExtrude extrude = currentSpline.gameObject.GetComponent<SplineExtrude>();
        //Debug.Log("Extruded");
    }
}
