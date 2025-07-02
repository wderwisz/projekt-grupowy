using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class FinishBanner2D : MonoBehaviour
{
    [SerializeField] private GameObject bannerPanel;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI accuracyText;
    [SerializeField] private XRRayInteractor leftRay;
    [SerializeField] private XRRayInteractor rightRay;
    public Transform player;
    public float menuDistance = 1.5f;
    public void Awake()
    {
        bannerPanel.SetActive(false);
    }

    public void Update()
    {
        PositionMenu();
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

    void PositionMenu()
    {
        // Pozycjonowanie menu przed graczem na jego wysokoœci
        Vector3 forward = player.forward;
        forward.y = 0; // Usuwamy nachylenie w górê/dó³, aby menu by³o na równej wysokoœci
        forward.Normalize();

        Vector3 menuPosition = player.position + forward * menuDistance;
        bannerPanel.transform.position = menuPosition;

        // Obracamy menu w stronê gracza
        Quaternion lookRotation = Quaternion.LookRotation(forward);
        bannerPanel.transform.rotation = lookRotation;
    }

}
