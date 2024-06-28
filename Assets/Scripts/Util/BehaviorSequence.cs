using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorSequence : MonoBehaviour
{
    [SerializeReference]
    public IBehaviorState[] behaviors;

    private int _currentIndex = 0;
    private IBehaviorState _currentState;
    public IBehaviorState CurrentState => _currentState;

    public bool isComplete { get; private set; }

    public BehaviorSequence(ICollection<IBehaviorState> behaviors)
    {
        behaviors = new List<IBehaviorState>(behaviors);
    }

    private void Start()
    {
        _currentIndex = -1;
        GoToNextState();
    }

    private void GoToNextState()
    {
        _currentState?.Exit();

        if(_currentIndex >= behaviors.Length - 1)
        {
            isComplete = true;
            return;
        }

        _currentIndex = (_currentIndex + 1) % behaviors.Length;
        _currentState = behaviors[_currentIndex];

        _currentState.Enter();
    }

    private void Update()
    {
        var result = _currentState.Process(Time.deltaTime);

        if(result.Equals(IBehaviorState.Result.DONE))
        {
            GoToNextState();
        }
    }
}
