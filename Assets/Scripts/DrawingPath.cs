using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class DrawingPath : MonoBehaviour
{
    [SerializeField] private GameObject whiteboard;
    [SerializeField] private GameObject dot;
    [SerializeField] private Config config;
    [SerializeField] private PathManager pathManager;
    public InputActionReference primaryButtonAction;
 
    private XRRayInteractor rayInteractor;
    private bool isHovering = false;

    // maksymalna odleglosc miedzy kropkami
    [SerializeField] private float maxDotSpacing = 0.03f;
    private float detectionRadius = 0.0f;  // Promieñ wykrywania
    private Vector3 lastDotPosition = Vector3.zero; // pozycja ostatniej kropki

    private void Awake()
    {
        rayInteractor = FindObjectOfType<XRRayInteractor>();

    }

    private void Update()
    {

        //tryb rysowania
        
        if (config.getDrawingMode())
        {
            //tryb usuwania
            if (config.getErasingMode())
            {
                if (isHovering)
                {
                    if (primaryButtonAction.action.ReadValue<float>() > 0)
                    {
                        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                        {
                            DotRecolor dotRemoval = hit.collider.GetComponent<DotRecolor>();
                            if (dotRemoval != null)
                            {
                                int removalIndex = dotRemoval.dotIndex;

                                // Usuwanie kropek od koñca
                                for (int i = pathManager.dots.Count - 1; i >= removalIndex; i--)
                                {
                                    // Zniszczenie obiektu kropki
                                    Destroy(pathManager.dots[i]);

                                    // Usuniêcie kropki z listy
                                    pathManager.dots.RemoveAt(i);
                                }

                                // Aktualizacja lastDotPosition po usuniêciu kropek
                                if (pathManager.dots.Count > 0)
                                {
                                    lastDotPosition = pathManager.dots[pathManager.dots.Count - 1].transform.position;
                                }
                                else
                                {
                                    lastDotPosition = Vector3.zero;
                                }
                            }
                        }
                    }
                }
            }
        
            else
            {
                //Debug.Log("Tryb rysowania wlaczony");
                if (isHovering)
                {
                    if (primaryButtonAction.action.ReadValue<float>() > 0)
                    {
                        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit) && hit.collider.gameObject == whiteboard)
                        {
                            Vector3 newPos = hit.point;
                            Debug.Log($"Hit Point (rysowanie): {newPos}");

                            // Jesli mamy zapisana poprzednia pozycje wypelniamy ewentualne luki
                            if (lastDotPosition != Vector3.zero)
                            {
                                float distance = Vector3.Distance(newPos, lastDotPosition);
                                if (distance > maxDotSpacing)
                                {
                                    int numInterpolated = Mathf.FloorToInt(distance * 2 / maxDotSpacing);
                                    for (int i = 1; i <= numInterpolated; i++)
                                    {
                                        // interpolacja liniowa miedzy lastDotPosition a newPos
                                        float t = (float)i / (numInterpolated + 1);
                                        Vector3 interpPos = Vector3.Lerp(lastDotPosition, newPos, t);
                                        GameObject interpDot = Instantiate(dot, interpPos, Quaternion.Euler(0f, 0f, 90f));
                                        interpDot.AddComponent<SphereCollider>();

                                        if (pathManager != null)
                                            pathManager.AddDot(interpDot);
                                        else
                                            Debug.LogWarning("PathManager jest null!");
                                    }
                                }
                            }

                            // dodajemy glowna kropke na nowej pozycji
                            GameObject newDot = Instantiate(dot, newPos, Quaternion.Euler(0f, 0f, 90f));
                            newDot.AddComponent<SphereCollider>();
                            if (pathManager != null)
                                pathManager.AddDot(newDot);


                            // aktualizacja pozycji ostatniej kropki
                            lastDotPosition = newPos;
                        }
                    }
                }
            }
        }
        else
        {
            //tryb kolorowania
            if (isHovering)
            {
                if (primaryButtonAction.action.ReadValue<float>() > 0)
                {
                    if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                    {
                        Debug.Log($"Hit Point (kolorowanie): {hit.point} - Trafiono w: {hit.collider.gameObject.name}");
                        DotRecolor dotRecolor = hit.collider.GetComponent<DotRecolor>();
                        if (dotRecolor != null) { 
                            Debug.Log("Klikniêto kropkê o indeksie: " + dotRecolor.dotIndex);
                            dotRecolor.Recolor();
                            int hitIndex = dotRecolor.dotIndex;
                           
                            //Zmiana koloru poprzednich kropek - zapobiega przenikaniu i lukom
                            for (int i = 1; i <= 6; i++)
                            {
                            
                                if (hitIndex - i >= 0)
                                {
                                    DotRecolor neighborDot = pathManager.GetDot(hitIndex - i);
                                    if (neighborDot != null)
                                    {
                                        neighborDot.Recolor();
                                    }
                                }


                            }


                            //Sprawdzamy czy szlak zosta³ w pe³ni odwzorowany
                            pathManager.CheckAndRemoveDots();
                            if (lastDotPosition != Vector3.zero)
                                lastDotPosition = Vector3.zero;


                        }
                        else
                        {
                            Debug.Log("Trafiono obiekt, ktory nie jest kropka.");
                        }
                    }
                    else
                    {
                        Debug.Log("Laser nie trafi³ w zaden obiekt.");
                    }
                }
            }
        }
    }

    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        isHovering = true;
        //Debug.Log("isHovering ustawione na true");
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        isHovering = false;
        //Debug.Log("isHovering ustawione na false");
    }
}
