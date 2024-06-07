using UnityEngine;

namespace metakazz.FSM
{
    public interface IState
    {
        public void Init(GameObject ctx);
        public void Enter();
        public void Tick();
        public void FixedTimeTick();
        public void Exit();
    }
}