﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    public bool LaunchedAtStartup;

    private void Awake()
    {
        QuietInitialization();

        if (LaunchedAtStartup)
        {
            gameObject.SetActive(true);
            StartUp();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    protected virtual void QuietInitialization()
    {
        Clear();
    }

    public void WakeUp()
    {
        gameObject.SetActive(true);
    }

    public virtual void StartUp()
    {

    }

    public virtual void Clear()
    {

    }

    public void Sleep(float afterSeconds = 0)
    {
        StartCoroutine(SleepAfterWait(afterSeconds));
    }

    IEnumerator SleepAfterWait(float afterSeconds)
    {
        yield return new WaitForSeconds(afterSeconds);
        gameObject.SetActive(false);
    }
}