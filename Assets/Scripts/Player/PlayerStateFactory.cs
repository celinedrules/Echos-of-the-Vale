using StateMachine.States.PlayerStates;

namespace Player
{
    public class PlayerStateFactory
    {
        private readonly PlayerController _player;
        private readonly StateMachine.StateMachine _stateMachine;
        
        public PlayerStateFactory(PlayerController player, StateMachine.StateMachine stateMachine)
        {
            _player = player;
            _stateMachine = stateMachine;
        }

        public T Create<T>(string animBoolName) where T : PlayerState, new()
        {
            T state = new T();
            state.Initialize(_player, _stateMachine, animBoolName);
            return state;
        }
    }
}