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

    private SplineSegmentMeshExtruder[] splineExtruder;
    public bool isMenuActive = false;
    public Transform player;
    public XRBaseController controller;
    public float menuDistance = 1.5f;

    Vector3 lastPoint = Vector3.zero;

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
        //włączanie menu poprzez dolny trigger lewego kontrolera shift + G 
        //bool isPressed = controller.selectInteractionState.active;
        if (isMenuActive)
        {
            GameManager.instance.UpdateGameState(GameState.OPTIONS_MENU_OPENED);
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
        // Pobieramy kierunek w kt�rym patrzy gracz (bez wp�ywu nachylenia g�ra/d�)
        Vector3 forward = player.forward;
        forward.y = 0; // Ignorujemy nachylenie g�owy gracza
        forward.Normalize();

        // Ustawiamy menu w odpowiedniej pozycji przed graczem
        Vector3 targetPosition = player.position + forward * menuDistance;
        menu.transform.position = Vector3.Lerp(menu.transform.position, targetPosition, Time.deltaTime * 10f);

        // Ustawiamy rotacj� menu, aby zawsze patrzy�o na gracza
        Quaternion targetRotation = Quaternion.LookRotation(forward);
        menu.transform.rotation = Quaternion.Slerp(menu.transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
    void PositionMenu()
    {
        // Pozycjonowanie menu przed graczem na jego wysoko�ci
        Vector3 forward = player.forward;
        forward.y = 0; // Usuwamy nachylenie w g�r�/d�, aby menu by�o na r�wnej wysoko�ci
        forward.Normalize();

        Vector3 menuPosition = player.position + forward * menuDistance;
        menu.transform.position = menuPosition;

        // Obracamy menu w stron� gracza
        Quaternion lookRotation = Quaternion.LookRotation(forward);
        menu.transform.rotation = lookRotation;
    }

    public void CloseMenu() // Funkcja do zamykania menu
    {

        if (menuController.modeToggle.value == 0)
        {
            GameManager.instance.UpdateGameState(GameState.DOCTOR_MODE);
        }
        else
        {
            GameManager.instance.UpdateGameState(GameState.PATIENT_MODE);
        }
        isMenuActive = false;
        menu.SetActive(false);
        leftRay.enabled = false;
        rightRay.enabled = false;
    }

   
    public void Load(string filePath)
    {    
        //menuController.FindSplineExtruder();
        List<List<Vector3>> list = SaveLoadSplinePoints.LoadVector3List(Path.Combine(Application.persistentDataPath,"saves", filePath));
        menuController.FindSplineExtruder();
        foreach (var pointsList in list)
        {
            CreateSpline(pointsList);
        }
        lastPoint = Vector3.zero;

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
        float distance = 0.0f;
        if (lastPoint != Vector3.zero)
        {
            distance = Vector3.Distance(lastPoint, middlePoint);

        }

        Vector3 direction = player.right; // wektor kierunku rysowania kolejnych szlakow
        Vector3 offset = direction.normalized * distance; 

        lastPoint = middlePoint;
        Vector3 up = Vector3.up;

        SplineContainer currentSpline = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        SplineSegmentMeshExtruder extruder = currentSpline.gameObject.GetComponent<SplineSegmentMeshExtruder>();

        Debug.Log(extruder);
        //currentSpline.Spline.Clear();
      
        foreach (var point in points)
        {
            Vector3 mappedPoint= point - middlePoint;
            Vector3 offsetVector = new Vector3(0.0f, player.position.y ,0.0f) + offset;
            currentSpline.Spline.Add(new BezierKnot(Quaternion.LookRotation(forward, up) * mappedPoint + offsetVector) );

        }
        drawingPathScript.listOfSplines.Add(currentSpline);
        drawingPathScript.ClearRecoloring();

        extruder.ExtrudeAndApplyMaterials(currentSpline.Spline);
        currentSpline.transform.position =  newPosition ;
        
        //Zmiana koloru pierwszego segmentu
        FirstSegment.FindAndRecolor(0);
    }

}
