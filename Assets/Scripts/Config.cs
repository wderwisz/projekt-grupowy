using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class Config : ScriptableObject
{
    // Tryb rysowania w prototypie (do zmiany po implementacji prze��czania mi�dzy trybami fizjo i pacjenta)
    // true -> ekstrudowanie szlaku nast�puje w trakcie kre�lenia krzywej - brak mo�liwo�ci kolorowania istniej�cego szlaku
    // false -> ekstrudowanie nast�puje dopiero po sko�czeniu rysowania - kolorowanie po najechaniu na istniej�cy szlak
    // true gdy testujemy tworzenie szlaku; false gdy kolorowanie 
    [SerializeField] private bool liveDrawingMode = true;

    //w��czenie opcji zmazywania w trybie fizjoterapeuty (narazie w 2d)
    //pewnie lepiej by�oby ustawi� na inny przycisk na kontolerze
    // usuwanie linii jedynie od jej ko�ca
    [SerializeField] private bool erasingMode = false;

    [Header("Ścieżki do zapisu/odczytu")]
    [SerializeField] public string defaultSaveName = "DefaultPath";
    [SerializeField] public string defaultLoadName = "DefaultPath";

    public bool getDrawingMode()
    {
        return liveDrawingMode;
    }

    public bool getErasingMode() 
    {
        return erasingMode;
    }
}
