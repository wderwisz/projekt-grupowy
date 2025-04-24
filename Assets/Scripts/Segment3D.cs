using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Segment3D : MonoBehaviour
{
    private enum MaterialType
    {
        DEFAULT = 0,
        BLINKING = 1
    }

    private bool colored = false;
    private MaterialType materialType;

    public void initialize(string name)
    {
        gameObject.name = name;
    }

    public bool isColored()
    {
        return colored;
    }

    public int getMaterialType()
    {
        return (int)materialType;
    } 

    public void setColored(bool x) {
        colored = x;
    }

    public void setMaterialTypeBlinking()
    {
        materialType = MaterialType.BLINKING;
    }

    public void setMaterialTypeDefault()
    {
        materialType = MaterialType.DEFAULT;
    }
}
