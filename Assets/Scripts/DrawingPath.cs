using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class DrawingPath : MonoBehaviour
{
    [SerializeField] private GameObject whiteboard;
    [SerializeField] private GameObject dot;
    [SerializeField] private Config config;
    [SerializeField] private PathManager pathManager;
    public InputActionReference primaryButtonAction;

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
    private int consecutiveMisses = 0; // Licznik kolejnych nietrafień

    private void Awake()
    {
        rayInteractor = FindObjectOfType<XRRayInteractor>();
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
                            // Usuwanie kropek od ko�ca
                            int removalIndex = dotRemoval.dotIndex;
                            for (int i = pathManager.dots.Count - 1; i >= removalIndex; i--)
                            {
                                Destroy(pathManager.dots[i]);
                                pathManager.dots.RemoveAt(i);
                            }
                            // Aktualizacja lastDotPosition po usuni�ciu kropek
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
            // kolorowanie zakończone
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
                            Debug.Log($"Log {Time.frameCount}: Zaczęto kolorowanie!");


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
                            //jesli kropka nie jest pokolorwana (i nie jest to pierwsze trafienie rozpoczynające)
                            if (!dotRecolor.IsColored)
                            {
                                // zwiekszenie wszystkich klikniec i poprawnych
                                Debug.Log($"Log {Time.frameCount}: Udane kliknięcie! Kropka: {dotRecolor.dotIndex}. totalClicks wynosi {totalClicks + 1}, successfulClicks wynosi {successfulClicks + 1}");
                                totalClicks++;
                                successfulClicks++;
                                Debug.Log("Kliknieto nowa kropke o indeksie: " + dotRecolor.dotIndex);
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
                            else // trafienie w już pokolorowaną kropkę gdy trwa kolorowanie
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
                            Debug.Log($"Log {Time.frameCount}: Nieudane kliknięcie! Hit: {hit.collider.gameObject.name}. Błędne kliknięcia: {consecutiveMisses}. (TotalClicks: {totalClicks})");
                            
                            if (consecutiveMisses >= 25) // manipulacja dokładnością 25 nietrafien dopiero zmniejsza dokładność
                            {
                                Debug.Log($"Log {Time.frameCount}: Zapisana niedokładność. totalClicks wynosi {totalClicks + 1}");
                                totalClicks++; // zwiększamy totalClicks tylko po 25 nietrafieniach
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
}
