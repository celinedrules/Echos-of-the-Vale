// Done
using UnityEngine;

namespace Audio
{
    public static class DebugLogger
    {
        public static void Log(string message, bool debug)
        {
            if (debug)
                Debug.Log("[FusionAudioManager]: " + message);
        }

        public static void LogWarning(string message, bool debug)
        {
            if (debug)
                Debug.LogWarning("[FusionAudioManager]: " + message);
        }
    }
}