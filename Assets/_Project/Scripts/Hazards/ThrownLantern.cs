using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    // Flies in an arc to a fixed point over T-THROW-TIME, then smashes into a fire (IMPL §12.2).
    public sealed class ThrownLantern : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] LanternFire firePrefab;
        [Tooltip("Arc apex above the straight line (look only).")]
        [SerializeField] float arcHeight = 1.5f;

        Vector3 from, to;
        float t;

        public void Launch(Vector3 start, Vector3 target)
        {
            from = start;
            to = target;
            t = 0f;
        }

        void Update()
        {
            t += Time.deltaTime / tuning.throwTime;
            transform.position = JumpArc.Evaluate(from, to, arcHeight, t);
            if (t < 1f) return;
            Instantiate(firePrefab, new Vector3(to.x, 0f, to.z), Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
