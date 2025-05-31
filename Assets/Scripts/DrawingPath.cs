using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using System.IO;

public class DrawingPath : MonoBehaviour
{
    [SerializeField] private GameObject whiteboard;
    [SerializeField] private GameObject dot;
    [SerializeField] private Config config;
    [SerializeField] private PathManager pathManager;
    public InputActionReference primaryButtonAction;
    public InputActionReference savePathAction;
    public InputActionReference loadPathAction;

    [Header("UI Input Fields")]
    public InputField saveNameInputField;
    public InputField loadNameInputField;

    [SerializeField] private string pathFileName = "namedPathsData.json";

    private XRRayInteractor rayInteractor;
    private bool isHovering = false;
    [SerializeField] private float maxDotSpacing = 0.03f; // maksymalna odleglosc miedzy kropkami
    private Vector3 lastDotPosition = Vector3.zero; // pozycja ostatniej kropki

    // zmienne do pomiaru czasu kolorowania
    private bool coloringStarted = false;
    private float coloringStartTime = 0f;

    // zmienne do pomiaru dokadnoci
    private int totalClicks = 0;
    private int successfulClicks = 0;
    private float coloredHoverTimer = 0f;
    private DotRecolor lastColoredHit = null;
    private int consecutiveMisses = 0;

    private void Awake()
    {
        rayInteractor = FindObjectOfType<XRRayInteractor>();
        if (pathManager != null && pathManager.dotPrefab == null)
        {
            pathManager.dotPrefab = dot;
        }

        if (savePathAction != null)
        {
            savePathAction.action.performed += _ =>
            {
                if (saveNameInputField != null && !string.IsNullOrEmpty(saveNameInputField.text))
                {
                    SavePath(saveNameInputField.text);
                }
                else
                {
                    Debug.LogError("Pole InputField do zapisu nazwy jest nieprzypisane lub puste!");
                }
            };
        }
        if (loadPathAction != null)
        {
            loadPathAction.action.performed += _ =>
            {
                if (loadNameInputField != null && !string.IsNullOrEmpty(loadNameInputField.text))
                {
                    LoadPath(loadNameInputField.text);
                }
                else
                {
                    Debug.LogError("Pole InputField do wczytania nazwy jest nieprzypisane lub puste!");
                }
            };
        }
    }

    private void OnEnable()
    {
        if (savePathAction != null) savePathAction.action.Enable();
        if (loadPathAction != null) loadPathAction.action.Enable();
        primaryButtonAction?.action.Enable();
    }

    private void OnDisable()
    {
        if (savePathAction != null) savePathAction.action.Disable();
        if (loadPathAction != null) loadPathAction.action.Disable();
        primaryButtonAction?.action.Disable();
    }

