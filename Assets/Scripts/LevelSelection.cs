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
        // Zniszcz komponenty AR, jeœli s¹
        //Destroy(FindObjectOfType<ARSession>()?.gameObject);
        //Destroy(FindObjectOfType<ARCameraManager>()?.gameObject);

        // Subskrypcja eventu, ¿eby funkcja wykona³a siê ju¿ po za³adowaniu nowej sceny
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(level);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Odsubskrybuj, by nie wykona³o siê wielokrotnie
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Jeœli przechodzimy na StartMenu to robimy restart systemów XR
        // Dlatego, ¿e w obecnej wersji mamy sytuacjê przejœcia AR -> VR
        // Jeœli aplikacja bêdzie full AR to mo¿na siê pozbyc (chyba)
        if(scene.name == "StartMenu")
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();

            XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
    }
}
