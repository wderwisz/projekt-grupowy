using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RecolorPath3D : MonoBehaviour
{
    public Material newMaterial;
    private Material originalMaterial;
    private MeshRenderer[] meshRenderers;

     private void OnTriggerEnter(Collider other)
     {
        Debug.Log("Triggerring");
        // Sprawdü, czy obiekt to kontroler
        if (other.CompareTag("Controller"))
        {
            Debug.Log($"Usuwanie segmentu spline'a: {gameObject.name}");

            
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            //renderer.sharedMaterial = newMaterial;
        }
     }

}
