using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// KLASA NIEU¯YWANA, DO USUNIÊCIA

public class DestroyingSplineSegment : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggerring");
        // SprawdŸ, czy obiekt to kontroler
        if (other.CompareTag("Controller"))
        {
            Debug.Log($"Usuwanie segmentu spline'a: {gameObject.name}");
            Destroy(gameObject); // Usuniêcie segmentu po kontakcie
        }
    }
}
