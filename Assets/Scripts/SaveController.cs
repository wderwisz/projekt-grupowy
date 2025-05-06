using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using System.IO;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;

public class SaveController : MonoBehaviour
{
    [SerializeField] private SplineContainer prefab;
    [SerializeField] private GameObject menu;
    [SerializeField] private DrawingPath3D drawingPathScript;
    [SerializeField] private MenuController menuController;
    [SerializeField] private XRRayInteractor leftRay;
    [SerializeField] private XRRayInteractor rightRay;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private NonNativeKeyboard keyboard;

    public bool isMenuActive = false;
    public Transform player;
    public XRBaseController controller;
    public float menuDistance = 1.5f;
    

    // Start is called before the first frame update
    void Start()
    {
        isMenuActive = false;
        menu.SetActive(false);
        Camera mainCamera = Camera.main;
        player = mainCamera.transform;
        leftRay.enabled = false;
        rightRay.enabled = false;
        inputField.onSelect.AddListener(x => OpenKeyboard());
    }

    // Update is called once per frame
    void Update()
    {
        //w³¹czanie menu poprzez dolny trigger lewego kontrolera shift + G 
        //bool isPressed = controller.selectInteractionState.active;
        if (isMenuActive)
        {
            GameManager.instance.UpdateGameState(GameState.OPTIONS_MENU_OPENED);
            menu.SetActive(isMenuActive);
            PositionMenu();
            leftRay.enabled = true;
            rightRay.enabled = true;
            FollowPlayer();
            
        }
        if (controller.selectInteractionState.active && isMenuActive)
        {
            CloseMenu();
        }
    }

    public void OpenKeyboard()
    {
        keyboard.InputField = inputField;
        keyboard.PresentKeyboard(inputField.text);
    }

    public void CloseMenu() // Funkcja do zamykania menu
    {
        if (menuController.modeToggle.value == 0)
        {
            GameManager.instance.UpdateGameState(GameState.DOCTOR_MODE);
        }
        else
        {
            GameManager.instance.UpdateGameState(GameState.PATIENT_MODE);
        }
        isMenuActive = false;
        menu.SetActive(false);
        keyboard.gameObject.SetActive(false);
        leftRay.enabled = false;
        rightRay.enabled = false;
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

    public void Save()
    {
        List<Spline> splines = drawingPathScript.listOfSplines;

        Debug.Log("lista splinów " + splines.Count );

        string fileName = inputField.text + ".json";
        Debug.Log(fileName);
        try
        {
            string saveDir = Path.Combine(Application.persistentDataPath, "saves");
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }

            SaveLoadSplinePoints.SaveVector3List(
                Path.Combine(saveDir, fileName),
                splines);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save spline points: {ex.Message}");
        }
       // SaveLoadSplinePoints.SaveVector3List(Path.Combine(Application.persistentDataPath, fileName), splines);
        //SaveLoadSplinePoints.SaveVector3List(Path.Combine(Application.persistentDataPath + "/saves/", fileName), splines);
        CloseMenu();
    }

    

}
