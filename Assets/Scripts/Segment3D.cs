using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Segment3D : MonoBehaviour
{
    private bool colored = false;

    public void initialize(string name)
    {
        gameObject.name = name;
    }

    public bool isColored()
    {
        return colored;
    }

    public void setColored(bool x) {
        colored = x;
    }
}
