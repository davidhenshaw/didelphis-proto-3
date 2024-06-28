using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBehaviorState
{
    public enum Result
    {
        DONE,
        INCOMPLETE,
    }

    void Enter();

    Result Process(float deltaTime);

    void Exit();
}
