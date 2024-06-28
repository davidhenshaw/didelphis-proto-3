using Services;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadNext : IBehaviorState
{
    public void Enter()
    {
        ServiceLocator.TryGetService(out SceneReferenceSequence sceneSeq);
        sceneSeq.LoadNextAsync();
    }

    public void Exit()
    {
    }

    public IBehaviorState.Result Process(float deltaTime)
    {
        return IBehaviorState.Result.DONE;
    }
}
