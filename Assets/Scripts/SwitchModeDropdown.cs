using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchModeDropdown : MonoBehaviour
{
    public void SetGameMode(int index)
    {
        if (index == 1) //Tryb pacjenta
        {
            GameManager.instance.UpdateGameState(GameState.PATIENT_MODE);
        }
        else if (index == 0) //Tryb terapeuty
        {
            GameManager.instance.UpdateGameState(GameState.DOCTOR_MODE);
        }
    }
}
