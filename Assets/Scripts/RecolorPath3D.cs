using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class RecolorPath3D : MonoBehaviour
{
    public Material newMaterial;
    public Material blinkingMaterial;
    [SerializeField] private FirstSegmentVisualHelper visualHelper;

    private GameState currentGameState;
    private GameObject previousSegment = null;
    private GameObject currentSegment;

    [SerializeField][Range(0f,1f)] private float hapticIntensity = 0.3f;
    [SerializeField] private float hapticDuration = 0.1f;


    public DrawingPath3D drawingPath3D;

    void Start()
    {
        drawingPath3D = GetComponent<DrawingPath3D>();

        // Jeśli nie znaleziono, spróbuj znaleźć go w obiekcie nadrzędnym lub innych obiektach
        if (drawingPath3D == null)
        {
            drawingPath3D = FindObjectOfType<DrawingPath3D>();
        }
    }

    public void setPreviousSegment(GameObject segment)
    {
        previousSegment = segment;
    }

    public void setCurrentSegment(GameObject segment)
    {
        currentSegment = segment;
    }

    private void GameManagerOnGameStateChanges(GameState newState)
    {
        currentGameState = newState;
    }


    // Funkcja wywo�ywana po dotkni�ciu przez collider kontrolera 
    private void OnTriggerEnter(Collider other)
    {

        currentGameState = GameManager.instance.state;

        if (GameManager.instance.isPaused || currentGameState != GameState.PATIENT_MODE) return; //Sprawdzenie trybu gry

        if (other.CompareTag("Controller"))
        {
            Segment3D segment = currentSegment.GetComponent<Segment3D>();
            bool hasSegmentZero = GameObject.Find("SplineSegmentMesh_0") != null;

            // Wyciągnij numer segmentu z nazwy tego obiektu
            int currentIndex = int.Parse(gameObject.name.Replace("SplineSegmentMesh_", ""));
 
            // Jeśli mamy segment_0, to pierwszy segment ma index 0, w przeciwnym razie - 1
            int expectedFirstIndex = hasSegmentZero ? 0 : 1;

            // Sprawdzenie, czy to pierwszy segment i czy nie jest pokolorowany
            if (currentIndex == expectedFirstIndex && !segment.isColored())
            {
                drawingPath3D.StartDrawing();
            }
            // Sprawdzenie czy poprzedni segment pokolorowany
            if (previousSegment != null && !previousSegment.GetComponent<Segment3D>().isColored()) return;

            // Sprawdzenie czy obecny jest ju� pomalowany
            if(currentSegment.GetComponent<Segment3D>().isColored()) return;

            currentSegment.GetComponent<Segment3D>().setColored(true);

            //Debug.Log($"Kolorowanie segmentu spline'a: {gameObject.name}");

            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = newMaterial;

            XRBaseController controller = other.GetComponentInParent<XRBaseController>();
            HapticController.SendHaptics(controller, hapticIntensity, hapticDuration);

            
        }
    }

}
