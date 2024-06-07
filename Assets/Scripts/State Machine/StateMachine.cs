using System;
using System.Collections.Generic;
using UnityEngine;

namespace metakazz.FSM
{
    public class StateMachine
    {
        static string INIT_ERROR = "State Machine has not yet been initialized. Did you call Init()?";
        static string ORPHAN_STATE_ERROR = "State {0} does not have a From transition. The state machine will be unable to leave this state once entered";

        private IState _currentState;
        public IState CurrentState { get => _currentState; }
        private GameObject _gameObject;

        private Dictionary<IState, List<Transition>> _transitions = new Dictionary<IState, List<Transition>>();
        private List<Transition> _currentTransitions = new List<Transition>();
        private List<Transition> _anyTransitions = new List<Transition>();

        private static List<Transition> EmptyTransitions = new List<Transition>(0);
        private bool _initialized = false;

        public bool SuppressWarnings = false;
        public bool DebugLogging { get; set; } = false;
        /// <summary>
        /// Create a new state machine with no states. The <paramref name="context"/> object is used to pass Monobehaviour information into the machine's states
        /// </summary>
        /// <param name="context"></param>
        public StateMachine(GameObject context)
        {
            _gameObject = context;
        }

        public StateMachine(GameObject gameObject, bool suppressWarnings) : this(gameObject)
        {
            SuppressWarnings = suppressWarnings;
        }


        /// <summary>
        /// Initializes all states that have been added as transitions. 
        /// IT IS RECOMMENDED TO USE THE BUILDER INSTEAD OF CALLING THIS DIRECTLY
        /// </summary>
        public void Init()
        {
            if (_initialized)
            {
                Debug.LogWarning("State Machine has already been initialized");
                return;
            }

            if (_currentState != null)
                _currentState.Init(_gameObject);
            HashSet<IState> initialized = new HashSet<IState>();

            //Initialize all From states
            foreach(IState state in _transitions.Keys)
            {
                if(initialized.Add(state))
                    state.Init(_gameObject);
            }

            //Initialize all the To states
            foreach (Transition transition in _anyTransitions)
            {
                InitStateInTransition(initialized, transition);
            }

            foreach (List<Transition> transitions in _transitions.Values)
            {
                foreach(Transition transition in transitions)
                {
                    InitStateInTransition(initialized, transition);
                }
            }

            this._initialized = true;
        }

        /// <summary>
        /// Executes the logic of the current state of the state machine
        /// </summary>
        public void Tick()
        {
            if (!_initialized)
            {
                Debug.LogError(INIT_ERROR);
                return;
            }
            _currentState?.Tick();
        }

        public void FixedTimeTick()
        {
            if (!_initialized)
            {
                Debug.LogError(INIT_ERROR);
                return;
            }
            _currentState?.FixedTimeTick();
        }

        /// <summary>
        /// Checks the conditions for the current state to transition to another
        /// </summary>
        public void Evaluate()
        {
            if (!_initialized)
            {
                Debug.LogError(INIT_ERROR);
                return;
            }

            var transition = GetTransition();
            if (transition != null)
                SetState(transition.To);
        }

        public void EvaluateAndTick()
        {
            Evaluate();
            _currentState?.Tick();
        }

        public void SetState(IState state)
        {
            if (state == _currentState)
                return;

            if (!_initialized)
            {
                _currentState = state;
                return;
            }

            _currentState?.Exit();
            _currentState = state;

            _transitions.TryGetValue(_currentState, out _currentTransitions);
            if (_currentTransitions == null)
                _currentTransitions = EmptyTransitions;

            _currentState?.Enter();

            if(DebugLogging)
                Debug.Log("Entered " + state.ToString());
        }
        /// <summary>
        /// Transition from <paramref name="from"/>State to <paramref name="to"/>State if <paramref name="predicate"/> is true
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="predicate"></param>
        public void AddTransition(IState from, IState to, Func<bool> predicate)
        {
            if (_transitions.TryGetValue(from, out var transitions) == false)
            {
                transitions = new List<Transition>();
                _transitions[from] = transitions;
            }

            transitions.Add(new Transition(to, predicate));
        }

        /// <summary>
        /// Transition from any state to the <paramref name="to"/>State if <paramref name="predicate"/> is true
        /// </summary>
        /// <param name="to"></param>
        /// <param name="predicate"></param>
        public void AddAnyTransition(IState to, Func<bool> predicate)
        {
            _anyTransitions.Add(new Transition(to, predicate));
        }

        private void InitStateInTransition(HashSet<IState> initialized, Transition transition)
        {
            IState state = transition.To;
            if (initialized.Add(state))
            {//was added to set (wasn't previously init'd)
                state.Init(_gameObject);
                if(!SuppressWarnings)
                    Debug.LogWarningFormat(_gameObject, ORPHAN_STATE_ERROR, state.ToString());
            }
        }
        
        private Transition GetTransition()
        {
            foreach (var transition in _currentTransitions)
                if (transition.Condition())
                    return transition;

            foreach (var transition in _anyTransitions)
                if (transition.Condition())
                    return transition;
            return null;
        }
        
        private class Transition
        {
            public static bool operator true(Transition t) => t.Condition();
            public static bool operator false(Transition t) => t.Condition();

            public Func<bool> Condition { get; }
            public IState To { get; }

            public Transition(IState to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }
        }

        private struct StateRecord
        {
            public readonly string ID;
            public readonly List<Transition> Transitions;
        }

        public class StateMachineBuilder
        {
            StateMachine _stateMachine;

            public StateMachineBuilder(GameObject context)
            {
                _stateMachine = new StateMachine(context);
            }

