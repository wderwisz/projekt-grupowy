using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxListener : MonoBehaviour
{
    private RecolorPath3D parent;

    public void Init(RecolorPath3D parentScript)
    {
        parent = parentScript;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Controller"))
        {
            if (parent.controllerInZone == null)
            {
                parent.controllerInZone = other;
                parent.OnUnifiedTriggerEnter(other);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Controller") && parent.controllerInZone == other)
        {
            parent.OnUnifiedTriggerExit(other);
            parent.controllerInZone = null;
        }
    }
}