    private void Update()
    {
        

        //tryb rysowania
        if (config.getDrawingMode())
        {
            //tryb usuwania
            if (config.getErasingMode())
            {
                if (isHovering && primaryButtonAction.action.ReadValue<float>() > 0)
                {
                    if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                    {
                        DotRecolor dotRemoval = hit.collider.GetComponent<DotRecolor>();
                        if (dotRemoval != null)
                        {
                            // Usuwanie kropek od koñca
                            int removalIndex = dotRemoval.dotIndex;
                            for (int i = pathManager.dots.Count - 1; i >= removalIndex; i--)
                            {
                                Destroy(pathManager.dots[i]);
                                pathManager.dots.RemoveAt(i);
                            }
                            // Aktualizacja lastDotPosition po usuniêciu kropek
                            lastDotPosition = (pathManager.dots.Count > 0) ?
                                              pathManager.dots[pathManager.dots.Count - 1].transform.position :
                                              Vector3.zero;
                        }
                    }
                }
            }
            else
            {
                //Debug.Log("Tryb rysowania wlaczony");
                if (isHovering && primaryButtonAction.action.ReadValue<float>() > 0)
                {
                    if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit) && hit.collider.gameObject == whiteboard)
                    {
                        Vector3 newPos = hit.point;
                        Debug.Log($"Hit Point (rysowanie): {newPos}");

                        // Jesli mamy zapisana poprzednia pozycje wypelniamy ewentualne luki
                        if (lastDotPosition != Vector3.zero)
                        {
                            float distance = Vector3.Distance(newPos, lastDotPosition);
                            if (distance > maxDotSpacing)
                            {
                                int numInterpolated = Mathf.FloorToInt(distance * 2 / maxDotSpacing);
                                for (int i = 1; i <= numInterpolated; i++)
                                {
                                    // interpolacja liniowa miedzy lastDotPosition a newPos
                                    float t = (float)i / (numInterpolated + 1);
                                    Vector3 interpPos = Vector3.Lerp(lastDotPosition, newPos, t);
                                    GameObject interpDot = Instantiate(dot, interpPos, Quaternion.Euler(0f, 0f, 90f));
                                    interpDot.AddComponent<SphereCollider>();
                                    if (pathManager != null)
                                        pathManager.AddDot(interpDot);
                                    else
                                        Debug.LogWarning("PathManager jest null!");
                                }
                            }
                        }
                        // dodajemy glowna kropke na nowej pozycji
                        GameObject newDot = Instantiate(dot, newPos, Quaternion.Euler(0f, 0f, 90f));
                        newDot.AddComponent<SphereCollider>();
                        if (pathManager != null)
                            pathManager.AddDot(newDot);
                        // aktualizacja pozycji ostatniej kropki
                        lastDotPosition = newPos;
                    }
                }
            }
        }
        else  //tryb kolorowania
        {
            // kolorowanie zakoñczone
            if (pathManager.coloringFinished)
            {
                if (coloringStarted)
                {
                    float elapsedTime = Time.time - coloringStartTime;
                    float accuracy = (totalClicks > 0) ? (successfulClicks / (float)totalClicks) * 100f : 0f;
                    Debug.Log($"Log {Time.frameCount}: Koniec kolorowania. Czas: {elapsedTime:F2}s, Trafienia: {successfulClicks}/{totalClicks} ({accuracy:F1}%).");
                    coloringStarted = false;
                    // Resetowanie licznikow
                    totalClicks = 0;
                    successfulClicks = 0;
                    consecutiveMisses = 0;
                }
                return;
            }

            // nacisniecie przycisku
            if (isHovering && primaryButtonAction.action.ReadValue<float>() > 0)
            {
                // pobranie trafienia
                if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                {
                    Debug.Log($"Hit Point (kolorowanie): {hit.point} - Trafiono w: {hit.collider.gameObject.name}");
                    DotRecolor dotRecolor = hit.collider.GetComponent<DotRecolor>();

                    if (dotRecolor != null)
                    {
                        //jak pokolorujemy kropke, zerujemy minusowe trafienia
                        consecutiveMisses = 0;

                        // Start kolorowania tylko przy pierwszej kropce
                        if (!coloringStarted && dotRecolor.dotIndex == 0 && !dotRecolor.IsColored)
                        {
                            coloringStartTime = Time.time;
                            coloringStarted = true;
                            Debug.Log($"Log {Time.frameCount}: Zaczêto kolorowanie!");


                            totalClicks++;
                            successfulClicks++;

                            Debug.Log($"Log {Time.frameCount}: Kliknieto nowa kropke o indeksie: " + dotRecolor.dotIndex);
                            dotRecolor.Recolor();

                            int hitIndex = dotRecolor.dotIndex;
                            for (int i = 1; i <= 6; i++)
                            {
                                if (hitIndex - i >= 0)
                                {
                                    DotRecolor neighborDot = pathManager.GetDot(hitIndex - i);
                                    if (neighborDot != null) neighborDot.Recolor();
                                }
                            }
                            pathManager.CheckAndRemoveDots();
                            lastColoredHit = null;
                            coloredHoverTimer = 0f;
                        }
                        // Podczas kolorowania
                        else if (coloringStarted)
                        {
                            //jesli kropka nie jest pokolorwana (i nie jest to pierwsze trafienie rozpoczynaj¹ce)
                            if (!dotRecolor.IsColored)
                            {
                                // zwiekszenie wszystkich klikniec i poprawnych
                                Debug.Log($"Log {Time.frameCount}: Udane klikniêcie! Kropka: {dotRecolor.dotIndex}. totalClicks wynosi {totalClicks + 1}, successfulClicks wynosi {successfulClicks + 1}");
                                totalClicks++;
                                successfulClicks++;
                                dotRecolor.Recolor();

                                //Zmiana koloru poprzednich kropek - zapobiega przenikaniu i lukom
                                int hitIndex = dotRecolor.dotIndex;
                                for (int i = 1; i <= 6; i++)
                                {
                                    if (hitIndex - i >= 0)
                                    {
                                        DotRecolor neighborDot = pathManager.GetDot(hitIndex - i);
                                        if (neighborDot != null) neighborDot.Recolor();
                                    }
                                }
                                pathManager.CheckAndRemoveDots();

                                // reset wskaznika
                                lastColoredHit = null;
                                coloredHoverTimer = 0f;

                                if (lastDotPosition != Vector3.zero)
                                    lastDotPosition = Vector3.zero;
                            }
                            else // trafienie w ju¿ pokolorowan¹ kropkê gdy trwa kolorowanie
                            {
                                if (lastColoredHit == dotRecolor)
                                {
                                    coloredHoverTimer += Time.deltaTime;
                                }
                                else
                                {
                                    lastColoredHit = dotRecolor;
                                    coloredHoverTimer = 0f;
                                }
                            }
                        }


                    }
                    else //nie trafienie w kropke
                    {
                        if (coloringStarted)
                        {
                            consecutiveMisses++; // licznik nietrafien
                            Debug.Log($"Log {Time.frameCount}: Nieudane klikniêcie! Hit: {hit.collider.gameObject.name}. B³êdne klikniêcia: {consecutiveMisses}. (TotalClicks: {totalClicks})");

                            if (consecutiveMisses >= 25) // manipulacja dokadnoœci¹ 25 nietrafien dopiero zmniejsza dokadnoœæ
                            {
                                Debug.Log($"Log {Time.frameCount}: Zapisana niedokadnoœæ. totalClicks wynosi {totalClicks + 1}");
                                totalClicks++; // zwiêkszamy totalClicks tylko po 25 nietrafieniach
                                consecutiveMisses = 0;
                            }
                            lastColoredHit = null;
                            coloredHoverTimer = 0f;
                        }

                    }
                }
                else // Laser nie trafil w nic
                {
                    if (coloringStarted)
                    {
                        Debug.Log($"Log {Time.frameCount}: Laser nie trafil w nic!");
                        lastColoredHit = null;
                        coloredHoverTimer = 0f;
                    }
                }

                // wyswietlanie pomiarow
                if (coloringStarted)
                {
                    float elapsedTime = Time.time - coloringStartTime;
                    float accuracy = (totalClicks > 0) ? (successfulClicks / (float)totalClicks) * 100f : 0f;
                    Debug.Log($"Log {Time.frameCount}: Statystyki - Czas: {elapsedTime:F2}s, Trafienia: {successfulClicks}/{totalClicks} ({accuracy:F1}%).");
                }
            }
            else
            {
                // gdy nie trzymamy przycisku
                lastColoredHit = null;
                coloredHoverTimer = 0f;
            }
        }
    }

    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        isHovering = true;
        //Debug.Log("isHovering ustawione na true");
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        isHovering = false;
        lastColoredHit = null;
        coloredHoverTimer = 0f;
        //Debug.Log("isHovering ustawione na false");
    }


    // Funkcja zapisu
    public void SavePath(string nameToSave)
    {
        if (pathManager == null)
        {
            Debug.LogError("PathManager jest nieprzypisany!");
            return;
        }
        if (string.IsNullOrEmpty(nameToSave))
        {
            Debug.LogError("Nie mo¿na zapisaæ z pust¹ œcie¿k¹.");
            return;
        }
        string fullPath = Path.Combine(Application.persistentDataPath, pathFileName);
        Debug.Log($"Zapisanie œcie¿ki '{nameToSave}' do: {fullPath}");

        pathManager.SaveNamedPath(nameToSave, fullPath);
    }

    // Funkcja wczytania
    public void LoadPath(string nameToLoad)
    {
        if (pathManager == null)
        {
            Debug.LogError("PathManager jest nieprzypisany!");
            return;
        }
        if (string.IsNullOrEmpty(nameToLoad))
        {
            Debug.LogError("Nie mo¿na wczytaæ z pust¹ œcie¿k¹.");
            return;
        }
        string fullPath = Path.Combine(Application.persistentDataPath, pathFileName);
        Debug.Log($"Œcie¿ka zapisu '{nameToLoad}' z: {fullPath}");

        if (pathManager.LoadNamedPath(nameToLoad, fullPath))
        {
            lastDotPosition = Vector3.zero;
            coloringStarted = false;
            totalClicks = 0;
            successfulClicks = 0;
            consecutiveMisses = 0;
            coloredHoverTimer = 0f;
            lastColoredHit = null;
            Debug.Log($"Œcie¿ka '{nameToLoad}' za³adowana.");
        }
        else
        {
            Debug.LogWarning($"Nie uda³o siê za³adowaæ œcie¿ki '{nameToLoad}'.");
        }
    }
}
