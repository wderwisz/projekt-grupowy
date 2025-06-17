using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;
using UnityEngine.UI;

public class DrawingPath : MonoBehaviour
{
    [Header("Komponenty sceny")]
    [SerializeField] private GameObject whiteboard;
    [SerializeField] private GameObject dot;
    [SerializeField] private Config config;
    [SerializeField] private PathManager pathManager;
    [SerializeField] private MenuController2D menuController;

    [Header("Lewy kontroler")]
    [SerializeField] private XRRayInteractor leftHandRayInteractor;
    [SerializeField] private InputActionReference leftHandActivateAction;

    [Header("Prawy kontroler")]
    [SerializeField] private XRRayInteractor rightHandRayInteractor;
    [SerializeField] private InputActionReference rightHandActivateAction;

    [Header("Konfiguracja slidera")]
    [SerializeField] private Slider dotSizeSlider;

    [Header("Inne")]
    public InputActionReference savePathAction;
    public InputActionReference loadPathAction;

    // Zmienne wewnętrzne
    private GameState currentGameState;
    [SerializeField] private string pathFileName = "namedPathsData.json";
    [SerializeField] private float maxDotSpacing = 0.03f;
    private Vector3 lastDotPosition = Vector3.zero;
    private bool blockDrawingForOneFrame = false;
    private float currentPathSize = 1.0f;

    private const float holdThreshold = 0.2f;

    private class ControllerDrawState
    {
        public float buttonDownTime = -1f;
        public bool continuousDrawingStarted = false;
        public Vector3 clickDownPosition;
    }
    private ControllerDrawState leftHandState = new ControllerDrawState();
    private ControllerDrawState rightHandState = new ControllerDrawState();

    private bool coloringStarted = false;
    private float coloringStartTime = 0f;
    private int totalClicks = 0;
    private int successfulClicks = 0;
    private int consecutiveMisses = 0;

    private void Awake()
    {
        GameManager.onGameStateChanged += GameManagerOnGameStateChanges;

        if (leftHandRayInteractor == null || rightHandRayInteractor == null || leftHandActivateAction == null || rightHandActivateAction == null)
        {
            Debug.LogError("DrawingPath: Nie wszystkie referencje do kontrolerów i ich akcji zostały przypisane w inspektorze", this);
        }
        if (dotSizeSlider == null)
        {
            Debug.LogWarning("DrawingPath: Referencja do suwaka 'dotSizeSlider' nie jest ustawiona! Używany będzie domyślny rozmiar kropki.", this);
        }
    }

    private void OnDestroy()
    {
        GameManager.onGameStateChanged -= GameManagerOnGameStateChanges;
    }

    private void GameManagerOnGameStateChanges(GameState newState)
    {
        currentGameState = newState;
    }

    private void Update()
    {
        if (blockDrawingForOneFrame)
        {
            blockDrawingForOneFrame = false;
            return;
        }

        HandleDrawingInput(leftHandRayInteractor, leftHandActivateAction, leftHandState);
        HandleDrawingInput(rightHandRayInteractor, rightHandActivateAction, rightHandState);
    }

    private void HandleDrawingInput(XRRayInteractor interactor, InputActionReference actionRef, ControllerDrawState state)
    {
        if (interactor == null || actionRef?.action == null) return;
        var action = actionRef.action;

        if (currentGameState != GameState.DOCTOR_MODE || (currentGameState == GameState.DOCTOR_MODE && !menuController.getEareserState()))
        {
            if (action.IsPressed() && interactor.TryGetCurrent3DRaycastHit(out var hit))
            {
                if (currentGameState == GameState.PATIENT_MODE)
                {
                    HandlePatientMode(hit);
                }
                else if (currentGameState == GameState.DOCTOR_MODE && action.WasPressedThisFrame()) // gumka na klikniecie
                {
                    HandleEraser(hit);
                }
            }
            return;
        }

        // rysowanie
        if (action.WasPressedThisFrame())
        {
            if (interactor.TryGetCurrent3DRaycastHit(out var hit) && hit.collider.gameObject == whiteboard)
            {
                state.buttonDownTime = Time.time;
                state.continuousDrawingStarted = false;
                state.clickDownPosition = hit.point;
            }
        }

        if (action.IsPressed() && state.buttonDownTime > 0)
        {
            if (!state.continuousDrawingStarted && (Time.time - state.buttonDownTime > holdThreshold))
            {
                state.continuousDrawingStarted = true;
                HandleClickDrawing(state.clickDownPosition);
            }

            if (state.continuousDrawingStarted)
            {
                if (interactor.TryGetCurrent3DRaycastHit(out var hit) && hit.collider.gameObject == whiteboard)
                {
                    HandleContinuousDrawing(hit.point);
                }
            }
        }

        if (action.WasReleasedThisFrame() && state.buttonDownTime > 0)
        {
            if (!state.continuousDrawingStarted)
            {
                HandleClickDrawing(state.clickDownPosition);
            }

            state.buttonDownTime = -1f;
            state.continuousDrawingStarted = false;
        }
    }

