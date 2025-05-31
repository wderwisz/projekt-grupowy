using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class WristMenuController : MonoBehaviour
{
    public Transform head;
    public Transform leftController;
    private Transform wrist;
    public GameObject menu;
    public XRBaseController leftXRController;
    [SerializeField] private  InputActionReference Xbuttonleft;
    [SerializeField] private GameObject drawingPoint;
    [SerializeField] private FreeDrawingCustomizer drawingCustomizer;
    private bool isMenuActive = false;

    private Color presentColor;

    public float activationAngle = 60f; // k¹t, po którym menu siê pokazuje
    public Vector3 positionOffset = new Vector3(0.1f, 0.05f, 0.05f);
    public Vector3 rotationOffset = new Vector3(30f, 0f, 0f);
    // Domyœlny materia³
    public Material material;


    void OnEnable()
    {
        Xbuttonleft.action.performed += OnButtonClicked;
        drawingCustomizer.OnColorChanged += HandleColorChange;
    }

    void OnDisable()
    {
        Xbuttonleft.action.performed -= OnButtonClicked;
        drawingCustomizer.OnColorChanged -= HandleColorChange;
    }
    void OnButtonClicked(InputAction.CallbackContext context)
    {
        isMenuActive = !isMenuActive;
        Debug.Log("Button clicked!");
        menu.SetActive(isMenuActive);
        
    }

    private void Start()
    {
        menu.SetActive(false);
        wrist = leftController;
        Camera mainCamera = Camera.main;
        Transform head = mainCamera.transform;

        transform.position = wrist.position +
                 wrist.right * positionOffset.x +
                 wrist.up * positionOffset.y +
                 wrist.forward * positionOffset.z;

        // Ustaw rotacjê z offsetem
        transform.rotation = wrist.rotation * Quaternion.Euler(rotationOffset);
        

    }
    void Update()
    {


    }

    private void HandleColorChange(Color newColor)
    {
            Debug.Log("zmiana koloru");
            presentColor = newColor;

            Renderer renderer = drawingPoint.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = presentColor;
            }
    }

    private void PositionMenu()
    {
        transform.position = leftController.position +
                             leftController.right * positionOffset.x +
                             leftController.up * positionOffset.y +
                             leftController.forward * positionOffset.z;
        transform.rotation = leftController.rotation * Quaternion.Euler(rotationOffset);
    }






}
