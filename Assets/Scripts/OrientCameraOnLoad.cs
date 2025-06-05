using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientCameraOnLoad : MonoBehaviour
{
    public Transform whiteboardTarget;

    void Start()
    {
        if (whiteboardTarget == null)
        {
            GameObject whiteboardObject = GameObject.Find("Whiteboard");
            if (whiteboardObject != null)
            {
                whiteboardTarget = whiteboardObject.transform;
            }
            else
            {
                Debug.LogError("Cel (tablica) nie jest ustawiony i nie mo¿na go znaleŸæ po nazwie 'Whiteboard' w skrypcie OrientCameraOnLoad!");
                return;
            }
        }

        // Skierowanie kamery na tablicê
        transform.LookAt(whiteboardTarget);

    }
}