    private void HandleClickDrawing(Vector3 newPos)
    {
        if (lastDotPosition == Vector3.zero)
        {
            if (dotSizeSlider != null)
            {
                currentPathSize = Mathf.Lerp(0.5f, 2.0f, dotSizeSlider.normalizedValue) / 10.0f;
            }
            else
            {
                currentPathSize = 0.1f;
            }
            CreateDotWithComponents(newPos);
        }
        else
        {
            float distance = Vector3.Distance(newPos, lastDotPosition);
            if (distance > maxDotSpacing)
            {
                int numInterpolated = Mathf.FloorToInt(distance / maxDotSpacing);
                for (int i = 1; i <= numInterpolated; i++)
                {
                    float t = (float)i / (float)(numInterpolated + 1);
                    Vector3 interpPos = Vector3.Lerp(lastDotPosition, newPos, t);
                    CreateDotWithComponents(interpPos);
                }
            }
            CreateDotWithComponents(newPos);
        }
        lastDotPosition = newPos;
    }

    private void HandleContinuousDrawing(Vector3 newPos)
    {
        if (lastDotPosition == Vector3.zero)
        {
            if (dotSizeSlider != null)
            {
                currentPathSize = Mathf.Lerp(0.5f, 2.0f, dotSizeSlider.normalizedValue) / 10.0f;
            }
            else
            {
                currentPathSize = 0.1f;
            }
        }

        if (lastDotPosition != Vector3.zero)
        {
            float distance = Vector3.Distance(newPos, lastDotPosition);
            if (distance > maxDotSpacing)
            {
                int numInterpolated = Mathf.FloorToInt(distance * 2 / maxDotSpacing);
                for (int i = 1; i <= numInterpolated; i++)
                {
                    float t = (float)i / (numInterpolated + 1);
                    Vector3 interpPos = Vector3.Lerp(lastDotPosition, newPos, t);
                    CreateDotWithComponents(interpPos);
                }
            }
        }
        CreateDotWithComponents(newPos);
        lastDotPosition = newPos;
    }
    // gumka
    private void HandleEraser(RaycastHit hit)
    {
        DotRecolor dotRemoval = hit.collider.GetComponent<DotRecolor>();
        if (dotRemoval != null)
        {
            int removalIndex = dotRemoval.dotIndex;
            for (int i = pathManager.dots.Count - 1; i >= removalIndex; i--)
            {
                Destroy(pathManager.dots[i]);
                pathManager.dots.RemoveAt(i);
            }
            lastDotPosition = (pathManager.dots.Count > 0) ? pathManager.dots[pathManager.dots.Count - 1].transform.position : Vector3.zero;
        }
    }

    /*
    private GameObject CreateDotWithComponents(Vector3 position)
    {
        Vector3 modifiedPosition = new Vector3(position.x * 1.01f, position.y, position.z);

        GameObject newDot = Instantiate(dot, modifiedPosition, Quaternion.Euler(0f, 0f, 90f));

        newDot.transform.localScale = Vector3.one * currentPathSize;

        if (newDot.GetComponent<SphereCollider>() == null) { newDot.AddComponent<SphereCollider>(); }
        DotRecolor dotRecolor = newDot.GetComponent<DotRecolor>();
        if (dotRecolor == null) { dotRecolor = newDot.AddComponent<DotRecolor>(); }

        dotRecolor.pathManagerInstance = pathManager;
        if (pathManager != null)
        {
            pathManager.AddDot(newDot);
            dotRecolor.dotIndex = pathManager.dots.Count - 1;
            dotRecolor.ApplyInitialVisuals();
        }
        else
        {
            Debug.LogError("PathManager jest null!");
        }
        return newDot;
    }
    */
    // zmiana rozmiaru kropki
    private GameObject CreateDotWithComponents(Vector3 position)
    {

       GameObject newDot = Instantiate(dot, position, Quaternion.Euler(0f, 0f, 90f));

       newDot.transform.localScale = Vector3.one * currentPathSize;
       if (newDot.GetComponent<SphereCollider>() == null) { newDot.AddComponent<SphereCollider>(); }
       DotRecolor dotRecolor = newDot.GetComponent<DotRecolor>();
       
       if (dotRecolor == null) { dotRecolor = newDot.AddComponent<DotRecolor>(); }
       dotRecolor.pathManagerInstance = pathManager;

       if (pathManager != null)
       {
           pathManager.AddDot(newDot);
           dotRecolor.dotIndex = pathManager.dots.Count - 1;
           dotRecolor.ApplyInitialVisuals();
       }
       else
       {
           Debug.LogError("PathManager jest null!");
       }

       return newDot;

    } 
    
