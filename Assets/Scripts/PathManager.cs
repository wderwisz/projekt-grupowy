using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    public List<GameObject> dots = new List<GameObject>();

    public int nextDotIndex = 0;

    public int coloredDots = 0;

    public bool coloringFinished = false;

    private float delayInSeconds = 3.0f;

    //dodawanie kropki do listy 
    public void AddDot(GameObject dot)
    {
        dots.Add(dot);
        Debug.Log("Liczba kropek w �cie�ce: " + dots.Count);

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


    //sprawdzenie czy uko�czono rysowanie szlaku
    public void CheckAndRemoveDots()
    {
        if (coloredDots == dots.Count - 1)  // Sprawdzamy, czy to ostatnia kropka
        {
            coloringFinished = true;
            StartCoroutine(RemoveDotsAfterDelay());  // Uruchamiamy coroutine, kt�ra poczeka 3 sekundy
        }
    }

    //usuwanie szlaku z op�nieniem
    private IEnumerator RemoveDotsAfterDelay()
    {
        yield return new WaitForSeconds(delayInSeconds); 

        // Usuwamy wszystkie obiekty z listy ze sceny
        foreach (GameObject dot in dots)
        {
            Destroy(dot);  
        }

        // Teraz czy�cimy list� i zerujemy indeksy
        dots.Clear(); 
        nextDotIndex = 0;
        coloredDots = 0;
        coloringFinished = false;
        Debug.Log("Wszystkie kropki zosta�y usuni�te.");
    }

}
