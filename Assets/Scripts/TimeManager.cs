using System.Collections;
using UnityEngine;

// Klasa licz¹ca czas trwania
public class TimeManager : MonoBehaviour
{
    private float startTime;
    private float endTime;
    private float timeTaken = 0f;


    // Rozpocznij liczenie czasu
    public void StartTimer()
    {
        startTime = Time.time;
    }

    // Zatrzymaj liczenie czasu
    public void StopTimer()
    {
        endTime = Time.time;
        timeTaken = endTime - startTime;
    }

    // Zwróæ policzony czas
    public float GetTimeTaken()
    {
        return timeTaken;
    }

    // Zresetuj czas
    public void resetTime()
    {
        timeTaken = 0;
    }

}
