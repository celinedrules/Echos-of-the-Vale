// Done
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.EntityData
{
    [CreateAssetMenu(fileName = "Enemy Data - ", menuName = "Echos of the Vale/Enemy Data/Enemy Data")]
    public class EnemyData : EntityData
    {
        [TabGroup("Starting Stats")]
        [SerializeField, HideLabel] private EntityStatsData enemyStatsData;
        
        [TabGroup("Battle Settings")]
        [ProgressBar(0, 10)]
        [SerializeField] private float battleMoveSpeed = 3.0f;
        [TabGroup("Battle Settings")]
        [ProgressBar(0, 10)]
        [SerializeField] private float attackDistance = 2.0f;
        [TabGroup("Battle Settings")]
        [ProgressBar(0, 10)]
        [SerializeField] private float battleTimeDuration = 5.0f;
        [TabGroup("Battle Settings")]
        [ProgressBar(0, 10)]
        [SerializeField] private float minRetreatDistance = 1.0f;
        [TabGroup("Battle Settings")]
        [SerializeField] private Vector2 retreatVelocity;

        [TabGroup("Stunned Settings")]
        [SerializeField] private float stunnedDuration = 1.0f;
        [TabGroup("Stunned Settings")]
        [SerializeField] private float stunnedVelocity = 7.0f;
        [TabGroup("Stunned Settings")]
        [SerializeField] private bool canBeStunned;
        [TabGroup("Stunned Settings")]
        [SerializeField] private int startStunnedFrame = 20;
        [TabGroup("Stunned Settings")]
        [SerializeField] private float stunnedFrameDuration = 0.25f;

        [TabGroup("Movement")]
        [SerializeField] private float idleTime = 2.0f;
        [TabGroup("Movement")]
        [SerializeField] private float moveTime = 2.0f;
        [TabGroup("Movement")]
        [SerializeField] private float moveSpeed = 1.4f;
        [TabGroup("Movement")]
        [SerializeField] private float randomMoveRadius = 5.0f;
        [TabGroup("Movement")]
        [SerializeField, Range(0, 2)] private float moveAnimSpeedMultiplier = 1f;

        [TabGroup("Detection")]
        [SerializeField] private float playerCheckDistance = 10.0f;

        public override EntityStatsData StatsData => enemyStatsData;
        public float BattleMoveSpeed => battleMoveSpeed;
        public float AttackDistance => attackDistance;
        public float BattleTimeDuration => battleTimeDuration;
        public float MinRetreatDistance => minRetreatDistance;
        public Vector2 RetreatVelocity => retreatVelocity;
        public float StunnedDuration => stunnedDuration;
        public float StunnedVelocity => stunnedVelocity;
        public bool CanBeStunned => canBeStunned;
        public int StartStunnedFrame => startStunnedFrame;
        public float StunnedFrameDuration => stunnedFrameDuration;
        public float IdleTime => idleTime;
        public float MoveTime => moveTime;
        public float MoveSpeed => moveSpeed;
        public float RandomMoveRadius => randomMoveRadius;
        public float MoveAnimSpeedMultiplier => moveAnimSpeedMultiplier;
        public float PlayerCheckDistance => playerCheckDistance;
    }
}