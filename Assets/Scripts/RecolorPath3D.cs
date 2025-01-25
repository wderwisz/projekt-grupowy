using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RecolorPath3D : MonoBehaviour
{
    public Material newMaterial;
    private Material originalMaterial;
    private MeshRenderer[] meshRenderers;

    void Start()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers.Length > 0)
        {
            originalMaterial = meshRenderers[0].material;
        }
    }

    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        foreach (var renderer in meshRenderers)
        {
            renderer.material = newMaterial;
        }
        Debug.Log("Kontroler wszed³ w interakcjê ze spline'em.");
    }

    public void OnHoverExited(HoverExitEventArgs args)
    {
        foreach (var renderer in meshRenderers)
        {
            renderer.material = originalMaterial;
        }
    }
}
