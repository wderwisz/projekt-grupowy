using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Klasa do zarz�dzania wygl�dem szlaku w trybie swobodnego rysowania
/// </summary>
public class FreeDrawingCustomizer : MonoBehaviour
{
    public event Action<Color> OnColorChanged;
    private DrawingPath3D drawingPath3DComponent;
    private SplineSegmentMeshExtruder extruder;
    private Color activeColor;
    [SerializeField]
    private List<Color> colors = new List<Color>()
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.magenta,
        Color.yellow,
        Color.black,
        Color.white,
        Color.gray,
    };

    // Domy�lny materia�
    public Material material;

    private void Awake()
    {
        drawingPath3DComponent = GetComponent<DrawingPath3D>();
        extruder = drawingPath3DComponent.GetSplineContainerPrefab().GetComponent<SplineSegmentMeshExtruder>();
        extruder.freeDrawingMaterial = material;    
    }

    [ContextMenu("ChangeColor")]
    public void setRandomColor()
    {
        int randID = UnityEngine.Random.Range(0, colors.Count);
        Color color = colors[randID];  
        //setColor(color);
    }

    // Funkcja do zmiany koloru szlaku 
    public void setColor(int i) {
        Material newMaterial = new Material(material);
        newMaterial.color = colors[i];
        extruder.freeDrawingMaterial = newMaterial;
        activeColor = colors[i];
        OnColorChanged?.Invoke(activeColor);
    }

    public Color getActiveColor()
    {
        return activeColor;
    }

}
