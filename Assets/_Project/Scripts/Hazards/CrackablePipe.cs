using System.Collections;
using UnityEngine;

namespace Nullflame
{
    // One Final Corridor crack point (design doc §7.6, IMPL §18.2). ConsumeAction runs the hold meter;
    // this spawns the oil drops on a crack and makes every corridor guard throw at a burst.
    public sealed class CrackablePipe : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] FacilityLayout layout;
        [SerializeField] ShadowArea oilDropPrefab;
        public ShadowArea strip;
        public Transform[] dropAnchors;
        public Transform burstPoint;
        [SerializeField] ParticleSystem drip;

        public bool Cracked { get; private set; }

        public void Crack()
        {
            Cracked = true;
            if (drip != null) drip.Play();
            SfxPlayer.Play(SfxId.PipeCrack);
            StartCoroutine(SpawnDrops());
        }

        // T-PIPE-DROPS drops, the first at once, the last T-PIPE-DROP-TIME later. Permanent (§7.6).
        IEnumerator SpawnDrops()
        {
            float gap = dropAnchors.Length > 1 ? tuning.pipeDropDuration / (dropAnchors.Length - 1) : 0f;
            for (int i = 0; i < dropAnchors.Length; i++)
            {
                if (i > 0) yield return new WaitForSeconds(gap);
                Instantiate(oilDropPrefab, dropAnchors[i].position, Quaternion.identity)
                    .SetSize(Vector2.one * tuning.oilDropSize);
            }
        }

        // Every lantern guard in the Final Corridor throws at the burst point; the oil makes the fire bigger.
        public void Burst()
        {
            Cracked = true;
            if (drip != null) drip.Play();
            SfxPlayer.Play(SfxId.PipeBurst);
            layout.TryGetArea("FinalCorridor", out Rect corridor);
            var guards = GameContext.Current.shadows.Guards;
            for (int i = 0; i < guards.Count; i++)
            {
                Vector3 p = guards[i].transform.position;
                if (guards[i].HasLantern && corridor.Contains(new Vector2(p.x, p.z)))
                    guards[i].GetComponent<LanternAttack>().ThrowAt(burstPoint.position, oil: true);
            }
        }
    }
}
