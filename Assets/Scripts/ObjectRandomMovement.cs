using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectRandomMovement : MonoBehaviour
{
    public GameObject dot;
    public HoverEnterEventArgs HoverEnterEvent;
    public HoverExitEventArgs HoverExitEvent;

    private Vector3 lastPositionVector = Vector3.zero;

    [SerializeField]
    private float minDistanceAfterMovement = 0.25f;

    private void Awake()
    {
        //lastPositionVector = Vector3.zero;
    }

    private Vector3 getNewRandomPositionVector(Vector3 oldVector)
    {
        Vector3 newVector;
        do{
            newVector = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        } while (Vector3.Distance(newVector, oldVector) < minDistanceAfterMovement); 

        return newVector;
    }

    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        Debug.Log($"{args.interactorObject} hovered over {args.interactableObject}", this);
        Vector3 v3 = getNewRandomPositionVector(lastPositionVector);
        dot.transform.localPosition = v3;
        lastPositionVector = v3;
    }

    public void OnHoverExited(HoverExitEventArgs args)
    {
        Debug.Log($"{args.interactorObject} stopped hovering over {args.interactableObject}", this);
    }

    void Update()
    {
        
    }
}
