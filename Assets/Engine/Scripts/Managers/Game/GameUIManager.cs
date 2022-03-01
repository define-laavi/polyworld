using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;
    public Animator uiAnimator;

    private void Awake()
    {
        instance = this;
    }

    public void SetTrigger(string trigger)
    {
        uiAnimator.SetTrigger(trigger);
    }
    public void RemoveTrigger(string trigger)
    {
        uiAnimator.ResetTrigger(trigger);
    }

    public static void StaticSetTrigger(string trigger)
    {
        instance.SetTrigger(trigger);
    }
    public static void StaticRemoveTrigger(string trigger)
    {
        instance.RemoveTrigger(trigger);
    }
}
