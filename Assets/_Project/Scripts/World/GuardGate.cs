using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    // The Semi-Final Gate: opens only for guards, reversing ones included (design doc §7.8, IMPL §16).
    // The doorway blocker stops jump raycasts and guard sight while closed; the panels are the look.
    [DefaultExecutionOrder(-40)]
    public sealed class GuardGate : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] ShadowRegistry registry;
        [SerializeField] Collider doorwayBlocker;
        [SerializeField] Transform panelWest;
        [SerializeField] Transform panelEast;
        [Tooltip("Gate footprint in metres: doorway width (X) and gate depth (Z).")]
        [SerializeField] Vector2 size = new Vector2(6f, 1.5f);

        GateLogic logic;
        float open01;
        Vector3 westClosed, eastClosed;

        public bool IsOpen => logic.IsOpen;

        void Awake()
        {
            logic = new GateLogic(tuning.gateCloseDelay);
            westClosed = panelWest.localPosition;
            eastClosed = panelEast.localPosition;
        }

        void Update()
        {
            bool wasOpen = logic.IsOpen;
            logic.Tick(GuardsInZone(), Time.deltaTime);
            if (logic.IsOpen != wasOpen) SfxPlayer.Play(SfxId.GateSlide);
            doorwayBlocker.enabled = !logic.IsOpen;
            open01 = Mathf.MoveTowards(open01, logic.IsOpen ? 1f : 0f, Time.deltaTime / tuning.gatePanelSlideTime);
            Vector3 slide = Vector3.right * (size.x * 0.5f * open01);
            panelWest.localPosition = westClosed - slide;
            panelEast.localPosition = eastClosed + slide;
        }

        // Zone: the gate footprint plus T-GATE-ZONE on each side.
        int GuardsInZone()
        {
            float hx = size.x * 0.5f, hz = size.y * 0.5f + tuning.gateTriggerDepth;
            Vector3 c = transform.position;
            int n = 0;
            var guards = registry.Guards;
            for (int i = 0; i < guards.Count; i++)
            {
                Vector3 p = guards[i].transform.position;
                if (Mathf.Abs(p.x - c.x) <= hx && Mathf.Abs(p.z - c.z) <= hz) n++;
            }
            return n;
        }
    }
}
