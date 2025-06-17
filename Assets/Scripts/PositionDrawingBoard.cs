using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PositionDrawingBoard : MonoBehaviour
{
    [Header("Referencja do kamery")]
    public Camera playerCamera;

    [Header("Komponenty do zarz¹dzania")]
    public List<Renderer> renderersToToggle;

    public List<Collider> collidersToToggle;

    [Header("Ustawienia pozycji tablicy")]
    public float distanceFromPlayer = 1.5f;

    void Awake()
    {
        // programowo wylaczamy komponenty
        ToggleComponents(false);
    }

    void Start()
    {
        if (playerCamera == null)
        {
            Debug.LogError("[PositionDrawingBoard]: Pole 'playerCamera' nie jest przypisane! Nie mo¿na spozycjonowaæ tablicy.", this);
            return; // przerwij jesli nie ma kamery
        }

        // korutyna, ktora poczeka, spozycjonuje i wlaczy widocznosc
        StartCoroutine(PositionAndShowAfterFrameEnd());
    }

    private IEnumerator PositionAndShowAfterFrameEnd()
    {
        // czekamy na koniec pierwszej klatki, aby system XR zdazyl zaktualizowaæ pozycje kamery
        yield return new WaitForEndOfFrame();

        // pozycjonujemy tablice 
        PositionSelfInFrontOfPlayer();

        // wlaczamy widocznosc komponentow
        Debug.Log("[PositionDrawingBoard]: Pozycjonowanie zakoñczone. W³¹czam widocznoœæ komponentów.");
        ToggleComponents(true);
    }

    void PositionSelfInFrontOfPlayer()
    {
        Transform cameraTransform = playerCamera.transform;

        Vector3 forwardDirection = cameraTransform.forward;
        forwardDirection.y = 0;
        forwardDirection.Normalize();

        transform.position = cameraTransform.position + forwardDirection * distanceFromPlayer;
        transform.rotation = Quaternion.LookRotation(forwardDirection);

        Debug.Log($"[PositionDrawingBoard]: Ustawiono pozycjê na {transform.position}");
    }

    private void ToggleComponents(bool isEnabled)
    {
        foreach (Renderer rend in renderersToToggle)
        {
            if (rend != null)
            {
                rend.enabled = isEnabled;
            }
        }

        foreach (Collider coll in collidersToToggle)
        {
            if (coll != null)
            {
                coll.enabled = isEnabled;
            }
        }
    }
}