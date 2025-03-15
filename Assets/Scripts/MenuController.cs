using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
public class MenuController : MonoBehaviour
{

    //public InputActionProperty showMenuAction;
    [SerializeField] private GameObject menu;
    [SerializeField] private XRRayInteractor leftRay;
    [SerializeField] private XRRayInteractor rightRay;
    private bool isMenuActive = false;
    private bool wasPressedLastFrame = false;
    public XRBaseController controller;
    public Transform player;
    public float menuDistance = 1.5f;



    // Start is called before the first frame update
    void Start()
    {
        isMenuActive = false;
        menu.SetActive(false);
        Camera mainCamera = Camera.main;
        player = mainCamera.transform;
        leftRay.enabled =  false;
        rightRay.enabled =  false;
    }

    // Update is called once per frame
    void Update()
    {
        //w³¹czanie menu poprzez dolny trigger lewego kontrolera shift + G 
        bool isPressed = controller.selectInteractionState.active;
        if (isPressed && !wasPressedLastFrame) // Wykrycie momentu wciœniêcia
        {
            isMenuActive = !isMenuActive;
            menu.SetActive(isMenuActive);
            if (isMenuActive)
            {
                PositionMenu();
                leftRay.enabled = true;
                rightRay.enabled = true;
            }
            else
            {
                leftRay.enabled = false;
                rightRay.enabled = false;
            }
        }

        if (isMenuActive)
        {
            FollowPlayer();
        }

        wasPressedLastFrame = isPressed;
    }

    void FollowPlayer()
    {
        // Pobieramy kierunek w którym patrzy gracz (bez wp³ywu nachylenia góra/dó³)
        Vector3 forward = player.forward;
        forward.y = 0; // Ignorujemy nachylenie g³owy gracza
        forward.Normalize();

        // Ustawiamy menu w odpowiedniej pozycji przed graczem
        Vector3 targetPosition = player.position + forward * menuDistance;
        menu.transform.position = Vector3.Lerp(menu.transform.position, targetPosition, Time.deltaTime * 10f);

        // Ustawiamy rotacjê menu, aby zawsze patrzy³o na gracza
        Quaternion targetRotation = Quaternion.LookRotation(forward);
        menu.transform.rotation = Quaternion.Slerp(menu.transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
    void PositionMenu()
    {
        // Pozycjonowanie menu przed graczem na jego wysokoœci
        Vector3 forward = player.forward;
        forward.y = 0; // Usuwamy nachylenie w górê/dó³, aby menu by³o na równej wysokoœci
        forward.Normalize();

        Vector3 menuPosition = player.position + forward * menuDistance;
        menu.transform.position = menuPosition;

        // Obracamy menu w stronê gracza
        Quaternion lookRotation = Quaternion.LookRotation(forward);
        menu.transform.rotation = lookRotation;
    }

    public void CloseMenu() // Funkcja do zamykania menu
    {
        isMenuActive = false;
        menu.SetActive(false);
    }
}
