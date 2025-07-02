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
                meshRenderer.material = new Material(originalMaterial);
                meshRenderer.material.color = initialDarkColor;
            }
            else
            {
                // uzyj domyslnego
                meshRenderer.material.color = initialDarkColor;
            }
        }
        else // pozostale kropki
        {
            if (originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
            }
            else
            {
                // bialy w razie braku koloru
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
            meshRenderer.material = coloredMaterial;
            meshRenderer.material.color = Color.red;
            
            isColored = true;
            if (dotIndex > pathManagerInstance.nextDotIndex)
                pathManagerInstance.nextDotIndex = dotIndex;
            pathManagerInstance.coloredDots++;

        }
        else
        {
   
        }
    }

}
