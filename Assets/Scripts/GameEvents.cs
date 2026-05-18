using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance;

    public Action onBikeIdle;
    public Action onBikeDragged;
    public Action onBikeLaunched;
    public Action onBikeStopped;

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void BikeIdle()
    {
        onBikeIdle?.Invoke(); 
    }

    public void BikeDragged()
    {
        onBikeDragged?.Invoke();
    }

    public void BikeLaunched()
    {
        onBikeLaunched?.Invoke();
    }

    public void BikeStopped()
    {
        onBikeStopped?.Invoke();
    }
}
