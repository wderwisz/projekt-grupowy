using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SizeSlider : MonoBehaviour
{

    [SerializeField] private UnityEngine.UI.Slider slider;
    [SerializeField] private TMPro.TextMeshProUGUI sliderText;
    [SerializeField] private SplineSegmentMeshExtruder splineExtruder;

    public float sizeValue = 0;

    private void Start()
    {

        Scene activeScene = SceneManager.GetActiveScene();
        string sceneName = activeScene.name;

        float valueFromPlayerPrefs = PlayerPrefs.GetFloat("Size" + sceneName); // Uzale¿nienie wielkoœci od sceny
        float roundedValue = Mathf.Round(valueFromPlayerPrefs * 100.0f) / 100.0f; // Zaokr¹glenie do 2 miejsc po przecinku
        slider.value = roundedValue;

        splineExtruder.setVectorScale(slider.value); //ustawianie wartoœæi slidera od razu po w³¹czeniu sceny

        sliderText.text = slider.value.ToString();
        sizeValue = slider.value;
        slider.onValueChanged.AddListener((v) =>
        {
            sliderText.text = v.ToString("0.00");
            setSize(v);
            splineExtruder.setVectorScale(v);
        });
    }

    private void Update()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        string sceneName = activeScene.name;
        slider.value = sizeValue;
        PlayerPrefs.SetFloat("Size"+sceneName, sizeValue);
    }


    private void setSize(float volume)
    {
        sizeValue = volume;
    }
}
