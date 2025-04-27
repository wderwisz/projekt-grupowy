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

    private float delayInSeconds = 3.0f;

    //dodawanie kropki do listy 
    public void AddDot(GameObject dot)
    {
        dots.Add(dot);
        Debug.Log("Liczba kropek w cieciu: " + dots.Count);

        // ustaw indeks kropki
        DotRecolor dotRecolor = dot.GetComponent<DotRecolor>();
        if (dotRecolor != null)
        {
            dotRecolor.dotIndex = dots.Count - 1;
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
        if (coloredDots == dots.Count)  
        {
            coloringFinished = true;
            StartCoroutine(RemoveDotsAfterDelay());  // Uruchamiamy coroutine, ktra poczeka 3 sekundy
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
            Debug.LogError("Ścieżka nie może być pusta.");
            return;
        }
        if (dots == null || dots.Count == 0)
        {
            Debug.LogWarning("Brak kropek do zapisania.");
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
            // nadpisz ścieżke
            collection.savedPaths[existingIndex] = currentPathData;
            Debug.Log($"Nadpisano ścieżke '{pathName}'.");
        }
        else
        {
            // utwórz ścieżke
            collection.savedPaths.Add(currentPathData);
            Debug.Log($"Dodano ścieżke '{pathName}'.");
        }

        // 4. Zapisz zaktualizowaną kolekcje
        if (SaveCollectionToFile(collection, filePath))
        {
             Debug.Log($"Ścieżka '{pathName}' ({currentPathData.dotPositions.Count} kropek) zapisano poprawnie. Wszystkie zapisy: {collection.savedPaths.Count}");
            // 5. Wyczyść aktualną ścieżkę po zapisaniu
            ClearPath();
        }
    }

    // Funkcja ładowania ścieżki
    public bool LoadNamedPath(string pathName, string filePath)
    {
        if (string.IsNullOrEmpty(pathName))
        {
            Debug.LogError("Ścieżka nie może być pusta.");
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
            Debug.LogWarning($"Ścieżka '{pathName}' nie znaleziona w {filePath}.");
            return false;
        }
        if (pathData.dotPositions == null || pathData.dotPositions.Count == 0)
        {
            Debug.LogWarning($"Ścieżka '{pathName}' znaleziona, ale bez zawartości (bez kropek).");
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
                 if (newDot.GetComponent<DotRecolor>() == null) Debug.LogWarning($"Wczytana kropka na {position} brakuje DotRecolor.");

                // dodaj do managera
                AddDot(newDot);
            }

            Debug.Log($"Path '{pathName}' loaded successfully ({dots.Count} dots).");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error instantiating dots for path '{pathName}': {e.Message}");
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
                Debug.LogWarning($"Plik {filePath} pusty lub niepoprawny JSON, zaczynamy z pustą kolekcją.");
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
            Debug.LogError($"Błąd zapisania kolekcji na {filePath}: {e.Message}");
            return false;
        }
    }

    public void ClearPath()
    {
        StopAllCoroutines();

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
        Debug.Log("Wyczyszczono ścieżke.");
    }
}
