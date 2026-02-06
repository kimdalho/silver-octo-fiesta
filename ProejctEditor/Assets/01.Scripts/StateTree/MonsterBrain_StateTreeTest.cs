using UnityEngine;

namespace StateTree
{
    public class MonsterBrain_Test : MonoBehaviour
    {
        public StateTreeRunner runner;
        public Transform target;

        public float aggroRange = 10f;
        public float attackRange = 2.5f;

        private void Reset()
        {
            runner = GetComponent<StateTreeRunner>();
        }

        private void Update()
        {
            if (runner == null) return;

            runner.target = target;

            if (target == null)
            {
                runner.SetState("Idle");
                return;
            }

            float d = Vector3.Distance(transform.position, target.position);

            if (d <= attackRange) runner.SetState("Attack");
            else if (d <= aggroRange) runner.SetState("Combat");
            else runner.SetState("Idle");
        }
    }
}