    // usuwanie szlaku
    public void ResetDrawingState()
    {
        Debug.Log("DrawingPath: Resetting full drawing and coloring state.");
        lastDotPosition = Vector3.zero;
        coloringStarted = false;
        coloringStartTime = 0f;
        totalClicks = 0;
        successfulClicks = 0;
        consecutiveMisses = 0;
        blockDrawingForOneFrame = true;
        currentPathSize = 1.0f;
    }
    // tryb pacjenta
    private void HandlePatientMode(RaycastHit hit)
    {
        if (pathManager.coloringFinished)
        {
            if (coloringStarted)
            {
                float elapsedTime = Time.time - coloringStartTime;
                float accuracy = (totalClicks > 0) ? (successfulClicks / (float)totalClicks) * 100f : 0f;
                Debug.Log($"Log {Time.frameCount}: Koniec kolorowania. Czas: {elapsedTime:F2}s, Trafienia: {successfulClicks}/{totalClicks} ({accuracy:F1}%).");

                coloringStarted = false;
                totalClicks = 0;
                successfulClicks = 0;
                consecutiveMisses = 0;
            }
            return;
        }

        DotRecolor dotRecolor = hit.collider.GetComponent<DotRecolor>();
        if (dotRecolor != null)
        {
            consecutiveMisses = 0;
            if (!coloringStarted && dotRecolor.dotIndex == 0 && !dotRecolor.IsColored)
            {
                coloringStartTime = Time.time;
                coloringStarted = true;
                totalClicks++;
                successfulClicks++;
                ColorTheDotAndNeighbours(dotRecolor);
            }
            else if (coloringStarted && !dotRecolor.IsColored)
            {
                totalClicks++;
                successfulClicks++;
                ColorTheDotAndNeighbours(dotRecolor);
            }
        }
        else
        {
            if (coloringStarted)
            {
                consecutiveMisses++;
                if (consecutiveMisses >= 25)
                {
                    totalClicks++;
                    consecutiveMisses = 0;
                }
            }
        }
    }

    private void ColorTheDotAndNeighbours(DotRecolor dotToColor)
    {
        dotToColor.Recolor();
        int hitIndex = dotToColor.dotIndex;
        for (int i = 1; i <= 6; i++)
        {
            if (hitIndex - i >= 0)
            {
                DotRecolor neighborDot = pathManager.GetDot(hitIndex - i);
                if (neighborDot != null) neighborDot.Recolor();
            }
        }
        pathManager.CheckAndRemoveDots();
    }

    public void SavePath(string nameToSave)
    {
        if (pathManager == null) { Debug.LogError("PathManager jest nieprzypisany!"); return; }
        if (string.IsNullOrEmpty(nameToSave)) { Debug.LogError("Nie można zapisać z pustą ścieżką."); return; }
        string fullPath = Path.Combine(Application.persistentDataPath, pathFileName);
        Debug.Log($"Zapisanie ścieżki '{nameToSave}' do: {fullPath}");
        pathManager.SaveNamedPath(nameToSave, fullPath);
    }

    public void LoadPath(string nameToLoad)
    {
        if (pathManager == null) { Debug.LogError("PathManager jest nieprzypisany!"); return; }
        if (string.IsNullOrEmpty(nameToLoad)) { Debug.LogError("Nie można wczytać z pustą ścieżką."); return; }
        string fullPath = Path.Combine(Application.persistentDataPath, pathFileName);
        if (pathManager.LoadNamedPath(nameToLoad, fullPath))
        {
            ResetDrawingState();
            Debug.Log($"Ścieżka '{nameToLoad}' załadowana.");
        }
        else { Debug.LogWarning($"Nie udało się załadować ścieżki '{nameToLoad}'."); }
    }
}