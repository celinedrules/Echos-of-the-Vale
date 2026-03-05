using Core.Interfaces;
using StateMachine.States.EnemyStates;
using UnityEngine;

namespace Enemy
{
    public class SlimeA : EnemyController, ICounterable
    {
        public bool CanBeCountered => IsAttacking;
        
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

        protected override void Update()
        {
            base.Update();
            
            Debug.Log(StateMachine.CurrentState);
        }

        public void HandleCounter()
        {
            if (!CanBeCountered)
                return;

            Stun(true);
        }
    }
}