using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class FinishBannerController : MonoBehaviour
{
    [SerializeField] private GameObject bannerPanel;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI accuracyText;
    [SerializeField] private XRRayInteractor leftRay;
    [SerializeField] private XRRayInteractor rightRay;
    public void Awake()
    {
        bannerPanel.SetActive(false);
    }
    public void ShowBanner(float time, float accuracy)
    {
        if (bannerPanel == null || timeText == null || accuracyText == null)
        {
            Debug.LogWarning("FinishBannerController is not fully assigned!");
            return;
        }

        bannerPanel.SetActive(true);
        timeText.text = $"Czas: {time:F2} s";
        accuracyText.text = $"Celnoœæ: {accuracy:F1} %";
        leftRay.enabled = true;
        rightRay.enabled = true;
    }

    public void HideBanner()
    {
        bannerPanel.SetActive(false);


    }

    
}
