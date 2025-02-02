using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class Config : ScriptableObject
{
    // Tryb rysowania w prototypie (do zmiany po implementacji prze³¹czania miêdzy trybami fizjo i pacjenta)
    // true -> ekstrudowanie szlaku nastêpuje w trakcie kreœlenia krzywej - brak mo¿liwoœci kolorowania istniej¹cego szlaku
    // false -> ekstrudowanie nastêpuje dopiero po skoñczeniu rysowania - kolorowanie po najechaniu na istniej¹cy szlak
    // true gdy testujemy tworzenie szlaku; false gdy kolorowanie 
    [SerializeField] private bool liveDrawingMode = true;

    public bool getDrawingMode()
    {
        return liveDrawingMode;
    }
}
