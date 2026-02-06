using UnityEngine;

namespace StateTree
{
    /// <summary>
    /// 상태 + 시퀀스 리스트 실행기 (VisualFSMRunner 의존성 없음)
    /// - Brain이 SetState로 상태를 바꿔준다.
    /// - Runner는 해당 상태의 steps를 순서대로 실행한다.
    /// </summary>
    public class StateTreeRunner : MonoBehaviour
    {
        public StateTreeAsset asset;

        [Header("Bindings")]
        public Animator animator;      // 연출용
        public Transform target;       // 추적 대상(플레이어)
        public float moveSpeed = 3.5f;
        public float turnSpeedDegPerSec = 540f;

        [Header("Debug")]
        [SerializeField] private string currentState;
        [SerializeField] private int stepIndex;
        [SerializeField] private float stepTimer;

        private StateDef _state;

        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            if (asset != null)
                SetState(asset.defaultState);
        }

        public void SetState(string stateName)
        {
            if (asset == null) return;

            var s = asset.FindState(stateName);
            currentState = stateName;

            _state = s;
            stepIndex = 0;
            stepTimer = 0f;
        }

        private void Update()
        {
            if (_state == null || _state.steps == null || _state.steps.Count == 0)
                return;

            // 마지막 step에서 멈춤(원하면 State에 Wait를 넣어 무한 유지)
            if (stepIndex >= _state.steps.Count)
                stepIndex = _state.steps.Count - 1;

            var step = _state.steps[stepIndex];
            bool done = TickStep(step, Time.deltaTime);

            if (done)
            {
                stepIndex++;
                stepTimer = 0f;
                if (stepIndex >= _state.steps.Count)
                    stepIndex = _state.steps.Count - 1;
            }
        }

        private bool TickStep(StepDef s, float dt)
        {
            stepTimer += dt;

            switch (s.type)
            {
                // ---------- Presentation ----------
                case StepType.AnimSetInt:
                    if (animator != null && !string.IsNullOrWhiteSpace(s.str))
                        animator.SetInteger(s.str, Mathf.RoundToInt(s.f1));
                    return true;

                case StepType.AnimSetBool:
                    if (animator != null && !string.IsNullOrWhiteSpace(s.str))
                        animator.SetBool(s.str, s.b1);
                    return true;

                case StepType.AnimTrigger:
                    if (animator != null && !string.IsNullOrWhiteSpace(s.str))
                        animator.SetTrigger(s.str);
                    return true;

                // ---------- Timing ----------
                case StepType.Wait:
                    return stepTimer >= Mathf.Max(0f, s.f1);

                // ---------- Movement (simple) ----------
                case StepType.MoveToTarget:
                    return TickMoveToTarget(range: s.f1, maxTime: s.f2, dt: dt);

                case StepType.TurnToTarget:
                    return TickTurnToTarget(maxDegPerSec: s.f1, maxTime: s.f2, dt: dt);

                // ---------- Combat hook ----------
                case StepType.AttackToken:
                    if (!string.IsNullOrWhiteSpace(s.str))
                        SendMessage("OnAttackToken", s.str, SendMessageOptions.DontRequireReceiver);
                    return true;

                default:
                    return true;
            }
        }

        private bool TickMoveToTarget(float range, float maxTime, float dt)
        {
            if (target == null) return true;

            Vector3 to = target.position - transform.position;
            to.y = 0f;

            float dist = to.magnitude;
            if (dist <= Mathf.Max(0.01f, range))
                return true;

            if (to.sqrMagnitude > 0.0001f)
            {
                var desired = Quaternion.LookRotation(to.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, desired, turnSpeedDegPerSec * dt
                );
            }

            transform.position += transform.forward * (moveSpeed * dt);

            if (maxTime > 0f && stepTimer >= maxTime)
                return true;

            return false;
        }

        private bool TickTurnToTarget(float maxDegPerSec, float maxTime, float dt)
        {
            if (target == null) return true;

            Vector3 to = target.position - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.0001f) return true;

            var desired = Quaternion.LookRotation(to.normalized, Vector3.up);
            float speed = (maxDegPerSec > 0f ? maxDegPerSec : turnSpeedDegPerSec);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, speed * dt);

            float angle = Quaternion.Angle(transform.rotation, desired);
            if (angle < 1f) return true;

            if (maxTime > 0f && stepTimer >= maxTime)
                return true;

            return false;
        }
    }
}
