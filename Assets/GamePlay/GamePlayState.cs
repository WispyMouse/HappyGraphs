using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayState : State
{
    public PlayFieldManager PlayFieldManagerInstance;

    public override void StartUp()
    {
        PlayFieldManagerInstance.InitiateStartupSequence();
    }
}
