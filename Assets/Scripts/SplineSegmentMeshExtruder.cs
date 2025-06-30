using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;
using System.Linq;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Experimental.Rendering;
using System.IO;

public class SplineSegmentMeshExtruder : MonoBehaviour
{
    public Material[] segmentMaterials; // Tablica materialow do roznych segmentow spline'a
    public Material recolorMaterial;
    public Material freeDrawingMaterial;

    // Liczba wierzcholkow bryly ekstrudowanej na pojedynczym segmencie i liczba jej trojkatow
    private const int numberOfVertices = 8;
    private const int numberOfTriangles = 12;

    private float width = 0.01f;  // Szerokosc segmentu
    private float hight = 0.01f;  // wysokosc segmentu
    [SerializeField] [Range(0.01f, 10.0f)] private float vectorScale = 1.0f;

    private Vector3 lastPerpendicularVector = Vector3.zero;
    [SerializeField] private bool isSegmentation = true;
    private List<GameObject> segments;
    private GameObject lastSegment = null;      //ostatni segment ktory nalezy usunac

    bool isFirstSegment = true;
    bool isLastSegment = false;


    private List<Vector3> verticesList = new List<Vector3>();  // Przechowywanie wierzcholkow
    private List<int> trianglesList = new List<int>();         // Przechowywanie trojkatow

    private Vector3[] lastVertices = new Vector3[4];           // Wierzcholki konca poprzedniego segmentu


    public List<GameObject> getSegmentList()
    {
        return segments;
    }

    public float getVectorScale()
    {
        return vectorScale;
    }

    private void Awake()
    {
        segments = new List<GameObject>();

    }

    public void setVectorScale(float v) {
        vectorScale = v;
    }

    public void ConfirmSegment()
    {
        lastSegment = null;
    }

