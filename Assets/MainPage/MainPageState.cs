using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPageState : State
{
    public GamePlayState GamePlayStateInstance;

    public Text EventDescription;
    public Animator AnimatorInstance;

    public override void Clear()
    {
        EventDescription.gameObject.SetActive(false);
    }

    public void QuickPlayButtonHovered()
    {
        EventDescription.text = "Start up a simple game of Happy Graphs right away, using the Quick Play rule set";
        EventDescription.gameObject.SetActive(true);
    }

    public void QuickPlayButtonSubmitted()
    {
        AnimatorInstance.SetBool("SlideOut", true);
        GamePlayStateInstance.StartUp();
    }
}
