using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;
public class LevelSelection : MonoBehaviour
{
    public void LoadLevel(int level)
    {
        // Zniszcz komponenty AR, je�li s�
        //Destroy(FindObjectOfType<ARSession>()?.gameObject);
        //Destroy(FindObjectOfType<ARCameraManager>()?.gameObject);

        // Subskrypcja eventu, �eby funkcja wykona�a si� ju� po za�adowaniu nowej sceny
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(level);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Odsubskrybuj, by nie wykona�o si� wielokrotnie
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Je�li przechodzimy na StartMenu to robimy restart system�w XR
        // Dlatego, �e w obecnej wersji mamy sytuacj� przej�cia AR -> VR
        // Je�li aplikacja b�dzie full AR to mo�na si� pozbyc (chyba)
        if(scene.name == "StartMenu")
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();

            XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
    }
}
