using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEditor;

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
        //Debug.Log(bannerPanel.activeSelf);
        //if (bannerPanel.activeSelf)
        //{
            FollowPlayer();
        //}

    }
    public void ShowBanner(float time, float accuracy)
    {
        PositionMenu();
        if (bannerPanel == null || timeText == null || accuracyText == null)
        {
            return;
        }

        bannerPanel.SetActive(true);
        timeText.text = $"Czas: {time:F2} s";
        accuracyText.text = $"Celnoœæ: {accuracy:F1} %";
        GameManager.instance.UpdateGameState(GameState.PAUSE);
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

    void FollowPlayer()
    {
        // Pobieramy kierunek w którym patrzy gracz (bez wp³ywu nachylenia góra/dó³)
        Vector3 forward = player.forward;
        forward.y = 0; // Ignorujemy nachylenie g³owy gracza
        forward.Normalize();

        // Ustawiamy menu w odpowiedniej pozycji przed graczem
        Vector3 targetPosition = player.position + forward * menuDistance;
        bannerPanel.transform.position = Vector3.Lerp(bannerPanel.transform.position, targetPosition, Time.deltaTime * 10f);

        // Ustawiamy rotacjê menu, aby zawsze patrzy³o na gracza
        Quaternion targetRotation = Quaternion.LookRotation(forward);
        bannerPanel.transform.rotation = Quaternion.Slerp(bannerPanel.transform.rotation, targetRotation, Time.deltaTime * 10f);
    }

}
