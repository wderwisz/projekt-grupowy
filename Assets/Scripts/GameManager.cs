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
            default:
                throw new System.ArgumentOutOfRangeException(nameof(newState), newState, null); //zobaczymy czy to dziala
        }

        onGameStateChanged?.Invoke(newState); //Informowanie komponentów o zmianie stanu.

    }

    private void HandleOptionsMenuOpened()
    {
        //Debug.Log("Menu opcji zosta³o otwarte");
    }

    private void HandleDoctorMode()
    {
        Debug.Log("Tryb lekarza aktywny");
    }

    private void HandlePatientMode()
    {
        Debug.Log("Tryb pacjenta aktywny");
        
    }

    private void Update()
    {
        //bool isPressed = controller.selectInteractionState.active; // Sprawdzanie czy wciœniêto

        //if (isPressed && !wasPressedLastFrame) // Wykrycie momentu wciœniêcia
        //{
        //    UpdateGameState(state == GameState.DOCTOR_MODE ? GameState.PATIENT_MODE : GameState.DOCTOR_MODE);
        //    Debug.Log("Tryb zmieniony na: " + state);
        //}

        //wasPressedLastFrame = isPressed; // Zapamiêtanie stanu na kolejn¹ klatkê
    }


}


public enum GameState
{
    PATIENT_MODE, 
    DOCTOR_MODE,
    OPTIONS_MENU_OPENED,
}
