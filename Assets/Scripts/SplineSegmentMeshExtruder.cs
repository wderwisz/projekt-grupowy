using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class SplineSegmentMeshExtruder : MonoBehaviour
{
    public Material[] segmentMaterials; // Tablica materia³ów do ró¿nych segmentów spline'a

    // Liczba wierzcho³ków bry³y ekstrudowanej na pojedynczym segmencie i liczba jej trójk¹tów
    private const int numberOfVertices = 8;
    private const int numberOfTriangles = 12;

    private float width = 0.01f;  // Szerokoœæ segmentu


    // Funkcja do ekstrudowania pojedynczego segmentu. Wywo³ywana po dodaniu kolejnego knota do spline'a
    public void ExtrudeSingleSegment(Spline spline, int knotIndex)
    {
        GameObject segmentMesh = new GameObject($"SplineSegmentMesh_{knotIndex}");
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
        segmentMesh.AddComponent<RecolorPath3D>();
    }

    // funkcja do ekstrudowania ca³ego spline'a po zakoñczeniu rysowania 
    public void ExtrudeAndApplyMaterials(Spline spline)
    {
        for (int i = 0; i < spline.Count - 1; i++)
        {
            GameObject segmentMesh = new GameObject($"SplineSegmentMesh_{i}");
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
            segmentMesh.AddComponent<RecolorPath3D>();

        }
    }

    // Funkcja tworz¹ca mesh w ramach pojedynczego segmentu
    private Mesh GenerateExtrudedMesh(Vector3 start, Vector3 end)
    {
        Mesh mesh = new Mesh();
        Vector3 direction = end - start;

        // Wyznaczanie wspó³rzêdnych przesuniêæ wierzcho³ków
        Vector3 temp = Vector3.up * width / 2;
        Vector3 right = Vector3.Cross(direction.normalized, temp).normalized * width;
        Vector3 up = Vector3.Cross(direction.normalized, right).normalized * width;

        // Wyznaczenie konkretnych wierzcho³ków segmentu
        Vector3[] vertices = new Vector3[numberOfVertices]
        {
            start - right - up,  //0
            start + right - up,
            end - right - up, 
            end + right - up,
            start - right + up,
            start + right + up,
            end - right + up,
            end + right + up  //7
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

    // Do usuniêcia, dodawanie rêczne boxColliderów - teraz dzia³a na meshColliderze
    public void AddCollidersToSpline(SplineContainer currentSpline)
    {
        for (int i = 0; i < currentSpline.Spline.Count - 1; i++)
        {
            GameObject colliderSegment = new GameObject($"SplineCollider_{i}");
            colliderSegment.transform.parent = currentSpline.transform;

            BoxCollider boxCollider = colliderSegment.AddComponent<BoxCollider>();
            boxCollider.AddComponent<DestroyingSplineSegment>();
            boxCollider.isTrigger = true;

            Rigidbody rb = colliderSegment.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            Vector3 start = (Vector3)currentSpline.Spline[i].Position;
            Vector3 end = (Vector3)currentSpline.Spline[i + 1].Position;

            Vector3 midPoint = (start + end) / 2;
            colliderSegment.transform.position = midPoint;

            float segmentLength = Vector3.Distance(start, end);
            boxCollider.size = new Vector3(0.015f, 0.015f, segmentLength);

            colliderSegment.transform.LookAt(end);
            colliderSegment.tag = "SplineSegment";
        }
    }
}
