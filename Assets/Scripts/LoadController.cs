using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using System.IO;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Linq;

public class LoadController : MonoBehaviour
{
    [SerializeField] private SplineContainer prefab;
    [SerializeField] private GameObject menu;
    [SerializeField] private DrawingPath3D drawingPathScript;
    [SerializeField] private XRRayInteractor leftRay;
    [SerializeField] private XRRayInteractor rightRay;
    [SerializeField] private MenuController menuController;


    public bool isMenuActive = false;
    public Transform player;
    public XRBaseController controller;
    public float menuDistance = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        isMenuActive = false;
        menu.SetActive(false);
        Camera mainCamera = Camera.main;
        player = mainCamera.transform;
        leftRay.enabled = false;
        rightRay.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        //w³¹czanie menu poprzez dolny trigger lewego kontrolera shift + G 
        //bool isPressed = controller.selectInteractionState.active;
        if (isMenuActive)
        {
            menu.SetActive(isMenuActive);
            PositionMenu();
            leftRay.enabled = true;
            rightRay.enabled = true;
            FollowPlayer();

        }
        if (controller.selectInteractionState.active && isMenuActive)
        {
            CloseMenu();
            menuController.CloseMenu();
            
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

    public void CloseMenu() // Funkcja do zamykania menu
    {
        isMenuActive = false;
        menu.SetActive(false);
        leftRay.enabled = false;
        rightRay.enabled = false;
    }

   
    public void Load(string filePath)
    {
        
        menuController.FindSplineExtruder();
        List<List<Vector3>> list = SaveLoadSplinePoints.LoadVector3List(Path.Combine(Application.persistentDataPath,"saves", filePath));
        foreach (var pointsList in list)
        {
            CreateSpline(pointsList);
        }

    }

    public void CreateSpline(List<Vector3> points)
    {

        Transform player = Camera.main.transform;
        Vector3 forward = player.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 newPosition = player.position + forward * 0.5f;
        newPosition.y = 0;
        Vector3 middlePoint = points[points.Count / 2];

        Vector3 up = Vector3.up;

        SplineContainer currentSpline = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        SplineSegmentMeshExtruder extruder = currentSpline.gameObject.GetComponent<SplineSegmentMeshExtruder>();

        Debug.Log(extruder);

        currentSpline.Spline.Clear();
        drawingPathScript.listOfSplines.Clear();
        foreach (var point in points)
        {
            Vector3 mappedPoint= point - middlePoint;
            Vector3 offsetVector = new Vector3(0.0f, player.position.y ,0.0f);
            currentSpline.Spline.Add(new BezierKnot(Quaternion.LookRotation(forward, up) * mappedPoint + offsetVector) );

        }
        drawingPathScript.listOfSplines.Add(currentSpline.Spline);
       
        extruder.ExtrudeAndApplyMaterials(currentSpline.Spline);
        currentSpline.transform.position =  newPosition ;
    }

}
