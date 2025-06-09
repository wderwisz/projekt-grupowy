using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Linq;

public class PathManager : MonoBehaviour
{
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

    public DrawingPath drawingPathInstance; // Referencja do DrawingPath

    private float delayInSeconds = 1.0f;

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
                Debug.Log("Obiekt nie ma komponentu DotRecolor.");
                return null;
            }
        }
        else
        {
            Debug.Log("Indeks spoza zakresu listy.");
            return null;
        }
    }


    //sprawdzenie czy ukończono rysowanie szlaku
    public void CheckAndRemoveDots()
    {
        Debug.Log($"PathManager.CheckAndRemoveDots -- pokolorowaneKropki: {coloredDots}, liczbaKropek: {dots.Count}");
        if (dots.Count > 0 && coloredDots == dots.Count)
        {
            Debug.Log("PathManager: Wszystkie kropki pokolorowane! Rozpoczynam proces usuwania.");
            coloringFinished = true;
            StartCoroutine(RemoveDotsAfterDelay());
        }
    }

    //usuwanie szlaku z opóźnieniem
    private IEnumerator RemoveDotsAfterDelay()
    {
        yield return new WaitForSeconds(delayInSeconds);

        // Usuwamy wszystkie obiekty z listy ze sceny
        foreach (GameObject dot in dots)
        {
            Destroy(dot);
        }

        // Teraz czyścimy listę i zerujemy indeksy
        dots.Clear();
        nextDotIndex = 0;
        coloredDots = 0;
        coloringFinished = false;
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
        int destroyedCount = 0;

        // Iteracja od tyłu jest bezpieczniejsza przy operacjach na kolekcji
        for (int i = dots.Count - 1; i >= 0; i--)
        {
            GameObject dotToDestroy = dots[i];
            if (dotToDestroy != null)
            {
                Destroy(dotToDestroy);
                // Usunięcie z listy indywidualnie nie jest konieczne, bo robimy dots.Clear() na końcu,
                // ale dodajemy log dla każdej niszczonej kropki.
                Debug.Log($"PathManager: Niszczenie kropki {dotToDestroy.name} (indeks {i})");
                destroyedCount++;
            }
            else
            {
                Debug.Log($"PathManager: Kropka o indeksie {i} w liście była już nullem przed próbą zniszczenia.");
            }
        }

        dots.Clear();
        Debug.Log($"PathManager.ClearPath(): Liczba kropek PO dots.Clear(): {dots.Count}. Łącznie przetworzonych GameObjectów do zniszczenia: {destroyedCount}");

        nextDotIndex = 0;
        coloredDots = 0;
        coloringFinished = false;
        Debug.Log("Wyczyszczono ścieżke.");

        if (drawingPathInstance != null)
        {
            drawingPathInstance.ResetDrawingState();
        }
        else
        {
            Debug.Log("PathManager.ClearPath(): Instancja DrawingPath nie została przypisana w PathManager.");
        }
    }

    public void ForceClearAllDotsInScene()
    {
        Debug.Log("PathManager.ForceClearAllDotsInScene() wywołane.");

        // 1: Znajdź wszystkie obiekty z komponentem DotRecolor
        DotRecolor[] allDotsInScene = FindObjectsOfType<DotRecolor>();
        int foundDotsCount = allDotsInScene.Length;
        Debug.Log($"PathManager: Znaleziono {foundDotsCount} obiektów z komponentem DotRecolor w scenie.");

        foreach (DotRecolor dotComponent in allDotsInScene)
        {
            if (dotComponent != null && dotComponent.gameObject != null)
            {
                Debug.Log($"PathManager: Niszczenie kropki ze sceny: {dotComponent.gameObject.name}");
                Destroy(dotComponent.gameObject);
            }
        }

        // 2: Wyczyść wewnętrzną listę i stan PathManager (dla spójności)
        int listCountBeforeClear = dots.Count;
        int destroyedFromListCount = 0;
        for (int i = dots.Count - 1; i >= 0; i--)
        {
            GameObject dotInList = dots[i];
            if (dotInList != null) 
            {
                Debug.Log($"PathManager: Upewnianie się, że kropka z wewnętrznej listy jest zniszczona: {dotInList.name}");
                Destroy(dotInList);
                destroyedFromListCount++;
            }
        }
        dots.Clear();
        Debug.Log($"PathManager: Wewnętrzna lista 'dots' wyczyszczona. Oryginalna liczba elementów: {listCountBeforeClear}. Przetworzono do zniszczenia z listy: {destroyedFromListCount}. Aktualna liczba w liście: {dots.Count}.");

        nextDotIndex = 0;
        coloredDots = 0;
        coloringFinished = false;
        Debug.Log("PathManager: Wewnętrzny stan (nextDotIndex, coloredDots, coloringFinished) zresetowany.");

        // 3: Zresetuj stan DrawingPath
        if (drawingPathInstance != null)
        {
            drawingPathInstance.ResetDrawingState();
            Debug.Log("PathManager: Wywołano ResetDrawingState() na instancji DrawingPath.");
        }
        else
        {
            Debug.Log("PathManager.ForceClearAllDotsInScene(): Instancja DrawingPath nie została przypisana w PathManager. Nie można zresetować jej stanu.");
        }

        Debug.Log("PathManager.ForceClearAllDotsInScene() zakończone.");
    }
}
