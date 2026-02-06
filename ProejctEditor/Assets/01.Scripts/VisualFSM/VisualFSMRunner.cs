using UnityEngine;

namespace VisualFSM
{
    /// <summary>
    /// Presentation FSM Runner (B 타입).
    /// - 외부 로직이 "현재 상태"를 결정 -> SetLogicState("Chase")
    /// - 이 Runner는 애니/이펙트/사운드만 적용
    /// </summary>
    public class VisualFSMRunner : MonoBehaviour
    {
        public VisualFSMAsset asset;

        [Header("Bindings")]
        public Animator animator;
        public AudioSource audioSource;

        [Tooltip("VFX spawned as children of this transform (if parentVfxToOwner == true).")]
        public Transform vfxRoot;

        [Header("Debug")]
        [SerializeField] private string currentLogicState = "";
        private string prevBoolParam = "";

        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
            audioSource = GetComponentInChildren<AudioSource>();
            vfxRoot = transform;
        }

        private void Awake()
        {
            if (vfxRoot == null) vfxRoot = transform;
        }

        private void Start()
        {
            if (asset != null && !string.IsNullOrWhiteSpace(asset.defaultLogicState))
            {
                SetLogicState(asset.defaultLogicState);
            }
        }

        /// <summary>
        /// Brain(로직)에서 호출: 상태 변경이 필요할 때 이 함수만 부르면 됨.
        /// </summary>
        public void SetLogicState(string logicState)
        {
            if (asset == null) return;
            if (string.IsNullOrWhiteSpace(logicState)) return;

            // 같은 상태면 무시
            if (!string.IsNullOrEmpty(currentLogicState) &&
                string.Equals(currentLogicState, logicState, System.StringComparison.OrdinalIgnoreCase))
                return;

            var state = asset.FindByLogicState(logicState);
            if (state == null)
            {
                // 상태 미등록이면 그냥 갱신만
                currentLogicState = logicState;
                return;
            }

            ApplyState(state);
            currentLogicState = logicState;
        }

        private void ApplyState(VisualState s)
        {
            // Animator
            if (animator != null)
            {
                // 이전 bool param 자동 리셋 (옵션)
                if (!string.IsNullOrEmpty(prevBoolParam) && prevBoolParam != s.animatorBoolParam)
                    animator.SetBool(prevBoolParam, false);

                if (!string.IsNullOrEmpty(s.animatorIntParam))
                    animator.SetInteger(s.animatorIntParam, s.animatorIntValue);

                if (!string.IsNullOrEmpty(s.animatorBoolParam))
                {
                    animator.SetBool(s.animatorBoolParam, s.animatorBoolValue);
                    prevBoolParam = s.animatorBoolParam;
                }
                else
                {
                    prevBoolParam = "";
                }

                if (!string.IsNullOrEmpty(s.animatorTrigger))
                    animator.SetTrigger(s.animatorTrigger);
            }

            // VFX
            if (s.enterVfxPrefab != null)
            {
                var spawnTr = (s.parentVfxToOwner && vfxRoot != null) ? vfxRoot : null;
                var go = Instantiate(s.enterVfxPrefab, transform.position, transform.rotation, spawnTr);
                go.transform.localScale = s.vfxLocalScale;
            }

            // SFX
            if (s.enterSfx != null)
            {
                if (audioSource != null) audioSource.PlayOneShot(s.enterSfx);
                else AudioSource.PlayClipAtPoint(s.enterSfx, transform.position);
            }
        }
    }
}
