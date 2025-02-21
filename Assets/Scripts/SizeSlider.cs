using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeSlider : MonoBehaviour
{

    [SerializeField] private UnityEngine.UI.Slider slider;
    [SerializeField] private TMPro.TextMeshProUGUI sliderText;
    [SerializeField] private SplineSegmentMeshExtruder splineExtruder;

    public float sizeValue = 0;

    // Start is called before the first frame update
    private void Start()
    {

        slider.value = PlayerPrefs.GetFloat("Size");
        sliderText.text = slider.value.ToString();
        sizeValue = slider.value;
        slider.onValueChanged.AddListener((v) =>
        {
            sliderText.text = v.ToString("0.00");
            setSize(v);
            splineExtruder.setVectorScale(v);
        });
    }

    // Update is called once per frame
    private void Update()
    {
        slider.value = sizeValue;
        PlayerPrefs.SetFloat("Size", sizeValue);
    }


    private void setSize(float volume)
    {
        sizeValue = volume;
    }
}
