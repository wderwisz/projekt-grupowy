using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// KLASA NIEU�YWANA, DO USUNI�CIA

public class DestroyingSplineSegment : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggerring");
        // Sprawd�, czy obiekt to kontroler
        if (other.CompareTag("Controller"))
        {
            Debug.Log($"Usuwanie segmentu spline'a: {gameObject.name}");
            Destroy(gameObject); // Usuni�cie segmentu po kontakcie
        }
    }
}
