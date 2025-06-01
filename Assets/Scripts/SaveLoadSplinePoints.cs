using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using UnityEngine;
using System.Globalization;
using UnityEngine.Splines;


public class SaveLoadSplinePoints : MonoBehaviour
{

    //string filePathx = Path.Combine(Application.persistentDataPath, "points.json");

    public static void SaveVector3List(string filePath, List<Spline> splinesList)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var spline in splinesList)
                {
                    for (int i = 0; i < spline.Count; i++)
                    {
                        Vector3 point = (Vector3)spline[i].Position;
                        writer.WriteLine($"{point.x.ToString("F6", CultureInfo.InvariantCulture)} " +
                                         $"{point.y.ToString("F6", CultureInfo.InvariantCulture)} " +
                                         $"{point.z.ToString("F6", CultureInfo.InvariantCulture)}");
                    }
                    // Separator oznaczający koniec jednej listy spline
                    writer.WriteLine("---");
                }
            }
            Debug.Log($"Dane zapisane do: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Błąd zapisu: {ex.Message}");
        }
    }



    public static List<List<Vector3>> LoadVector3List(string filePath)
    {
        List<List<Vector3>> splinesList = new List<List<Vector3>>();

        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning("Plik nie istnieje, zwracam pustą listę.");
                return splinesList;
            }

            using (StreamReader reader = new StreamReader(filePath))
            {
                List<Vector3> currentSpline = new List<Vector3>();
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line == "---")
                    {
                        // Jeśli napotkamy separator, dodajemy aktualny spline do listy
                        if (currentSpline.Count > 0)
                        {
                            splinesList.Add(currentSpline);
                            currentSpline = new List<Vector3>(); // Tworzymy nową listę na nowy spline
                        }
                    }
                    else
                    {
                        string[] parts = line.Split(' ');
                        if (parts.Length == 3 &&
                            float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                            float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                        {
                            currentSpline.Add(new Vector3(x, y, z));
                        }
                    }
                }

                // Dodanie ostatniego splinu (jeśli istnieje)
                if (currentSpline.Count > 0)
                {
                    splinesList.Add(currentSpline);
                }
            }
            Debug.Log($"Dane odczytane z pliku: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Błąd odczytu: {ex.Message}");
        }

        return splinesList;
    }



}
