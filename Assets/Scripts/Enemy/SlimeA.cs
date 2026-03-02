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
            AttackState = Factory.Create<AttackState>("Attack");
            BattleState = Factory.Create<BattleState>("Battle");
            RetreatState = Factory.Create<RetreatState>("Retreat");
            StunnedState = Factory.Create<StunnedState>("Stunned");
            DeathState = Factory.Create<DeathState>("Death");
        }

        protected override void Start()
        {
            base.Start();
            StateMachine.Initialize(IdleState);
        }
    }
}