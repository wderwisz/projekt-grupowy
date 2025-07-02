using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Linq;

public class PathManager : MonoBehaviour
{
    [SerializeField] private FinishBanner2D finishBannerController;
    [System.Serializable]
    private class NamedPathData
    {
        public string name;
        public List<Vector3> dotPositions = new List<Vector3>();
    }

    [System.Serializable]
    private class SavedPathsCollection
    {
        public List<NamedPathData> savedPaths = new List<NamedPathData>();
    }

    public List<GameObject> dots = new List<GameObject>();
    public GameObject dotPrefab;

    public int nextDotIndex = 0;

    public int coloredDots = 0;

    public bool coloringFinished = false;

    private float delayInSeconds = 1.0f;

    public DrawingPath drawingPathInstance;

    private void Awake()
    {
        if (drawingPathInstance == null)
        {
            drawingPathInstance = FindObjectOfType<DrawingPath>();
            if (drawingPathInstance != null)
            {
                Debug.Log("PathManager: Pomyślnie znaleziono i przypisano instancję DrawingPath", this);
            }
            else
            {
                Debug.LogError("PathManager: Nie udało się znaleźć obiektu ze skryptem DrawingPath w scenie", this);
            }
        }
    }

    //dodawanie kropki do listy 
    public void AddDot(GameObject dot)
    {
        dots.Add(dot);
        Debug.Log("Liczba kropek w ścieżcie: " + dots.Count);

        // ustaw indeks kropki
        DotRecolor dotRecolor = dot.GetComponent<DotRecolor>();
        if (dotRecolor != null)
        {
            dotRecolor.dotIndex = dots.Count - 1;
            dotRecolor.pathManagerInstance = this;
            Debug.Log($"PathManager: Dodano kropkę z indeksem {dotRecolor.dotIndex}. Razem kropek: {dots.Count}");
            dotRecolor.ApplyInitialVisuals();
        }
    }

