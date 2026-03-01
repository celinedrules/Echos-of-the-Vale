using StateMachine.States.EnemyStates;

namespace Enemy
{
    public class SlimeA : EnemyController
    {
        protected override void Awake()
        {
            base.Awake();

            IdleState = Factory.Create<IdleState>("Idle");
            MoveState = Factory.Create<MoveState>("Move");
        }

        protected override void Start()
        {
            base.Start();
            StateMachine.Initialize(IdleState);
        }
    }
}