using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{

    //public InputActionProperty showMenuAction;
    [SerializeField] private GameObject menu;
    [SerializeField] private XRRayInteractor leftRay;
    [SerializeField] private XRRayInteractor rightRay;
    [SerializeField] private DrawingPath3D drawingPathScript;
    [SerializeField] private SaveController saveController;
    [SerializeField] private LoadController loadController;
    [SerializeField] private GameObject saveFileMenu;
    [SerializeField] private GameObject loadMenu;
    [SerializeField] private GameObject endMenu;
    [SerializeField] public Slider modeToggle;

    private bool isMenuActive = false;
    private bool wasPressedLastFrame = false;
    public XRBaseController controller;
    public Transform player;
    public float menuDistance = 1.5f;
    private Spline spline;
    private SplineSegmentMeshExtruder[] splineExtruder;
    private SplineContainer[] splineContainer;



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
        if (isPressed && !wasPressedLastFrame && !saveController.isMenuActive && !loadController.isMenuActive && !endMenu.activeSelf) // Wykrycie momentu wciœniêcia
        {
            GameManager.instance.UpdateGameState(GameState.OPTIONS_MENU_OPENED);
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
                CloseMenu();
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
    public void FindSplineExtruder() //funkcja do usuwania szlaku
    {

        splineExtruder = FindObjectsByType<SplineSegmentMeshExtruder>(0); //znalezienie wszystkich szlaków
        splineContainer = FindObjectsByType<SplineContainer>(0);
        if (splineExtruder != null)
        {
            foreach (SplineSegmentMeshExtruder mesh in splineExtruder)
            {
                mesh.ClearTrail();
            }
            foreach (SplineContainer spline in splineContainer)
            {
                Destroy(spline.gameObject);
            }
            drawingPathScript.listOfSplines.Clear();
        }
        else
        {
            Debug.Log("Nie znaleziono szlaku");
        }
    }
    public void CloseMenu() // Funkcja do zamykania menu
    {
        
        if (modeToggle.value == 0)
        {
            GameManager.instance.UpdateGameState(GameState.DOCTOR_MODE);
        }
        else
        {
            GameManager.instance.UpdateGameState(GameState.PATIENT_MODE);
        }

        isMenuActive = false;
        menu.SetActive(false);

        leftRay.enabled = false;
        rightRay.enabled = false;
    }


    public void SaveSpline()
    {
        saveController.isMenuActive = true;
        isMenuActive = false;
        menu.SetActive(false);

        leftRay.enabled = false;
        rightRay.enabled = false;

    }


    public void LoadSpline()
    {
        FindSplineExtruder();
        loadController.isMenuActive = true;
        isMenuActive = false;
        menu.SetActive(false);

        leftRay.enabled = false;
        rightRay.enabled = false;

    }
}
