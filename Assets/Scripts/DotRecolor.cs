using UnityEngine;

public class DotRecolor : MonoBehaviour
{
    public Material coloredMaterial;

    public Material originalMaterial;

    private MeshRenderer meshRenderer;

    // indeks kropek w sciezce
    [HideInInspector] public int dotIndex = 0;
    private bool isColored = false;

    public bool IsColored => isColored;

    private void Awake()
    {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();

        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
    }

        // metoda do zmiany koloru kropki
        public void Recolor()
           {
        PathManager pathManager = FindObjectOfType<PathManager>();

        //kolorujemy po kolei od pocz¹tku z uwzglêdnieniem s¹siada który móg³by nas zas³aniaæ
        if (pathManager.nextDotIndex + 1 >= dotIndex && !isColored)
        
        {
           
            // Zmiana materialu nie dziala, narazie ustawiamy kolor
            meshRenderer.material = coloredMaterial;
            meshRenderer.material.color = Color.red;

            isColored = true;
            Debug.Log("DotRecolor: Pokolorowano kropkê o indeksie: " + dotIndex);
            if (dotIndex > pathManager.nextDotIndex)
                pathManager.nextDotIndex = dotIndex;
            pathManager.coloredDots++;
        }
        else
        {
            Debug.Log($" Warunki do zmiany koloru nie spe³nione dla kropki {dotIndex}");
        }
    }
}
