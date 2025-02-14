using UnityEngine;

public class DotRecolor : MonoBehaviour
{
    [Tooltip("Materia�, kt�ry zostanie ustawiony po odwzorowaniu kropki.")]
    public Material coloredMaterial;

    private Material originalMaterial;
    private MeshRenderer meshRenderer;

    // indeks kropek w �cie�ce
    [HideInInspector]
    public int dotIndex = 0;

    private bool isColored = false;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originalMaterial = meshRenderer.material;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Controller")) return;

        PathManager pathManager = FindObjectOfType<PathManager>();
        if (pathManager == null) return;

        // Sprawdz czy kolejna kropka to ta do kolorwania
        if (pathManager.nextDotIndex == dotIndex && !isColored)
        {
            meshRenderer.material = coloredMaterial;
            isColored = true;
            Debug.Log("Pokolorowano kropk� o indeksie: " + dotIndex);
            pathManager.nextDotIndex++;  // przechodzimy do nast�pnej kropki
        }
    }

    public bool IsColored()
    {
        return isColored;
    }
}
