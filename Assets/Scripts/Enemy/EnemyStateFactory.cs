using StateMachine.States.EnemyStates;

namespace Enemy
{
    public class EnemyStateFactory
    {
        private readonly EnemyController _enemy;
        private readonly StateMachine.StateMachine _stateMachine;
        
        public EnemyStateFactory(EnemyController enemy, StateMachine.StateMachine stateMachine)
        {
            _enemy = enemy;
            _stateMachine = stateMachine;
        }

        public T Create<T>(string animBoolName) where T : EnemyState, new()
        {
            T  state = new T();
            state.Initialize(_enemy, _stateMachine, animBoolName);
            return state;
        }
    }
}