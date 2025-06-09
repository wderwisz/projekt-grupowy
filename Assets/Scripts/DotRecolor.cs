using UnityEngine;

public class DotRecolor : MonoBehaviour
{
    public Material coloredMaterial;

    public Material originalMaterial;

    public Material initialDarkerMaterial;
    public Color initialDarkColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private MeshRenderer meshRenderer;

    // indeks kropek w sciezce
    [HideInInspector] public int dotIndex = 0;
    private bool isColored = false;

    public PathManager pathManagerInstance;

    public bool IsColored => isColored;

    private void Awake()
    {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();

        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
    }


    public void ApplyInitialVisuals()
    {
        if (meshRenderer == null) Awake();

        if (dotIndex < 5) // Pierwsze 5 kropek
        {
            if (initialDarkerMaterial != null)
            {
                meshRenderer.material = initialDarkerMaterial;
            }
            else if (originalMaterial != null)
            {
                meshRenderer.material = new Material(originalMaterial); // Tworzymy kopię, by zmienić kolor
                meshRenderer.material.color = initialDarkColor;
            }
            else
            {
                // Jeśli żaden materiał nie jest dostępny, użyj koloru na domyślnym materiale renderera
                meshRenderer.material.color = initialDarkColor;
            }
        }
        else // Pozostałe kropki
        {
            if (originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
            }
            else
            {
                // Domyślny wygląd dla pozostałych kropek, jeśli originalMaterial nie jest ustawiony
                meshRenderer.material.color = Color.white;
            }
        }
        isColored = false;
    }

    // metoda do zmiany koloru kropki
    public void Recolor()
    {
        if (pathManagerInstance == null)
        {
            Debug.LogError("Błąd: pathManagerInstance nie jest ustawiony w DotRecolor!");
            return;
        }

        //kolorujemy po kolei od początku z uwzględnieniem sąsiada który mógłby nas zasłaniać
        if (pathManagerInstance.nextDotIndex + 1 >= dotIndex && !isColored)
        
        {
           
            // Zmiana materialu nie dziala, narazie ustawiamy kolor
            meshRenderer.material = coloredMaterial;
            meshRenderer.material.color = Color.red;
            
            isColored = true;
            Debug.Log($"DotRecolor ({gameObject.name}, indeks {dotIndex}): Pomyślnie ZMIENIONO KOLOR. pathManager.coloredDots = {pathManagerInstance.coloredDots}, pathManager.dots.Count = {pathManagerInstance.dots.Count}");
            if (dotIndex > pathManagerInstance.nextDotIndex)
                pathManagerInstance.nextDotIndex = dotIndex;
            pathManagerInstance.coloredDots++;

            //pathManager.CheckAndRemoveDots();
        }
        else
        {
            Debug.Log($"DotRecolor ({gameObject.name}, indeks {dotIndex}): Warunki NIE SPEŁNIONE do zmiany koloru. isColored: {isColored}, nextDotIndex: {pathManagerInstance.nextDotIndex}, Wymagany nextDotIndex lub nextDotIndex+1: {dotIndex}");
        }
    }

}
