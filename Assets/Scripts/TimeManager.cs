using System.Collections;
using UnityEngine;

// Klasa licz¹ca czas trwania
public class TimeManager : MonoBehaviour
{
    private float startTime;
    private float endTime;
    private float timeTaken = 0f;

    private bool isRunning = false;
    private bool isPaused = false;
    private float pauseStartTime;
    private float pausedTimeTotal = 0f;

    // Rozpocznij liczenie czasu
    public void StartTimer()
    {
        startTime = Time.time;
        isRunning = true;
        isPaused = false;
        pausedTimeTotal = 0f;
    }

    // Zatrzymaj liczenie czasu
    public void StopTimer()
    {
        if (!isRunning) return;

        if (isPaused)
        {
            endTime = pauseStartTime;
        }
        else
        {
            endTime = Time.time;
        }

        timeTaken = endTime - startTime - pausedTimeTotal;

        isRunning = false;
        isPaused = false;
    }

    // Zatrzymaj czas tymczasowo (pauza)
    public void PauseTimer()
    {
        if (isRunning && !isPaused)
        {
            pauseStartTime = Time.time;
            isPaused = true;
        }
    }

    // Kontynuuj liczenie po pauzie
    public void ResumeTimer()
    {
        if (isRunning && isPaused)
        {
            float pausedDuration = Time.time - pauseStartTime;
            pausedTimeTotal += pausedDuration;
            isPaused = false;
        }
    }

    // Zwróæ policzony czas
    public float GetTimeTaken()
    {
        if (isRunning)
        {
            float currentTime = isPaused ? pauseStartTime : Time.time;
            return currentTime - startTime - pausedTimeTotal;
        }

        return timeTaken;
    }

    // Zresetuj czas
    public void resetTime()
    {
        timeTaken = 0f;
        isRunning = false;
        isPaused = false;
        pausedTimeTotal = 0f;
    }
}
