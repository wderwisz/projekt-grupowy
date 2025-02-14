using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class DrawingPath : MonoBehaviour
{
    [SerializeField]
    private GameObject whiteboard;

    [SerializeField]
    private GameObject dot;

    private Config config;

    public InputActionReference primaryButtonAction;

    private XRRayInteractor rayInteractor;

    private bool isHovering = false;

    private PathManager pathManager;

    private void Awake()
    {
        rayInteractor = FindObjectOfType<XRRayInteractor>();
        if (config == null)
        {
            config = Resources.Load<Config>("MainConfig");
            if (config == null)
                Debug.LogError("Nie uda³o siê za³adowaæ Config z Resources!");
        }
    }


    private void Update()
    {
        if (config == null)
        {
            Debug.LogError("Config nie zosta³ przypisany!");
            return; 
        }

        if (config.getDrawingMode())
        {
            if (isHovering)
            {
                if (primaryButtonAction.action.ReadValue<float>() > 0)  // wcisniecie przycisku
                {
                    if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit) && hit.collider.gameObject == whiteboard)
                    {
                        Debug.Log($"Hit Point: {hit.point}");
                        GameObject newDot = Instantiate(dot, hit.point, Quaternion.Euler(0f, 0f, 90f));
                        if (pathManager != null)
                            pathManager.AddDot(newDot);
                    }
                }
                else
                {
                    Debug.Log("No hit");
                }
            }
        }
        else
        {
            // tryb kolorowania
        }
    }


    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        isHovering = true;
    }
    public void OnHoverExit(HoverExitEventArgs args)
    {
        isHovering = false;
    }
}
