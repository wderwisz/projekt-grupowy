using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SwitchModeSlider : MonoBehaviour, IPointerClickHandler
{
    public Slider modeSlider;
    public Button backgroundButton; // Button tylko na t³o

    private void Start()
    {
        modeSlider.onValueChanged.AddListener(OnSliderChanged);
        backgroundButton.onClick.AddListener(OnBackgroundClicked);
        UpdateState((int)modeSlider.value);
    }

    private void OnBackgroundClicked()
    {
        // Klik na t³o zmienia stan
        if (modeSlider.value < 0.5f)
        {
            modeSlider.value = 1;
        }
        else
        {
            modeSlider.value = 0;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Po klikniêciu zmieniamy wartoœæ na przeciwn¹
        if (modeSlider.value < 0.5f)
        {
            modeSlider.value = 1;
        }
        else
        {
            modeSlider.value = 0;
        }
    }

    private void OnSliderChanged(float value)
    {
        UpdateState((int)value);
    }

    private void UpdateState(int value)
    {
        if (value == 1)
        {
            GameManager.instance.UpdateGameState(GameState.PATIENT_MODE);
        }
        else
        {
            GameManager.instance.UpdateGameState(GameState.DOCTOR_MODE);
        }
    }
}
