using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Klasa licz�ca dok�adno�� kolorowania 
public class AccuracyManager : MonoBehaviour
{
    public static AccuracyManager instance {  get; private set; }

    private bool startedRecoloring = false;
    private bool isRecoloring = false;
    private float timeTotal = 0f;
    private float timeRecoloring = 0f;
    private float timeOffPathThreshold = 0.3f; // czas w sekundach kt�ry musi zosta� przekroczony, aby zacz�� nalicza� czas poza lini�
    private TimeManager timeManager;

    void Awake()
    {

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        timeManager = new TimeManager();
    }

    public void StartRecoloringTimer()
    {
        if (!isRecoloring)
        {
            timeManager.StartTimer();
            startedRecoloring = true;
            isRecoloring = true;
        }
    }

    public void StopRecoloringTimer()
    {
        if (isRecoloring && startedRecoloring)
        {
            timeManager.StopTimer();
            float timeTaken = timeManager.GetTimeTaken();
            timeTotal += timeTaken;
            timeRecoloring += timeTaken;
            isRecoloring = false;
        }
    }

    public void StartIdleTimer()
    {
        if (!isRecoloring && startedRecoloring)
        {
            timeManager.StartTimer();
            isRecoloring = true;
        }
    }

    public void StopIdleTimer()
    {
        if (isRecoloring && startedRecoloring)
        {
            timeManager.StopTimer();
            float timeTaken = timeManager.GetTimeTaken();
            timeTotal += timeTaken;
            if(timeTaken < timeOffPathThreshold)
            {
                timeRecoloring += timeTaken;
            }
            isRecoloring = false;
        }
    }

    public float GetAccuracy()
    {
        return timeTotal > 0 ? (timeRecoloring / timeTotal) * 100f : 99.9f;
    }

    public float GetTimeTotal()
    {
        return timeTotal;
    }

    public float GetTimeRecoloring()
    {
        return timeRecoloring;
    }

    public bool GetStartedRecoloring()
    {
        return startedRecoloring;
    }

    public void Reset()
    {
        timeTotal = 0f;
        timeRecoloring = 0f;
        startedRecoloring = false;
        isRecoloring = false;
    }

   //Stw�rz obiekt je�eli nie ma jeszcze na scenie
   [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        if (instance == null)
        {
            GameObject accuracyManagerObj = new GameObject("AccuracyManager");
            instance = accuracyManagerObj.AddComponent<AccuracyManager>();
            DontDestroyOnLoad(accuracyManagerObj);
        }
    }



}
