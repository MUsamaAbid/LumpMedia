//using System.Diagnostics;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public float timerDuration = 7f; // Timer duration in seconds
    private float timer; // Current timer value
    private bool isTimerRunning = false;
    [SerializeField] Manager manager;

    [SerializeField] Text timerUI;
    int t;
    void Start()
    {
        isTimerRunning = false;
        // Initialize the timer
        //ResetTimer();
        //StartTimer();
    }

    void Update()
    {
        // Check if the timer is running
        if (isTimerRunning)
        {
            // Update the timer value
            timer -= Time.deltaTime;
            int t = (int)timer;
            timerUI.text = t.ToString();
            Debug.Log("Time: " + timer);
            // Check if the timer has reached zero
            if (timer <= 0f)
            {
                // Timer has reached zero, perform actions here
                Debug.Log("Timer has reached zero!");
                isTimerRunning = false;
                // Reset the timer
                //ResetTimer();
                manager.TimerEnded();
            }
        }
    }
    public void RestartTimer()
    {
        ResetTimer();
        StartTimer();
    }

    // Method to start or resume the timer
    public void StartTimer()
    {
        isTimerRunning = true;
    }

    // Method to pause the timer
    public void PauseTimer()
    {
        isTimerRunning = false;
    }

    // Method to reset the timer to its initial value
    public void ResetTimer()
    {
        isTimerRunning = false;
        timer = timerDuration;
    }
}
