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
            if (pauseBanner.activeSelf)
        {
            FollowPlayer(); 
        }
        //}
    }


    void FollowPlayer()
    {
        // Pobieramy kierunek w którym patrzy gracz (bez wp³ywu nachylenia góra/dó³)
        Vector3 forward = player.forward;
        forward.y = 0; // Ignorujemy nachylenie g³owy gracza
        forward.Normalize();

        // Ustawiamy menu w odpowiedniej pozycji przed graczem
        Vector3 targetPosition = player.position + forward * menuDistance;
        pauseBanner.transform.position = Vector3.Lerp(pauseBanner.transform.position, targetPosition, Time.deltaTime * 10f);

        // Ustawiamy rotacjê menu, aby zawsze patrzy³o na gracza
        Quaternion targetRotation = Quaternion.LookRotation(forward);
        pauseBanner.transform.rotation = Quaternion.Slerp(pauseBanner.transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
    void PositionMenu()
    {
        // Pozycjonowanie menu przed graczem na jego wysokoœci
        Vector3 forward = player.forward;
        forward.y = 0; // Usuwamy nachylenie w górê/dó³, aby menu by³o na równej wysokoœci
        forward.Normalize();

        Vector3 menuPosition = player.position + forward * menuDistance;
        pauseBanner.transform.position = menuPosition;

        // Obracamy menu w stronê gracza
        Quaternion lookRotation = Quaternion.LookRotation(forward);
        pauseBanner.transform.rotation = lookRotation;
    }
}
