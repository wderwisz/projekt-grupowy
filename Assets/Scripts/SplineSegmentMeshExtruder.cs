using System;
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
    [SerializeField][Range(0.01f, 2.0f)] private float height = 0.01f;  // wysokoœæ segmentu

    private float baseWidth = 0.01f;
    private float baseHeight = 0.01f;
    internal void scaleWidthAndHeight(float v)
    {
        width = baseWidth * v;
        height = baseHeight * v;

        Debug.Log(width.ToString() + height.ToString());
    }

    private Vector3 lastPerpendicularVector = Vector3.zero;
    private bool isSegmentation = true;
    private List<GameObject> segments;



    private List<Vector3> verticesList = new List<Vector3>();  // Przechowywanie wierzcho³ków
    private List<int> trianglesList = new List<int>();         // Przechowywanie trójk¹tów
    private Vector3[] lastVertices = new Vector3[4];           // Wierzcho³ki koñca poprzedniego segmentu
    public List<GameObject> getSegmentList()
    {
        return segments;
    }

    private void Awake()
    {
        segments = new List<GameObject>();
    }

  

    private GameObject CreateSegmentMesh(Vector3 startPoint, Vector3 endPoint, int index)
    {
        GameObject segmentMesh = new GameObject($"SplineSegmentMesh_{index}");
        segmentMesh.transform.parent = this.transform;

        Segment3D segmentComponent = segmentMesh.AddComponent<Segment3D>();

        MeshFilter meshFilter = segmentMesh.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = segmentMesh.AddComponent<MeshRenderer>();

        // Przypisz materia³ rotacyjnie
        meshRenderer.material = segmentMaterials[index % segmentMaterials.Length];

        // Generuj extrudowany mesh
        Mesh mesh = GenerateExtrudedMesh(startPoint, endPoint, isSegmentation);
        meshFilter.mesh = mesh;

        // Dodaj collider
        MeshCollider collider = segmentMesh.AddComponent<MeshCollider>();
        collider.convex = true;
        collider.isTrigger = true;

        // Dodaj komponent do zmiany koloru
        RecolorPath3D recolorPath3D = segmentMesh.AddComponent<RecolorPath3D>();
        recolorPath3D.newMaterial = recolorMaterial;

        // Dodaj segment do listy
        segments.Add(segmentMesh);

        return segmentMesh;
    }

    // Funkcja do ekstrudowania pojedynczego segmentu
    public void ExtrudeSingleSegment(Spline spline, int knotIndex)
    {
        if (knotIndex <= 0) return;

        Vector3 startPoint = (Vector3)spline[knotIndex - 1].Position;
        Vector3 endPoint = (Vector3)spline[knotIndex].Position;

        GameObject segmentMesh = CreateSegmentMesh(startPoint, endPoint, knotIndex);

        // Ustawianie zale¿noœci miêdzy segmentami
        RecolorPath3D recolorPath3D = segmentMesh.GetComponent<RecolorPath3D>();
        recolorPath3D.setCurrentSegment(segments[knotIndex - 1]);
        if (knotIndex > 1) recolorPath3D.setPreviousSegment(segments[knotIndex - 2]);
    }

    // Funkcja do ekstrudowania ca³ego spline'a
    public void ExtrudeAndApplyMaterials(Spline spline)
    {
        for (int i = 0; i < spline.Count - 1; i++)
        {
            Vector3 startPoint = (Vector3)spline[i].Position;
            Vector3 endPoint = (Vector3)spline[i + 1].Position;

            GameObject segmentMesh = CreateSegmentMesh(startPoint, endPoint, i);

            // Ustawianie zale¿noœci miêdzy segmentami
            RecolorPath3D recolorPath3D = segmentMesh.GetComponent<RecolorPath3D>();
            recolorPath3D.setCurrentSegment(segments[i]);
            if (i > 0) recolorPath3D.setPreviousSegment(segments[i - 1]);
        }
        lastPerpendicularVector = Vector3.zero;
        
    }


    private Mesh GenerateExtrudedMeshContinuous(Vector3 start, Vector3 end)
    {
        Mesh mesh = new Mesh();
        Vector3 directionVector = end - start;

        Vector3 currentPerpendicularVector = GetPerpendicularInPlane(start, end);
        if (lastPerpendicularVector == Vector3.zero)
        {
           lastPerpendicularVector = GetPerpendicularInPlane(start, end);
        }

        // Wyznaczanie przesuniêæ
        Vector3 y1 = lastPerpendicularVector * height;
        Vector3 y2 = currentPerpendicularVector * height;

        Vector3 z1 = GetPerpendicular(lastPerpendicularVector, directionVector) * width;
        Vector3 z2 = GetPerpendicular(currentPerpendicularVector, directionVector) * width;
        lastPerpendicularVector = currentPerpendicularVector;
     
        // Definiowanie wierzcho³ków
        Vector3[] newVertices = new Vector3[4]
        {
        end - z2 - y2, // Dolny lewy (nowy)
        end + z2 - y2, // Dolny prawy (nowy)
        end - z2 + y2, // Górny lewy (nowy)
        end + z2 + y2  // Górny prawy (nowy)
        };

        // Jeœli to pierwszy segment, dodaj równie¿ wierzcho³ki pocz¹tkowe
        if (verticesList.Count == 0)
        {
            verticesList.Add(start - z1 - y1);
            verticesList.Add(start + z1 - y1);
            verticesList.Add(start - z1 + y1);
            verticesList.Add(start + z1 + y1);
        }

        // Dodaj nowe wierzcho³ki do listy
        verticesList.AddRange(newVertices);

        // Dodawanie trójk¹tów dla œcian bocznych (ale nie ³¹cz¹cych segmentów)
        int startIndex = verticesList.Count - 8;  // Indeks pierwszego wierzcho³ka poprzedniego segmentu

        if (startIndex >= 0)
        {
            // Boczne œciany (bez wewnêtrznych)
            trianglesList.AddRange(new int[] {
            startIndex, startIndex + 1, startIndex + 5,
            startIndex, startIndex + 5, startIndex + 4,

            startIndex + 1, startIndex + 3, startIndex + 7,
            startIndex + 1, startIndex + 7, startIndex + 5,

            startIndex + 2, startIndex + 6, startIndex + 7,
            startIndex + 2, startIndex + 7, startIndex + 3,

            startIndex, startIndex + 4, startIndex + 6,
            startIndex, startIndex + 6, startIndex + 2
        });
        }

        // Zapamiêtaj wierzcho³ki koñcowe dla nastêpnego segmentu
        lastVertices = newVertices;

        // Tworzenie i zwracanie mesha
        mesh.vertices = verticesList.ToArray();
        mesh.triangles = trianglesList.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        return mesh;
    }



    private Mesh GenerateExtrudedMeshWithSegmentation(Vector3 start, Vector3 end)
    {
        Mesh mesh = new Mesh();
        Vector3 directionVector = end - start;
        Vector3 currentPerpendicularVector = GetPerpendicularInPlane(start, end);

        if(lastPerpendicularVector == Vector3.zero)
        {
           lastPerpendicularVector = GetPerpendicularInPlane(start, end);
        }
        // Wyznaczanie wspó³rzêdnych przesuniêæ wierzcho³ków
        Vector3 y1 = lastPerpendicularVector * height;
        Vector3 y2 = currentPerpendicularVector * height;
        Vector3 z1 = GetPerpendicular(lastPerpendicularVector, directionVector) * width;
        Vector3 z2 = GetPerpendicular(currentPerpendicularVector, directionVector) * width;
        lastPerpendicularVector = currentPerpendicularVector;
        //if (Vector3.Angle(lastPerpendicularVector, currentPerpendicularVector) > 170)
          //lastPerpendicularVector = Vector3.zero; // Resetuj, jeœli zmiana jest zbyt du¿a
       

        Vector3[] vertices = new Vector3[numberOfVertices]
      {
                start - z1 - y1,  //0
                start + z1 - y1,
                end - z2 - y2,
                end + z2 - y2,
                start - z1 + y1,
                start + z1+ y1,
                end - z2 + y2,
                end + z2 + y2  //7
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
        mesh.RecalculateTangents();
        return mesh;
    }

    Vector3 GetPerpendicular(Vector3 a, Vector3 b)
    {
        return Vector3.Cross(a, b).normalized;
    }

    private Mesh GenerateExtrudedMesh(Vector3 start, Vector3 end, bool isSegmetation)
    {
        if (isSegmetation)
        {
            return GenerateExtrudedMeshWithSegmentation(start, end);
        }
        else
        {
            return GenerateExtrudedMeshContinuous(start, end);
        }
    }

    public Vector3 GetPerpendicularInPlane(Vector3 p2, Vector3 p3)
    {
        Vector3 reference = p3 - p2;

        // Jeœli wektor referencyjny jest zerowy, zwracamy domyœln¹ wartoœæ
        if (reference == Vector3.zero)
        {
            return Vector3.up;
        }
        //punkt pomocniczny na lezy na poprzednim wektorze prostopadlym do punktu startowego
        Vector3 p1 = p2 - lastPerpendicularVector; 

        Vector3 v1 = p2 - p1;
        Vector3 v2 = p3 - p1;
       
        // Sprawdzenie, czy v1 i v2 nie s¹ wspó³liniowe
        Vector3 normal = Vector3.Cross(v1, v2).normalized;
        if (normal == Vector3.zero)
        {
            // Tworzymy punkt pomocniczy w pobli¿u œrodka odcinka, ale nie na lini  i p2-p3
            Vector3 midpoint = (p2 + p3) / 2;
            // Jeœli normalna nie istnieje, próbujemy innego punktu p1
            p1 = new Vector3(midpoint.x, midpoint.y+1.0f, midpoint.z ); // Dodanie ma³ej perturbacji do y
            normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;

            if (normal == Vector3.zero)
            {
                return Vector3.up; // Ostateczna wartoœæ awaryjna
            }
        }

        // Projekcja reference na p³aszczyznê
        Vector3 referenceProjected = reference - Vector3.Dot(reference, normal) * normal;

        // Jeœli projekcja jest zerowa, próbujemy z innym punktem p1
        if (referenceProjected == Vector3.zero)
        {
            return Vector3.Cross(normal, Vector3.right).normalized;
        }

        return Vector3.Cross(referenceProjected, normal).normalized;
    }

    public void ClearTrail() //usuwanie traila 
    {
        Debug.Log(segments.Count);
        foreach (GameObject segment in segments)
        {
            Debug.Log(segment);
            Destroy(segment);
        }

        segments.Clear();
    }
}


