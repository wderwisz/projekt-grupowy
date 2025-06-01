using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class PauseBannerController : MonoBehaviour
{

    public Transform head;
    public Transform leftController;
    public XRBaseController leftXRController;

    [SerializeField] public GameObject mainMenu;
    [SerializeField] public GameObject saveMenu;
    [SerializeField] public GameObject loadMenu;

    public Vector3 positionOffset = new Vector3(0.1f, 0.05f, 0.05f);
    public Vector3 rotationOffset = new Vector3(30f, 0f, 0f);

    [SerializeField] private GameObject pauseBanner; // np. obiekt z napisem "PAUZA"

    private void Awake()
    {
        GameManager.onGameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.onGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState newState)
    {
        // Zak³adamy, ¿e pauza jest aktywna niezale¿nie od trybu
        pauseBanner.SetActive(GameManager.instance.isPaused);
    }

    private void Update()
    {

        bool isAnyMenuOpen = saveMenu.activeSelf|| loadMenu.activeSelf|| mainMenu.activeSelf;
        // W razie, gdyby pauza zmienia³a siê bez zmiany GameState
        //if (pauseBanner.activeSelf != GameManager.instance.isPaused)
        //{
            pauseBanner.SetActive(GameManager.instance.isPaused && !isAnyMenuOpen);
        //}
    }


    private void PositionMenu()
    {
        transform.position = leftController.position +
                             leftController.right * positionOffset.x +
                             leftController.up * positionOffset.y +
                             leftController.forward * positionOffset.z;
        transform.rotation = leftController.rotation * Quaternion.Euler(rotationOffset);
    }
}
