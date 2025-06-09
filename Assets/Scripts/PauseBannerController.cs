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
    public Transform player;
    public float menuDistance = 1.5f;

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
        // Zak�adamy, �e pauza jest aktywna niezale�nie od trybu
        pauseBanner.SetActive(GameManager.instance.isPaused);
    }

    private void Update()
    {

        bool isAnyMenuOpen = saveMenu.activeSelf|| loadMenu.activeSelf|| mainMenu.activeSelf;
        // W razie, gdyby pauza zmienia�a si� bez zmiany GameState
        //if (pauseBanner.activeSelf != GameManager.instance.isPaused)
        //{
            pauseBanner.SetActive(GameManager.instance.isPaused && !isAnyMenuOpen);
            if (pauseBanner.activeSelf)
        {
            FollowPlayer(); 
        }
        //}
    }


    void FollowPlayer()
    {
        // Pobieramy kierunek w kt�rym patrzy gracz (bez wp�ywu nachylenia g�ra/d�)
        Vector3 forward = player.forward;
        forward.y = 0; // Ignorujemy nachylenie g�owy gracza
        forward.Normalize();

        // Ustawiamy menu w odpowiedniej pozycji przed graczem
        Vector3 targetPosition = player.position + forward * menuDistance;
        pauseBanner.transform.position = Vector3.Lerp(pauseBanner.transform.position, targetPosition, Time.deltaTime * 10f);

        // Ustawiamy rotacj� menu, aby zawsze patrzy�o na gracza
        Quaternion targetRotation = Quaternion.LookRotation(forward);
        pauseBanner.transform.rotation = Quaternion.Slerp(pauseBanner.transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
    void PositionMenu()
    {
        // Pozycjonowanie menu przed graczem na jego wysoko�ci
        Vector3 forward = player.forward;
        forward.y = 0; // Usuwamy nachylenie w g�r�/d�, aby menu by�o na r�wnej wysoko�ci
        forward.Normalize();

        Vector3 menuPosition = player.position + forward * menuDistance;
        pauseBanner.transform.position = menuPosition;

        // Obracamy menu w stron� gracza
        Quaternion lookRotation = Quaternion.LookRotation(forward);
        pauseBanner.transform.rotation = lookRotation;
    }
}
