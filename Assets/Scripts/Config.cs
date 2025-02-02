using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class Config : ScriptableObject
{
    // Tryb rysowania w prototypie (do zmiany po implementacji przełączania między trybami fizjo i pacjenta)
    // true -> ekstrudowanie szlaku następuje w trakcie kreślenia krzywej - brak możliwości kolorowania istniejącego szlaku
    // false -> ekstrudowanie następuje dopiero po skończeniu rysowania - kolorowanie po najechaniu na istniejący szlak
    // true gdy testujemy tworzenie szlaku; false gdy kolorowanie 
    [SerializeField] private bool liveDrawingMode = true;

    public bool getDrawingMode()
    {
        return liveDrawingMode;
    }
}
