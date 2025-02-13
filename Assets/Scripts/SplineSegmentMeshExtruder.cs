using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class SplineSegmentMeshExtruder : MonoBehaviour
{
    public Material[] segmentMaterials; // Tablica materia³ów do ró¿nych segmentów spline'a
    public Material recolorMaterial;

    // Liczba wierzcho³ków bry³y ekstrudowanej na pojedynczym segmencie i liczba jej trójk¹tów
    private const int numberOfVertices = 8;
    private const int numberOfTriangles = 12;

    [SerializeField][Range(0.01f, 2.0f)] private float width = 0.01f;  // Szerokoœæ segmentu
    [SerializeField][Range(0.01f, 2.0f)] private float hight = 0.01f;  // wysokoœæ segmentu

    private Vector3 lastPerpendicularInY = Vector3.zero; 

    private List<GameObject> segments;

    public List<GameObject> getSegmentList()
    {
        return segments;
    }

    private void Awake()
    {
        segments = new List<GameObject>();
    }

    // Funkcja do ekstrudowania pojedynczego segmentu. Wywo³ywana po dodaniu kolejnego knota do spline'a
    public void ExtrudeSingleSegment(Spline spline, int knotIndex)
    {
        GameObject segmentMesh = new GameObject($"SplineSegmentMesh_{knotIndex}");
        Segment3D segmentComponent = segmentMesh.AddComponent<Segment3D>();
        segmentMesh.transform.parent = this.transform;

        MeshFilter meshFilter = segmentMesh.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = segmentMesh.AddComponent<MeshRenderer>();

        // Przypisz materia³ (rotacyjnie dla ró¿nych segmentów)
        meshRenderer.material = segmentMaterials[knotIndex % segmentMaterials.Length];

        // Generuj extrudowany segment jako osobny mesh
        Vector3 startPoint = (Vector3)spline[knotIndex - 1].Position;
        Vector3 endPoint = (Vector3)spline[knotIndex].Position;

        // Tworzenie prostego mesha wzd³u¿ segmentu spline'a
        Mesh mesh = GenerateExtrudedMesh(startPoint, endPoint);
        meshFilter.mesh = mesh;

        MeshCollider collider = segmentMesh.AddComponent<MeshCollider>();
        collider.convex = true;
        collider.isTrigger = true;

        //segmentMesh.AddComponent<DestroyingSplineSegment>();
        RecolorPath3D recolorPath3D = segmentMesh.AddComponent<RecolorPath3D>();
        recolorPath3D.newMaterial = recolorMaterial;

        segments.Add(segmentMesh);

        recolorPath3D.setCurrentSegment(segments[knotIndex - 1]);
        if(knotIndex > 1) recolorPath3D.setPreviousSegment(segments[knotIndex - 2]);
    }

    // funkcja do ekstrudowania ca³ego spline'a po zakoñczeniu rysowania 
    public void ExtrudeAndApplyMaterials(Spline spline)
    {
        for (int i = 0; i < spline.Count - 1; i++)
        {
            GameObject segmentMesh = new GameObject($"SplineSegmentMesh_{i}");
            Segment3D segmentComponent = segmentMesh.AddComponent<Segment3D>();
            segmentMesh.transform.parent = this.transform;

            MeshFilter meshFilter = segmentMesh.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = segmentMesh.AddComponent<MeshRenderer>();

            // Przypisz materia³ (rotacyjnie dla ró¿nych segmentów)
            meshRenderer.material = segmentMaterials[i % segmentMaterials.Length];

            // Generowanie ekstrudowanego segmentu jako osobny mesh
            Vector3 startPoint = (Vector3)spline[i].Position;
            Vector3 endPoint = (Vector3)spline[i + 1].Position;

            // Tworzenie prostego mesha wzd³u¿ segmentu spline'a
            Mesh mesh = GenerateExtrudedMesh(startPoint, endPoint);
            meshFilter.mesh = mesh;

            // Dodawanie collidera do mesha i ustawianie jego atrybutów
            MeshCollider collider = segmentMesh.AddComponent<MeshCollider>();
            collider.convex = true;
            collider.isTrigger = true;

            //segmentMesh.AddComponent<DestroyingSplineSegment>();
            RecolorPath3D recolorPath3D = segmentMesh.AddComponent<RecolorPath3D>();
            recolorPath3D.newMaterial = recolorMaterial;

            segments.Add(segmentMesh);

            recolorPath3D.setCurrentSegment(segments[i]);
            if (i > 0) recolorPath3D.setPreviousSegment(segments[i - 1]);
        }
    }

    // Funkcja tworz¹ca mesh w ramach pojedynczego segmentu
    private Mesh GenerateExtrudedMesh(Vector3 start, Vector3 end)
    {
        Mesh mesh = new Mesh();
        Vector3 directionVector = end - start;

        Vector3 currentPerpendicularInY = GetPerpendicularXY(directionVector);
        if (lastPerpendicularInY == Vector3.zero)
        {
            lastPerpendicularInY = GetPerpendicularXY(directionVector);
        }

        // Wyznaczanie wspó³rzêdnych przesuniêæ wierzcho³ków
        Vector3 y1 = lastPerpendicularInY * hight;
        Vector3 y2 = currentPerpendicularInY * hight;
        Vector3 z = new Vector3(0, 0, 1) * width;

        lastPerpendicularInY = currentPerpendicularInY;
        Vector3[] vertices = new Vector3[numberOfVertices]
      {
            start - z - y1,  //0
            start + z - y1,
            end - z - y2,
            end + z - y2,
            start - z + y1,
            start + z+ y1,
            end - z + y2,
            end + z + y2  //7
      };

        // Wyznaczanie trójk¹tów mesha pomiêdzy wierzcho³kami
        int[] triangles = new int[numberOfTriangles * 3] { 
            0, 1, 2,    // sciany
            2, 1, 3,
            0, 2, 6,
            6, 4, 0,
            1, 7, 3,
            7, 1, 5,
            5, 4, 6, 
            6, 7, 5,
            0, 4, 5,    // podstawy
            0, 5, 1,
            2, 7, 6,
            2, 3, 7
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
    Vector3 GetPerpendicularXY(Vector3 v)
    {
        return new Vector3(-v.y, v.x, 0).normalized; // Normalizujemy wynik, aby by³ jednostkowy
    }
  
}
