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
        //w��czanie menu poprzez dolny trigger lewego kontrolera shift + G 
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
        // Pobieramy kierunek w kt�rym patrzy gracz (bez wp�ywu nachylenia g�ra/d�)
        Vector3 forward = player.forward;
        forward.y = 0; // Ignorujemy nachylenie g�owy gracza
        forward.Normalize();

        // Ustawiamy menu w odpowiedniej pozycji przed graczem
        Vector3 targetPosition = player.position + forward * menuDistance;
        menu.transform.position = Vector3.Lerp(menu.transform.position, targetPosition, Time.deltaTime * 10f);

        // Ustawiamy rotacj� menu, aby zawsze patrzy�o na gracza
        Quaternion targetRotation = Quaternion.LookRotation(forward);
        menu.transform.rotation = Quaternion.Slerp(menu.transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
    void PositionMenu()
    {
        // Pozycjonowanie menu przed graczem na jego wysoko�ci
        Vector3 forward = player.forward;
        forward.y = 0; // Usuwamy nachylenie w g�r�/d�, aby menu by�o na r�wnej wysoko�ci
        forward.Normalize();

        Vector3 menuPosition = player.position + forward * menuDistance;
        menu.transform.position = menuPosition;

        // Obracamy menu w stron� gracza
        Quaternion lookRotation = Quaternion.LookRotation(forward);
        menu.transform.rotation = lookRotation;
    }

    public void Save()
    {
        List<Spline> splines = drawingPathScript.listOfSplines;

        Debug.Log("lista splin�w " + splines.Count );

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
