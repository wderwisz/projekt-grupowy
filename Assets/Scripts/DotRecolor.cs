using UnityEngine;

public class DotRecolor : MonoBehaviour
{
    public Material coloredMaterial;

    public Material originalMaterial;

    private MeshRenderer meshRenderer;

    // indeks kropek w sciezce
    [HideInInspector] public int dotIndex = 0;
    private bool isColored = false;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // metoda do zmiany koloru kropki
    public void Recolor()
    {
        PathManager pathManager = FindObjectOfType<PathManager>();

        if (pathManager.nextDotIndex == dotIndex && !isColored)
        {
            meshRenderer.material = coloredMaterial;
            isColored = true;
            Debug.Log("DotRecolor: Pokolorowano kropkê o indeksie: " + dotIndex);
            pathManager.nextDotIndex++;  // przechodzimy do nastêpnej kropki
        }
        else
        {
            Debug.Log($" Warunki do zmiany koloru nie spe³nione dla kropki {dotIndex}");
        }
    }
}
