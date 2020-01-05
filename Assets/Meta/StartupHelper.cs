using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupHelper : MonoBehaviour
{
    private void Start()
    {
        foreach (State curState in SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<State>()))
        {
            if (curState.LaunchedAtStartup)
            {
                curState.Clear();
                curState.WakeUp();
                curState.StartUp();
            }
            else
            {
                curState.Clear();
                curState.Sleep();
            }
        }
    }
}
