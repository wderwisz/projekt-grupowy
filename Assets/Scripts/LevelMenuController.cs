using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMenuController : MonoBehaviour
{
    public Transform player;
    public float menuDistance = 1.5f;
    [SerializeField] private GameObject menu;
    private bool isMenuActive;
    // Start is called before the first frame update
    void Start()
    {
        Camera mainCamera = Camera.main;
        player = mainCamera.transform;
        isMenuActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMenuActive)
        {
            FollowPlayer();
        }
    }

    void FollowPlayer()
    {
        // Pobieramy kierunek w którym patrzy gracz (bez wp³ywu nachylenia góra/dó³)
        Vector3 forward = player.forward;
        forward.y = 0; // Ignorujemy nachylenie g³owy gracza
        forward.Normalize();

        // Ustawiamy menu w odpowiedniej pozycji przed graczem
        Vector3 targetPosition = player.position + forward * menuDistance;
        menu.transform.position = Vector3.Lerp(menu.transform.position, targetPosition, Time.deltaTime * 10f);

        // Ustawiamy rotacjê menu, aby zawsze patrzy³o na gracza
        Quaternion targetRotation = Quaternion.LookRotation(forward);
        menu.transform.rotation = Quaternion.Slerp(menu.transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
    void PositionMenu()
    {
        // Pozycjonowanie menu przed graczem na jego wysokoœci
        Vector3 forward = player.forward;
        forward.y = 0; // Usuwamy nachylenie w górê/dó³, aby menu by³o na równej wysokoœci
        forward.Normalize();

        Vector3 menuPosition = player.position + forward * menuDistance;
        menu.transform.position = menuPosition;

        // Obracamy menu w stronê gracza
        Quaternion lookRotation = Quaternion.LookRotation(forward);
        menu.transform.rotation = lookRotation;
    }
}
