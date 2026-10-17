using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    // Flies in an arc to a fixed point over T-THROW-TIME, then smashes into a fire (IMPL §12.2).
    public sealed class ThrownLantern : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] LanternFire firePrefab;
        [SerializeField] AssetRegistry assets;
        [Tooltip("Arc apex above the straight line (look only).")]
        [SerializeField] float arcHeight = 1.5f;

        Vector3 from, to;
        float t;
        bool oil;

        public void Launch(Vector3 start, Vector3 target, bool oilFire = false)
        {
            oil = oilFire;
            from = start;
            to = target;
            t = 0f;
        }

        // One burst: the pack prefab loops, so the loop is turned off before its first frame.
        void Smash(Vector3 at)
        {
            GameObject go = Instantiate(assets.lanternSmashVfx, at, Quaternion.identity);
            foreach (ParticleSystem ps in go.GetComponentsInChildren<ParticleSystem>())
            {
                ParticleSystem.MainModule main = ps.main;
                main.loop = false;
            }
            Destroy(go, 4f);
        }

        void Update()
        {
            t += Time.deltaTime / tuning.throwTime;
            transform.position = JumpArc.Evaluate(from, to, arcHeight, t);
            if (t < 1f) return;
            LanternFire fire = Instantiate(firePrefab, new Vector3(to.x, 0f, to.z), Quaternion.identity);
            if (oil) fire.UseOilRadius();
            if (assets != null && assets.lanternSmashVfx != null) Smash(fire.transform.position);
            SfxPlayer.Play(SfxId.LanternSmash);
            Destroy(gameObject);
        }
    }
}
