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

    private float delayInSeconds = 1.0f;

    //dodawanie kropki do listy 
    public void AddDot(GameObject dot)
    {
        dots.Add(dot);
        Debug.Log("Liczba kropek w œcie¿ce: " + dots.Count);

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


    //sprawdzenie czy ukoñczono rysowanie szlaku
    public void CheckAndRemoveDots()
    {
        if (coloredDots == dots.Count)
        {
            coloringFinished = true;
            StartCoroutine(RemoveDotsAfterDelay());  // Uruchamiamy coroutine, ktra poczeka 3 sekundy
        }
    }


    public void removeDots() //TODO sprawdziæ czemy siê doty nie usuwaj¹
    {

        foreach (GameObject dot in dots)
        {
            Destroy(dot);
        }
        // Teraz czyœcimy listê i zerujemy indeksy
        dots.Clear();
        nextDotIndex = 0;
        coloredDots = 0;
        Debug.Log("Wszystkie kropki zosta³y usuniête.");

    }

    //usuwanie szlaku z opóŸnieniem
    private IEnumerator RemoveDotsAfterDelay()
    {
        yield return new WaitForSeconds(delayInSeconds);

        // Usuwamy wszystkie obiekty z listy ze sceny
        foreach (GameObject dot in dots)
        {
            Destroy(dot);
        }

        // Teraz czyœcimy listê i zerujemy indeksy
        dots.Clear();
        nextDotIndex = 0;
        coloredDots = 0;
        coloringFinished = false;
        Debug.Log("Wszystkie kropki zosta³y usuniête.");
    }

    public void SaveNamedPath(string pathName, string filePath)
    {
        if (string.IsNullOrEmpty(pathName))
        {
            Debug.LogError("Œcie¿ka nie mo¿e byæ pusta.");
            return;
        }
        if (dots == null || dots.Count == 0)
        {
            Debug.LogWarning("Brak kropek do zapisania.");
            return;
        }

        // 1. Za³aduj kolekcjê lub j¹ stwórz
        SavedPathsCollection collection = LoadCollectionFromFile(filePath);

        // 2. przygotuj dane do zapisania
        NamedPathData currentPathData = new NamedPathData
        {
            name = pathName,
            dotPositions = dots.Select(dot => dot.transform.position).ToList()
        };

        // 3. Czy taka œcie¿ka ju¿ istnieje
        int existingIndex = collection.savedPaths.FindIndex(p => p.name == pathName);

        if (existingIndex != -1)
        {
            // nadpisz œcie¿ke
            collection.savedPaths[existingIndex] = currentPathData;
            Debug.Log($"Nadpisano œcie¿ke '{pathName}'.");
        }
        else
        {
            // utwórz œcie¿ke
            collection.savedPaths.Add(currentPathData);
            Debug.Log($"Dodano œcie¿ke '{pathName}'.");
        }

        // 4. Zapisz zaktualizowan¹ kolekcje
        if (SaveCollectionToFile(collection, filePath))
        {
            Debug.Log($"Œcie¿ka '{pathName}' ({currentPathData.dotPositions.Count} kropek) zapisano poprawnie. Wszystkie zapisy: {collection.savedPaths.Count}");
            // 5. Wyczyœæ aktualn¹ œcie¿kê po zapisaniu
            ClearPath();
        }
    }

    // Funkcja ³adowania œcie¿ki
    public bool LoadNamedPath(string pathName, string filePath)
    {
        if (string.IsNullOrEmpty(pathName))
        {
            Debug.LogError("Œcie¿ka nie mo¿e byæ pusta.");
            return false;
        }
        if (dotPrefab == null)
        {
            Debug.LogError("Dot Prefab nie jest przypisane w PathManager!");
            return false;
        }

        // 1. Za³aduj kolekcjê
        SavedPathsCollection collection = LoadCollectionFromFile(filePath);
        if (collection == null)
        {
            // Nie mo¿na za³adowaæ kolekcji
            return false;
        }

        // 2. ZnajdŸ œcie¿kê o podanej nazwie
        NamedPathData pathData = collection.savedPaths.FirstOrDefault(p => p.name == pathName);

        if (pathData == null)
        {
            Debug.LogWarning($"Œcie¿ka '{pathName}' nie znaleziona w {filePath}.");
            return false;
        }
        if (pathData.dotPositions == null || pathData.dotPositions.Count == 0)
        {
            Debug.LogWarning($"Œcie¿ka '{pathName}' znaleziona, ale bez zawartoœci (bez kropek).");
        }

        // 3. Wyczyœæ aktualn¹ œcie¿kê
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
                Debug.LogWarning($"Plik {filePath} pusty lub niepoprawny JSON, zaczynamy z pust¹ kolekcj¹.");
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
            Debug.LogError($"B³ad wczytania œcie¿ki {filePath}: {e.Message}");
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
            Debug.LogError($"B³¹d zapisania kolekcji na {filePath}: {e.Message}");
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
        Debug.Log("Wyczyszczono œcie¿ke.");
    }
}
