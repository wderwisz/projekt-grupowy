using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameState state;

    public XRBaseController controller;

    public static event Action<GameState> onGameStateChanged;

    private bool wasPressedLastFrame = false;

    public bool isPaused = false;

    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UpdateGameState(GameState.DOCTOR_MODE);
    }

    public GameState GetGameState()
    {
        return state;
    }
    public void UpdateGameState(GameState newState)
    {
        state = newState;

        switch (newState)
        {
            case GameState.PATIENT_MODE:
                HandlePatientMode();
                break;
            case GameState.DOCTOR_MODE:
                HandleDoctorMode();
                break;
            case GameState.OPTIONS_MENU_OPENED:
                HandleOptionsMenuOpened();
                break;
            case GameState.PAUSE:
                HandlePause();
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(newState), newState, null); 
        }

        onGameStateChanged?.Invoke(newState); //Informowanie komponent�w o zmianie stanu.

    }

    private void HandleOptionsMenuOpened()
    {
        //Debug.Log("Menu opcji zosta�o otwarte");
    }

    private void HandleDoctorMode()
    {
        Debug.Log("Tryb lekarza aktywny");
    }

    private void HandlePatientMode()
    {
        Debug.Log("Tryb pacjenta aktywny");
        
    }

    private void HandlePause()
    {
        Debug.Log("Pauza w��czona - GAMEMANAGER");
    }

    private void Update()
    {
        //bool isPressed = controller.selectInteractionState.active; // Sprawdzanie czy wci�ni�to

        //if (isPressed && !wasPressedLastFrame) // Wykrycie momentu wci�ni�cia
        //{
        //    UpdateGameState(state == GameState.DOCTOR_MODE ? GameState.PATIENT_MODE : GameState.DOCTOR_MODE);
        //    Debug.Log("Tryb zmieniony na: " + state);
        //}

        //wasPressedLastFrame = isPressed; // Zapami�tanie stanu na kolejn� klatk�
    }
}

public enum GameState
{
    PATIENT_MODE, 
    DOCTOR_MODE,
    OPTIONS_MENU_OPENED,
    PAUSE
}
