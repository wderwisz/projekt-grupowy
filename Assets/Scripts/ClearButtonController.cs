using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;

public class ClearButtonController : MonoBehaviour
{

    [SerializeField] private Button clearButton;
    private Spline spline;
    private SplineSegmentMeshExtruder[] splineExtruder;

    // Start is called before the first frame update
    void Start()
    {
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(FindSplineExtruder);
        }
        else
        {
            Debug.LogError("Brak przypisanego przycisku ClearButton!");
        }
    }

    void FindSplineExtruder()
    {
        //spline = FindObjectsByType
        splineExtruder = FindObjectsByType<SplineSegmentMeshExtruder>(0); //znalezienie wszystkich szlaków
        if (splineExtruder != null && clearButton != null)
        {
            foreach (SplineSegmentMeshExtruder extruder in splineExtruder)
            {
                extruder.ClearTrail();
            }
            
        }
        else
        {
            Debug.Log("Nie znaleziono szlaku albo buttona");
        }
    }

}