    // wydobywanie instancji kropki po indeksie
    public DotRecolor GetDot(int index)
    {
        if (index >= 0 && index < dots.Count)
        {
            // Pobierz komponent DotRecolor z GameObjectu na podstawie indeksu
            DotRecolor dotRecolor = dots[index].GetComponent<DotRecolor>();
            if (dotRecolor != null)
            {
                return dotRecolor;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }


    //sprawdzenie czy ukończono rysowanie szlaku
    public void CheckAndRemoveDots(float elapsedTime, float accuracy)
    {
        if (dots.Count > 0 && coloredDots == dots.Count)
        {
            Debug.Log("PathManager: Wszystkie kropki pokolorowane! Rozpoczynam proces usuwania.");
            coloringFinished = true;
            finishBannerController.ShowBanner(elapsedTime, accuracy);
        }
    }

    //usuwanie szlaku z opóźnieniem
    private IEnumerator RemoveDotsAfterDelay()
    {
        
        
        yield return new WaitForSeconds(delayInSeconds);

        // usuwamy wszystkie obiekty z listy ze sceny
        foreach (GameObject dot in dots)
        {
            Destroy(dot);
        }

        // czyscimy liste i zerujemy indeksy
        dots.Clear();
        nextDotIndex = 0;
        coloredDots = 0;
        coloringFinished = false;

        if (drawingPathInstance != null)
        {
            drawingPathInstance.ResetDrawingState();
            Debug.Log("PathManager: Zresetowano stan rysowania (lastDotPosition) po ukończeniu i usunięciu ścieżki.");
        }
        else
        {
            Debug.LogError("PathManager: Nie można zresetować stanu rysowania, ponieważ referencja do DrawingPath jest pusta!");
        }

        Debug.Log("Wszystkie kropki zostały usunięte.");
    }

    public void SaveNamedPath(string pathName, string filePath)
    {
        if (string.IsNullOrEmpty(pathName))
        {
            Debug.LogError("ścieżka nie może być pusta.");
            return;
        }
        if (dots == null || dots.Count == 0)
        {
            Debug.Log("Brak kropek do zapisania.");
            return;
        }

        // 1. Załaduj kolekcję lub ją stwórz
        SavedPathsCollection collection = LoadCollectionFromFile(filePath);

        // 2. przygotuj dane do zapisania
        NamedPathData currentPathData = new NamedPathData
        {
            name = pathName,
            dotPositions = dots.Select(dot => dot.transform.position).ToList()
        };

        // 3. Czy taka ścieżka już istnieje
        int existingIndex = collection.savedPaths.FindIndex(p => p.name == pathName);

        if (existingIndex != -1)
        {
            // nadpisz ścieżka
            collection.savedPaths[existingIndex] = currentPathData;
            Debug.Log($"Nadpisano ścieżka '{pathName}'.");
        }
        else
        {
            // utwórz ścieżka
            collection.savedPaths.Add(currentPathData);
            Debug.Log($"Dodano ścieżka '{pathName}'.");
        }

        // 4. Zapisz zaktualizowaną kolekcję
        if (SaveCollectionToFile(collection, filePath))
        {
            Debug.Log($"ścieżka '{pathName}' ({currentPathData.dotPositions.Count} kropek) zapisano poprawnie. Wszystkie zapisy: {collection.savedPaths.Count}");
            // 5. Wyczyść aktualną ciećkę po zapisaniu
            //ClearPath();
        }
    }

    // Funkcja ładowania ciećki
    public bool LoadNamedPath(string pathName, string filePath)
    {
        if (string.IsNullOrEmpty(pathName))
        {
            Debug.LogError("ścieżka nie może być pusta.");
            return false;
        }
        if (dotPrefab == null)
        {
            Debug.LogError("Dot Prefab nie jest przypisane w PathManager!");
            return false;
        }

        // 1. Załaduj kolekcję
        SavedPathsCollection collection = LoadCollectionFromFile(filePath);
        if (collection == null)
        {
            // Nie można załadować kolekcji
            return false;
        }

        // 2. Znajdź ścieżkę o podanej nazwie
        NamedPathData pathData = collection.savedPaths.FirstOrDefault(p => p.name == pathName);

        if (pathData == null)
        {
            Debug.Log($"ścieżka '{pathName}' nie znaleziona w {filePath}.");
            return false;
        }
        if (pathData.dotPositions == null || pathData.dotPositions.Count == 0)
        {
            Debug.Log($"ścieżka '{pathName}' znaleziona, ale bez zawartości (bez kropek).");
        }

        // 3. Wyczyść aktualną ścieżkę
        ClearPath();

        // 4. Odtwórz kropki
        try
        {
            foreach (Vector3 position in pathData.dotPositions)
            {
                GameObject newDot = Instantiate(dotPrefab, position, Quaternion.Euler(0f, 0f, 90f));

                // Ustawienia kropki
                if (newDot.GetComponent<SphereCollider>() == null) newDot.AddComponent<SphereCollider>();
                if (newDot.GetComponent<DotRecolor>() == null) Debug.Log($"Wczytana kropka na {position} brakuje DotRecolor.");

                // dodaj do managera
                AddDot(newDot);
            }

            Debug.Log($"Ścieżka '{pathName}' wczytana pomyślnie ({dots.Count} kropek).");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Błąd instancji kropek dla ścieżki '{pathName}': {e.Message}");
            return false;
        }
    }

    // metody pomocnicze do zapisu i odczytu kolekcji
    private SavedPathsCollection LoadCollectionFromFile(string filePath)
    {
        if (!File.Exists(filePath)) // zwrot pustej kolekcji
        {
            return new SavedPathsCollection();
        }

        try
        {
            string json = File.ReadAllText(filePath);
            SavedPathsCollection collection = JsonUtility.FromJson<SavedPathsCollection>(json);

            // Plik pusty lub niepoprawny JSON
            if (collection == null)
            {
                Debug.Log($"Plik {filePath} pusty lub niepoprawny JSON, zaczynamy z pustą kolekcją.");
                return new SavedPathsCollection();
            }

            if (collection.savedPaths == null)
            {
                collection.savedPaths = new List<NamedPathData>();
            }

            return collection;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Bład wczytania ścieżki {filePath}: {e.Message}");
            return new SavedPathsCollection();
        }
    }

    private bool SaveCollectionToFile(SavedPathsCollection collection, string filePath)
    {
        try
        {
            string json = JsonUtility.ToJson(collection, true);
            File.WriteAllText(filePath, json);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Bład zapisania kolekcji na {filePath}: {e.Message}");
            return false;
        }
    }

    public void ClearPath()
    {
        StopAllCoroutines();
        Debug.Log($"PathManager.ClearPath() wywołane. Liczba kropek PRZED czyszczeniem: {dots.Count}");

        foreach (GameObject dot in dots)
        {
            if (dot != null)
            {
                Destroy(dot);
            }
        }

        dots.Clear();
        nextDotIndex = 0;
        coloredDots = 0;
        coloringFinished = false;
        Debug.Log("PathManager: Wewnętrzny stan (listy, liczniki) został zresetowany.");

        // referencja powinna byc ustawiona w awake
        if (drawingPathInstance != null)
        {
            drawingPathInstance.ResetDrawingState();
        }
        else
        {
            // jesli DrawingPath nie istnieje w scenie
            Debug.LogError("PathManager.ClearPath(): Mimo próby, referencja do drawingPathInstance wciąż jest pusta", this);
        }
    }

    public void ForceClearAllDotsInScene()
    {
        Debug.Log("PathManager.ForceClearAllDotsInScene() wywołane - metoda siłowego czyszczenia.");
        StopAllCoroutines();

        DotRecolor[] allDotsInScene = FindObjectsOfType<DotRecolor>();
        Debug.Log($"PathManager: Znaleziono {allDotsInScene.Length} obiektów z komponentem DotRecolor do zniszczenia.");

        foreach (DotRecolor dotComponent in allDotsInScene)
        {
            if (dotComponent != null)
            {
                Destroy(dotComponent.gameObject);
            }
        }

        dots.Clear();
        nextDotIndex = 0;
        coloredDots = 0;
        coloringFinished = false;
        Debug.Log("PathManager: Wewnętrzny stan (lista, liczniki, flagi) zresetowany.");

        if (drawingPathInstance != null)
        {
            drawingPathInstance.ResetDrawingState();
        }
        else
        {
            Debug.LogError("PathManager.ForceClearAllDotsInScene(): Nie można zresetować stanu DrawingPath, ponieważ referencja jest pusta.");
        }

        Debug.Log("PathManager.ForceClearAllDotsInScene() zakończone pomyślnie.");
    }
}
