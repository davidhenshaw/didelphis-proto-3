using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAndWait : IBehaviorState
{
    [SerializeField]
    GameObject gameObject;

    [SerializeField]
    float duration;
    float _elapsed;

    [SerializeField]
    bool deactivateOnComplete;

    [SerializeField]
    bool startActive = false;

    private void Start()
    {
        gameObject.SetActive(startActive);
    }

    public void Enter()
    {
        gameObject.SetActive(true);
    }

    public void Exit()
    {
        _elapsed = 0;
        gameObject.SetActive(false);
    }

    public IBehaviorState.Result Process(float deltaTime)
    {
        _elapsed += deltaTime;
        if(_elapsed >= duration)
            return IBehaviorState.Result.DONE;
        else
            return IBehaviorState.Result.INCOMPLETE;
    }

}
