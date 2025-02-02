using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using CommonUsages = UnityEngine.XR.CommonUsages;

public class DrawingPath : MonoBehaviour
{
    [SerializeField]
    private GameObject whiteboard;

    [SerializeField]
    private GameObject dot;

    public InputActionReference primaryButtonAction;

    private XRRayInteractor rayInteractor;

    private bool isHovering = false;


    private void Awake()
    {
        rayInteractor = FindObjectOfType<XRRayInteractor>();
    }

    // Dwie funkcje wywo³ywane przez RayInteractor
    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        isHovering = true;
    }
    public void OnHoverExit(HoverExitEventArgs args)
    {
        isHovering = false;
    }

    private void Update()
    {
        if (isHovering)
        {
            if (primaryButtonAction.action.ReadValue<float>() > 0)  // Wciœniêcie przycisku
            {
                // Tworzenie kropek na powierzchni whiteboarda
                if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit) && hit.collider.gameObject == whiteboard)
                {
                    Debug.Log($"Hit Point: {hit.point}");
                    Instantiate(dot, hit.point, Quaternion.Euler(0f, 0f, 90f));
                }
            }
            else
            {
                Debug.Log("No hit");
            }
        }

    }
}
