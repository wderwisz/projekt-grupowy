using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HapticController : MonoBehaviour
{
    public XRBaseController leftController, rightController;
    private float defatultIntensity = 0.5f;
    private float defaultDuration = 0.3f;


    [ContextMenu("Test Haptics")]
    public void TestHaptics()
    {
        leftController.SendHapticImpulse(defatultIntensity, defaultDuration);
        rightController.SendHapticImpulse(defatultIntensity, defaultDuration);
    }
    public static void SendHaptics(XRBaseController controller, float intensity, float duration)
    {
        controller.SendHapticImpulse(intensity, duration);
    }

}
