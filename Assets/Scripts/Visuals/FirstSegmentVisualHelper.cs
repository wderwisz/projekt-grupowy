using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Klasa do oznaczania pierwszego segmentu po narysowaniu szlaku
/// </summary>
public class FirstSegmentVisualHelper : MonoBehaviour 
{
    private List<GameObject> segments;
    [SerializeField] private Material blinkingMaterial;
    [SerializeField] private Material defaultMaterial;
    public void setSegments(List<GameObject> p_segments)
    {
        segments = p_segments;
    }

    private GameObject findFirstToRecolor()
    {
        foreach (GameObject segment in segments) 
        {
            if (!segment.gameObject.GetComponent<Segment3D>().isColored())
            {
                return segment;
            }
        }
        return null;
    }

    public void activateBlinkingSegment()
    {
        if (segments == null) return;
        GameObject segment = findFirstToRecolor();
        if (segment == null) return;
        MeshRenderer renderer = segment.gameObject.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = blinkingMaterial;
    }

    private GameObject[] getAllSegments()
    {
        GameObject[] allSegments = GameObject.FindGameObjectsWithTag("SplineSegment");

        
        return allSegments;
    }

    public void deleteRecolor()
    {
        GameObject[] allSegments = getAllSegments();

        foreach (GameObject segment in allSegments)
        {
            segment.GetComponent<MeshRenderer>().sharedMaterial = defaultMaterial;
            segment.GetComponent<Segment3D>().setColored(false);
        }
    }
}

