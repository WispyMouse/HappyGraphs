using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    public bool LaunchedAtStartup { get; set; } = false;

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

    public virtual void StartUp()
    {

    }

    public virtual void Clear()
    {

    }
}
