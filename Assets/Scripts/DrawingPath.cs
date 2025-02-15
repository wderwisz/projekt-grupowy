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
    [SerializeField] private float maxDotSpacing = 0.1f;
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
                                int numInterpolated = Mathf.FloorToInt(distance / maxDotSpacing);
                                for (int i = 1; i <= numInterpolated; i++)
                                {
                                    // interpolacja liniowa miedzy lastDotPosition a newPos
                                    float t = (float)i / (numInterpolated + 1);
                                    Vector3 interpPos = Vector3.Lerp(lastDotPosition, newPos, t);
                                    GameObject interpDot = Instantiate(dot, interpPos, Quaternion.Euler(0f, 0f, 90f));
                                    if (pathManager != null)
                                        pathManager.AddDot(interpDot);
                                    else
                                        Debug.LogWarning("PathManager jest null!");
                                }
                            }
                        }

                        // dodajemy glowna kropke na nowej pozycji
                        GameObject newDot = Instantiate(dot, newPos, Quaternion.Euler(0f, 0f, 90f));
                        if (pathManager != null)
                            pathManager.AddDot(newDot);


                        // aktualizacja pozycji ostatniej kropki
                        lastDotPosition = newPos;
                    }
                }
            }
        }
        else
        {
            //tryb kolorowania
            //Debug.Log("Tryb kolorowania wlaczony");
            int dotsLayer = LayerMask.NameToLayer("Dots");
            int dotsMask = 1 << dotsLayer;
            rayInteractor.raycastMask = dotsMask;



            if (isHovering)
            {
                if (primaryButtonAction.action.ReadValue<float>() > 0)
                {
                    if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                    {
                        Debug.Log($"Hit Point (kolorowanie): {hit.point} - Trafiono w: {hit.collider.gameObject.name}");
                        DotRecolor dotRecolor = hit.collider.GetComponent<DotRecolor>();
                        if (dotRecolor != null)
                        {
                            Debug.Log("Klikniêto kropkê o indeksie: " + dotRecolor.dotIndex);
                            dotRecolor.Recolor();
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