            public StateMachineBuilder(GameObject context, bool suppressWarnings) : this(context) 
            {
                _stateMachine = new StateMachine(context, suppressWarnings);
            }

            public StateMachine Build()
            {
                _stateMachine.Init();
                return _stateMachine;
            }

            public StateMachineBuilder WithGameObject(GameObject gameObject)
            {
                _stateMachine._gameObject = gameObject;
                return this;
            }

            public StateMachineBuilder WithTransition(IState from, IState to, Func<bool> predicate)
            {
                _stateMachine.AddTransition(from, to, predicate);
                return this;
            }

            public StateMachineBuilder WithAnyTransition(IState to, Func<bool> predicate)
            {
                _stateMachine.AddAnyTransition(to, predicate);
                return this;
            }
        }
    }

    public class FSM
    {
        static string INIT_ERROR = "State Machine has not yet been initialized. Did you call Init()?";
        static string ORPHAN_STATE_ERROR = "State {0} does not have a From transition. The state machine will be unable to leave this state once entered";

        private string _currentStateID;
        public string CurrentState { get => _currentStateID; }
        private StateRecord _currentStateRecord;
        private GameObject _gameObject;

        private Dictionary<string, StateRecord> _records = new Dictionary<string, StateRecord>();
        private List<Transition> _currentTransitions = new List<Transition>();
        private List<Transition> _anyTransitions = new List<Transition>();
        private List<Action> _eventTransitions = new List<Action>();


        private static List<Transition> EmptyTransitions = new List<Transition>(0);
        private bool _initialized = false;

        public bool SuppressWarnings = false;
        public bool DebugLogging { get; set; } = false;
        /// <summary>
        /// Create a new state machine with no states. The <paramref name="context"/> object is used to pass Monobehaviour information into the machine's states
        /// </summary>
        /// <param name="context"></param>
        public FSM(GameObject context)
        {
            _gameObject = context;
        }

        public FSM(GameObject gameObject, bool suppressWarnings) : this(gameObject)
        {
            SuppressWarnings = suppressWarnings;
        }


        /// <summary>
        /// Executes the logic of the current state of the state machine
        /// </summary>
        public void Tick()
        {
            _records[_currentStateID].State?.Tick();
        }

        public void FixedTimeTick()
        {
            _records[_currentStateID].State?.FixedTimeTick();
        }

        /// <summary>
        /// Checks the conditions for the current state to transition to another
        /// </summary>
        public void Evaluate()
        {
            var transition = GetTransition();
            if (transition != null)
                SetState(transition.To);
        }

        public void EvaluateAndTick()
        {
            Evaluate();
            _currentStateRecord.State?.Tick();
        }

        public void SetState(string nextStateID)
        {
            if (nextStateID == _currentStateID)
                return;

            //if (!_initialized)
            //{
            //    _currentStateID = nextStateID;
            //    _currentStateRecord = _records[_currentStateID];
            //    return;
            //}

            _currentStateRecord.State?.Exit();
            _currentStateID = nextStateID;
            
            _records.TryGetValue(_currentStateID, out _currentStateRecord);
            _currentTransitions = _currentStateRecord.Transitions;
            if (_currentTransitions == null)
                _currentTransitions = EmptyTransitions;

            _currentStateRecord.State?.Enter();

            if (DebugLogging)
                Debug.Log("Entered " + nextStateID.ToString());
        }
        
        public void RegisterState(string stateID, IState state)
        {
            _records.Add(stateID, new StateRecord(state));
        }

        /// <summary>
        /// Transition from <paramref name="from"/>State to <paramref name="to"/>State if <paramref name="predicate"/> is true
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="predicate"></param>
        public void AddTransition(string from, string to, Func<bool> predicate)
        {
            if (_records.TryGetValue(from, out var stateRecord) == false)
            {
                Debug.LogError($"State \"{from}\" not registered in state machine.");
                return;
            }

            stateRecord.Transitions.Add(new Transition(to, predicate));
        }

        public Action CreateTransitionCallback(string from, string to)
        {
            if (_records.TryGetValue(from, out var stateRecord) == false)
            {
                Debug.LogError($"State \"{from}\" not registered in state machine.");
                return null;
            }

            Action callback = () =>
            {
                if (this.CurrentState.Equals(from))
                {
                    SetState(to);
                }
            };

            return callback;
        }
        /// <summary>
        /// Transition from any state to the <paramref name="to"/>State if <paramref name="predicate"/> is true
        /// </summary>
        /// <param name="to"></param>
        /// <param name="predicate"></param>
        public void AddAnyTransition(string to, Func<bool> predicate)
        {
            _anyTransitions.Add(new Transition(to, predicate));
        }

        private Transition GetTransition()
        {
            foreach (var transition in _currentTransitions)
                if (transition.Condition())
                    return transition;

            foreach (var transition in _anyTransitions)
                if (transition.Condition())
                    return transition;
            return null;
        }

        private class Transition
        {
            public static bool operator true(Transition t) => t.Condition();
            public static bool operator false(Transition t) => t.Condition();

            public Func<bool> Condition { get; protected set; }
            public string To { get; }

            public Transition(string to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }
        }
        private class EventTransition : Transition
        {
            public EventTransition(string to, Func<bool> condition) : base(to, condition)
            {
            }

            public void OnEventRaised()
            {
                Condition = () => true;
            }
        }

        private struct StateRecord
        {
            public readonly IState State;
            public readonly List<Transition> Transitions;

            public StateRecord(IState state)
            {
                State = state;
                Transitions = new List<Transition>();
            }

            public Action CreateEventTransition(string from, string to, FSM stateMachine)
            {
                return () => { 
                    if(stateMachine.CurrentState.Equals(from))
                        stateMachine.SetState(to); 
                };
            }
        }
    }
}
