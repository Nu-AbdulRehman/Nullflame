using System;
using UnityEngine;

namespace Nullflame
{
    // Unlocked by eating the Time SCP (design doc §7.4, IMPL §14). Reverse arrives in B6, rewind in B7.
    public sealed class TimePowers : MonoBehaviour
    {
        public bool HasTimePower { get; private set; }
        public event Action Granted;

        public void Grant()
        {
            if (HasTimePower) return;
            HasTimePower = true;
            Granted?.Invoke();
        }
    }
}