    private GameObject CreateSegmentMesh(Vector3 startPoint, Vector3 endPoint, int index)
    {
        GameObject segmentMesh = new GameObject($"SplineSegmentMesh_{index}");
        segmentMesh.tag = "SplineSegment";
        segmentMesh.transform.parent = this.transform;

        Segment3D segmentComponent = segmentMesh.AddComponent<Segment3D>();

        MeshFilter meshFilter = segmentMesh.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = segmentMesh.AddComponent<MeshRenderer>();

        // Przypisywanie meteriału do mesha w zależności od trybu segementacji
        if (isSegmentation)
        {
            meshRenderer.material = segmentMaterials[index % segmentMaterials.Length];
        }
        else
        {
            meshRenderer.material = freeDrawingMaterial;
        }

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
    public void ExtrudeSingleSegment(Spline spline, int knotIndex, bool lastSegement = false)
    {
        if (knotIndex <= 0) return;

        if (lastSegement && !isSegmentation)
        {
            isLastSegment = true;
        }
        Vector3 startPoint = (Vector3)spline[knotIndex - 1].Position;
        Vector3 endPoint = (Vector3)spline[knotIndex].Position;

        DeleteLastSegment();

        GameObject segmentMesh = CreateSegmentMesh(startPoint, endPoint, knotIndex);

        lastSegment = segments.Last();

        // Ustawianie zaleznosci miedzy segmentami
        RecolorPath3D recolorPath3D = segmentMesh.GetComponent<RecolorPath3D>();
        recolorPath3D.setCurrentSegment(segments[knotIndex - 1]);
        if (knotIndex > 1) recolorPath3D.setPreviousSegment(segments[knotIndex - 2]);
    }
    //Dla rysowania live przed dodaniem kolejnej warstwy usuwamy poprzenia
    private void DeleteLastSegment()
    {
        if (segments.Count > 0 && lastSegment != null && !isSegmentation)
        {
            Debug.Log(lastSegment);
            Destroy(lastSegment);
            segments.RemoveAt(segments.Count - 1);

        }

    }

    // Funkcja do ekstrudowania calego spline'a
    public void ExtrudeAndApplyMaterials(Spline spline)
    {

        for (int i = 0; i < spline.Count - 1; i++)
        {

            isLastSegment = CheckIsLastSegment(spline, i);

            Vector3 startPoint = (Vector3)spline[i].Position;
            Vector3 endPoint = (Vector3)spline[i + 1].Position;

            if (isSegmentation || isLastSegment)
            {
                GameObject segmentMesh = CreateSegmentMesh(startPoint, endPoint, i);

                // Ustawianie zaleznooci miedzy segmentami
                RecolorPath3D recolorPath3D = segmentMesh.GetComponent<RecolorPath3D>();
                recolorPath3D.setCurrentSegment(segments[i]);
                if (i > 0) recolorPath3D.setPreviousSegment(segments[i - 1]);
            }
            else
            {
                Mesh mesh = GenerateExtrudedMesh(startPoint, endPoint, isSegmentation);
            }
        }

        restoreSettings();
    }


    public void restoreSettings()
    {
        lastPerpendicularVector = Vector3.zero;
        isFirstSegment = true;
        lastSegment = null;
    }
    public int numberOfLayers()
    {
        return 1;
    }
    private bool CheckIsLastSegment(Spline spline, int index)
    {
        if (!isSegmentation && index == spline.Count - 2)
        {
            return true;

        }
        return isLastSegment;
    }

    private Mesh GenerateExtrudedMeshContinuous(Vector3 start, Vector3 end)
    {
        Mesh mesh = new Mesh();

        Vector3 directionVector = end - start;

        Vector3 currentPerpendicularVector = GetPerpendicularInPlane(start, end);

        if (lastPerpendicularVector == Vector3.zero)
            lastPerpendicularVector = GetPerpendicularInPlane(start, end);

        // Wyznaczanie przesuniec
        Vector3 y1 = lastPerpendicularVector * vectorScale * hight;
        Vector3 y2 = currentPerpendicularVector * vectorScale * hight;
        Vector3 z1 = GetPerpendicular(lastPerpendicularVector, directionVector) * vectorScale * width;
        Vector3 z2 = GetPerpendicular(currentPerpendicularVector, directionVector) * vectorScale * width;
        lastPerpendicularVector = currentPerpendicularVector;

        // Definiowanie wierzcholkow
        Vector3[] newVertices = new Vector3[4]
        {
        end - z2 - y2, // Dolny lewy (nowy)
        end + z2 - y2, // Dolny prawy (nowy)
        end - z2 + y2, // G�rny lewy (nowy)
        end + z2 + y2  // G�rny prawy (nowy)
        };

        // Jezli to pierwszy segment, dodaj tez wierzcholki poczatkowe
        if (verticesList.Count == 0)
        {
            verticesList.Add(start - z1 - y1);
            verticesList.Add(start + z1 - y1);
            verticesList.Add(start - z1 + y1);
            verticesList.Add(start + z1 + y1);
        }

        // Dodaj nowe wierzcholki do listy
        verticesList.AddRange(newVertices);

        // Dodawanie trojkatow dla scian bocznych
        int startIndex = verticesList.Count - 8;  // Indeks pierwszego wierzcho�ka poprzedniego segmentu

        updateTriangleList(startIndex);

        // Zapamietaj wierzcholki koncowe dla nastepnego segmentu
        lastVertices = newVertices;

        // Tworzenie i zwracanie mesha
        mesh.vertices = verticesList.ToArray();
        mesh.triangles = trianglesList.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void updateTriangleList(int index)
    {
        if (index >= 0)
            trianglesList.AddRange(trailBody(index));

        if (isFirstSegment)
            trianglesList.AddRange(initialSideWall(index));

        if (isLastSegment)
            trianglesList.AddRange(finalSideWall(index));
    }


    private int[] trailBody(int index)
    {
        //korpus szlaku bez scian bocznych
        int[] corp = new int[] {
            index, index + 1, index + 5,
            index, index + 5, index + 4,

            index + 1, index + 3, index + 7,
            index + 1, index + 7, index + 5,

            index + 2, index + 6, index + 7,
            index + 2, index + 7, index + 3,

            index, index + 4, index + 6,
            index, index + 6, index + 2
        };
        return corp;
    }
    private int[] finalSideWall(int index)
    {
        isLastSegment = false;
        int[] sideWall = new int[] {
            index + 5, index + 6, index + 4,
            index + 6, index + 5, index + 7,
        };
        return sideWall;

    }

    private int[] initialSideWall(int index)
    {
        isFirstSegment = false;
        int[] sideWall = new int[] {
            index, index + 2, index + 1,
            index + 2, index + 3, index + 1,
        };
        return sideWall;

    }

    private Mesh GenerateExtrudedMeshWithSegmentation(Vector3 start, Vector3 end)
    {
        Mesh mesh = new Mesh();
        Vector3 directionVector = end - start;
        Vector3 currentPerpendicularVector = GetPerpendicularInPlane(start, end);

        if (lastPerpendicularVector == Vector3.zero)
        {
            lastPerpendicularVector = GetPerpendicularInPlane(start, end);
        }
        // Wyznaczanie wspolrzednych przesuniecia wierzcholkow
        Vector3 y1 = lastPerpendicularVector * vectorScale * hight;
        Vector3 y2 = currentPerpendicularVector * vectorScale * hight;
        Vector3 z1 = GetPerpendicular(lastPerpendicularVector, directionVector) * vectorScale * width;
        Vector3 z2 = GetPerpendicular(currentPerpendicularVector, directionVector) * vectorScale * width;
        lastPerpendicularVector = currentPerpendicularVector;


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

        // Wyznaczanie trojkatow mesha pomiedzy wierzcholkami
        int[] triangles = meshTriangles();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        return mesh;
    }

    private int[] meshTriangles()
    {
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
        return triangles;
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

        // Jezli wektor referencyjny jest zerowy, zwracamy domyslna wartosc
        if (reference == Vector3.zero)
        {
            return Vector3.up;
        }
        //punkt pomocniczny na lezy na poprzednim wektorze prostopadlym do punktu startowego
        Vector3 p1 = p2 - lastPerpendicularVector;

        Vector3 v1 = p2 - p1;
        Vector3 v2 = p3 - p1;

        // Sprawdzenie, czy v1 i v2 nie sie wspoliniowe
        Vector3 normal = Vector3.Cross(v1, v2).normalized;
        if (normal == Vector3.zero)
        {
            // Tworzymy punkt pomocniczy w poblizu srodka odcinka, ale nie na lini  i p2-p3
            Vector3 midpoint = (p2 + p3) / 2;
            // Jezli normalna nie istnieje, probujemy innego punktu p1
            p1 = new Vector3(midpoint.x, midpoint.y + 1.0f, midpoint.z); // Dodanie malej perturbacji do y
            normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;

            if (normal == Vector3.zero)
            {
                return Vector3.up; // Ostateczna warto�� awaryjna
            }
        }

        // Projekcja reference na plaszczyzny
        Vector3 referenceProjected = reference - Vector3.Dot(reference, normal) * normal;

        // Jezli projekcja jest zerowa, probujemy z innym punktem p1
        if (referenceProjected == Vector3.zero)
        {
            return Vector3.Cross(normal, Vector3.right).normalized;
        }

        return Vector3.Cross(referenceProjected, normal).normalized;
    }

    public void ClearTrail() //usuwanie traila 
    {
        //Debug.Log(segments.Count);
        foreach (GameObject segment in segments)
        {
            //Debug.Log(segment);
            Destroy(segment);
        }

        segments.Clear();
    }



    public void GenerateCirclePoints(float radius, XRBaseController controller)
    {

        int numberOfSegments = (int)(radius * 200);
        Vector3 center = controller.transform.position;  // Pobierz pozycję obiektu
        Vector3 normal = controller.transform.right;     // Normalna do płaszczyzny (prostopadła do X)

        // Tworzenie lokalnego układu współrzędnych
        Vector3 up = Vector3.up;
        Vector3 tangent = Vector3.Cross(up, normal).normalized;  // Wektor poziomy
        Vector3 bitangent = Vector3.Cross(normal, tangent).normalized; // Wektor pionowy
        float angleOffset = 90;
        Quaternion rotation = Quaternion.AngleAxis(angleOffset, up);
        Spline spline = new Spline();

        for (int i = 0; i < numberOfSegments; i++)
        {

            float angle = i * Mathf.PI * 2 / numberOfSegments;

            BezierKnot knot = new BezierKnot(center + rotation *
                tangent * (Mathf.Cos(angle) * radius) +
                bitangent * (Mathf.Sin(angle) * radius));
            spline.Add(knot);
        }
        spline.Add(spline[0]);
        ExtrudeAndApplyMaterials(spline);
    }

    public void GeneratePolygonPoints(int sides, float radius, XRBaseController controller)
    {
        int numberOfSegments = (int)(radius * 200);
        if (sides < 3) return; // Minimalna ilość boków to 3 (trójkąt)

        Vector3 center = controller.transform.position;  // Środek wielokąta
        Vector3 normal = controller.transform.right;     // Normalna do płaszczyzny
        Vector3 up = Vector3.up;

        // Układ współrzędnych dla wielokąta
        Vector3 tangent = Vector3.Cross(up, normal).normalized;
        Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

        float angleOffset = 90; //Przesuniecie aby wielokat byl rysowany prostopadle do kontrolera
        Quaternion rotation = Quaternion.AngleAxis(angleOffset, up);
        //rotation = rotation * Quaternion.Euler(0, 0, 0);
        Spline spline = new Spline();
        float angle = 0;

        // Obliczanie pozycji wierzchołka wielokąta
        Vector3 lastLocalPoint = tangent * (Mathf.Cos(angle) * radius) +
                             bitangent * (Mathf.Sin(angle) * radius);

        for (int i = 1; i <= sides; i++)
        {
            int k = i % sides;
            angle = k * Mathf.PI * 2 / sides;  // Kąt dla wierzchołka

            // Obliczanie pozycji wierzchołka wielokąta
            Vector3 localPoint = tangent * (Mathf.Cos(angle) * radius) +
                                 bitangent * (Mathf.Sin(angle) * radius);

            Vector3 lengthOfWall = localPoint - lastLocalPoint;
            for (int j = 1; j <= numberOfSegments; j++)
            {
                Vector3 point = lastLocalPoint + j * lengthOfWall / numberOfSegments;
                BezierKnot knot = new BezierKnot(center + rotation * point);
                spline.Add(knot);
            }
            lastLocalPoint = localPoint;
        }
        spline.Add(spline[0]); // Zamknięcie wielokąta
        ExtrudeAndApplyMaterials(spline);
    }



}


