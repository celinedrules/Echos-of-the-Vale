using Managers;
using UnityEngine;
using Utilities;

namespace StateMachine.States.EnemyStates
{
    public class BattleState : EnemyState
    {
        private Transform _player;
        private Transform _lastTarget;
        private Timer _battleTimer;

        public override void Enter()
        {
            base.Enter();

            UpdateBattleTimer();

            _player ??= Enemy.PlayerTransform;
            
            if(!ShouldRetreat())
                return;
            
            Debug.Log("Retreat");
        }

        public override void Update()
        {
            base.Update();

            if (ShouldRetreat())
            {
                StateMachine.ChangeState(Enemy.RetreatState);
                return;
            }
            
            if (Enemy.PlayerDetected)
            {
                UpdateTargetIfNeeded();
                UpdateBattleTimer();
            }
            
            if(InAttackRange() && Enemy.PlayerDetected)
            {
                StateMachine.ChangeState(Enemy.AttackState);
                Debug.Log("BattleState: In attack range");
            }
            else
            {
                Enemy.MoveTowardPlayer();
            }
        }

        public override void Exit()
        {
            base.Exit();
            _battleTimer.Cancel();
        }

        private void UpdateTargetIfNeeded()
        {
            if(!Enemy.PlayerDetected)
                return;

            Transform newTarget = Enemy.Sensor.GetPlayerTransform();

            if (newTarget != _lastTarget)
            {
                _lastTarget = newTarget;
                _player = newTarget;
            }
        }

        private void UpdateBattleTimer()
        {
            if(_battleTimer != null && !_battleTimer.IsCompleted)
                _battleTimer.Restart();
            else
                _battleTimer = TimerManager.Instance.CreateTimer(Enemy.BattleTimeDuration, OnBattleTimeExpired);
        }
        
        private void OnBattleTimeExpired() => StateMachine.ChangeState(Enemy.IdleState);
        private bool InAttackRange() => GetDistanceToPlayer() < Enemy.AttackDistance;
        private float GetDistanceToPlayer() => !_player ? float.MaxValue : Vector3.Distance(Enemy.transform.position, _player.position);
        private bool ShouldRetreat() => GetDistanceToPlayer() > Enemy.MinRetreatDistance;
    }
}