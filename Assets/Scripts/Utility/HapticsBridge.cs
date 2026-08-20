using UnityEngine;

namespace WormWars.Core
{
    // Thin wrapper around haptic feedback so call sites read the same as the spec's haptics
    // table. Unity's Handheld.Vibrate() can't distinguish impact strength on its own — swap
    // the bodies here for a native haptics plugin (e.g. iOS UIImpactFeedbackGenerator /
    // Android VibrationEffect) without touching any caller.
    public static class HapticsBridge
    {
        public static void MediumImpact() => Vibrate();
        public static void HeavyImpact() => Vibrate();
        public static void HeavyImpactLong() => Vibrate();
        public static void LightTick() => Vibrate();
        public static void SelectionTick() => Vibrate();
        public static void SustainedRumble() => Vibrate();

        static void Vibrate()
        {
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }
    }
}
