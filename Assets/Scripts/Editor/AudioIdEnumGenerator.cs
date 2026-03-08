// Done
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Data.Audio;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class AudioIdEnumGenerator
    {
        private const string GeneratedPath = "Assets/Scripts/Audio/Generated";

        [MenuItem("Tools/Audio/Generate All Audio ID Enums")]
        public static void GenerateAllEnums()
        {
            EnsureDirectoryExists();

            string[] guids = AssetDatabase.FindAssets("t:AudioData");

            var groupedByType = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<AudioData>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(data => data != null)
                .GroupBy(data => data.GetType());

            foreach (var group in groupedByType)
            {
                var info = GetEnumInfoForType(group.Key);
                if (info == null)
                    continue;

                var allIds = group
                    .Where(data => data.AudioIds != null)
                    .SelectMany(data => data.AudioIds)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .OrderBy(id => id)
                    .ToList();

                if (allIds.Count > 0)
                    GenerateCombinedFile(info.Value.enumName, info.Value.trackType, allIds);
            }

            AssetDatabase.Refresh();
            Debug.Log("[AudioIdEnumGenerator] All audio ID enums generated successfully!");
        }

        public static void GenerateEnumForAsset(AudioData audioData)
        {
            if (audioData == null)
                return;

            EnsureDirectoryExists();

            var info = GetEnumInfoForType(audioData.GetType());
            if (info == null)
            {
                Debug.LogWarning($"[AudioIdEnumGenerator] Unknown AudioData type: {audioData.GetType().Name}");
                return;
            }

            string[] guids = AssetDatabase.FindAssets($"t:{audioData.GetType().Name}");

            var allIds = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<AudioData>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(data => data != null && data.AudioIds != null)
                .SelectMany(data => data.AudioIds)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            if (allIds.Count == 0)
            {
                Debug.LogWarning($"[AudioIdEnumGenerator] No audio IDs found for {info.Value.enumName}");
                return;
            }

            GenerateCombinedFile(info.Value.enumName, info.Value.trackType, allIds);

            AssetDatabase.Refresh();
            Debug.Log($"[AudioIdEnumGenerator] Generated {info.Value.enumName} with {allIds.Count} entries.");
        }

        private static (string enumName, string trackType)? GetEnumInfoForType(System.Type type)
        {
            return type.Name switch
            {
                "BackgroundAudioData" => ("BackgroundAudioId", "Background"),
                "PlayerAudioData" => ("PlayerAudioId", "PlayerFX"),
                "EnemyAudioData" => ("EnemyAudioId", "EnemyFX"),
                "EnvironmentAudioData" => ("EnvironmentAudioId", "EnvironmentFX"),
                "DialogueAudioData" => ("DialogueAudioId", "Dialogue"),
                "UiAudioData" => ("UiAudioId", "UI"),
                _ => null
            };
        }

        private static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(GeneratedPath))
                Directory.CreateDirectory(GeneratedPath);
        }

        private static void GenerateCombinedFile(string enumName, string trackType,
            System.Collections.Generic.List<string> ids)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// This file is auto-generated. Do not modify manually.");
            sb.AppendLine(
                "// Regenerate using the button on AudioData assets or Tools > Audio > Generate All Audio ID Enums");
            sb.AppendLine();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine("namespace Audio");
            sb.AppendLine("{");

            // Enum definition
            sb.AppendLine($"    public enum {enumName}");
            sb.AppendLine("    {");
            foreach (string id in ids)
            {
                string sanitized = SanitizeEnumValue(id);
                sb.AppendLine($"        {sanitized},");
            }

            sb.AppendLine("    }");
            sb.AppendLine();

            // Extension method
            sb.AppendLine("    public static partial class AudioIdExtensions");
            sb.AppendLine("    {");
            sb.AppendLine($"        public static string ToId(this {enumName} audioId) => audioId.ToString();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // Partial class with overloads
            sb.AppendLine("    public partial class FusionAudioManager");
            sb.AppendLine("    {");
            sb.AppendLine(
                $"        public void PlayAudio({enumName} audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>");
            sb.AppendLine($"            PlayAudio(AudioTrackType.{trackType}, audioId.ToId(), fade, delay, speed);");
            sb.AppendLine();
            sb.AppendLine(
                $"        public void PlayAudio({enumName} audioId, float fadeDuration, float delay = 0.0f, float speed = 1.0f) =>");
            sb.AppendLine(
                $"            PlayAudio(AudioTrackType.{trackType}, audioId.ToId(), fadeDuration, delay, speed);");
            sb.AppendLine();
            sb.AppendLine(
                $"        public void PlayAudio({enumName} audioId, AudioSource source, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>");
            sb.AppendLine(
                $"            PlayAudio(AudioTrackType.{trackType}, audioId.ToId(), source, fade, delay, speed);");
            sb.AppendLine();
            sb.AppendLine(
                $"        public void PlayRandomAudio({enumName} audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>");
            sb.AppendLine(
                $"            PlayRandomAudio(AudioTrackType.{trackType}, audioId.ToId(), fade, delay, speed);");
            sb.AppendLine();
            sb.AppendLine(
                $"        public void PlayRandomAudio({enumName} audioId, AudioSource source, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>");
            sb.AppendLine(
                $"            PlayRandomAudio(AudioTrackType.{trackType}, audioId.ToId(), source, fade, delay, speed);");
            sb.AppendLine();
            sb.AppendLine(
                $"        public void StopAudio({enumName} audioId, bool fade = false, float delay = 0.0f) =>");
            sb.AppendLine($"            StopAudio(AudioTrackType.{trackType}, audioId.ToId(), fade, delay);");
            sb.AppendLine();
            sb.AppendLine($"        public void PauseAudio({enumName} audioId, float delay = 0.0f) =>");
            sb.AppendLine($"            PauseAudio(AudioTrackType.{trackType}, audioId.ToId(), delay);");
            sb.AppendLine();
            sb.AppendLine($"        public void ResumeAudio({enumName} audioId, float delay = 0.0f) =>");
            sb.AppendLine($"            ResumeAudio(AudioTrackType.{trackType}, audioId.ToId(), delay);");
            sb.AppendLine();
            sb.AppendLine(
                $"        public void RestartAudio({enumName} audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>");
            sb.AppendLine($"            RestartAudio(AudioTrackType.{trackType}, audioId.ToId(), fade, delay, speed);");
            sb.AppendLine();
            sb.AppendLine($"        public bool IsAudioPlaying({enumName} audioId) =>");
            sb.AppendLine($"            IsAudioPlaying(AudioTrackType.{trackType}, audioId.ToId());");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            string filePath = Path.Combine(GeneratedPath, $"{enumName}.cs");
            File.WriteAllText(filePath, sb.ToString());
        }

        private static string SanitizeEnumValue(string value)
        {
            string sanitized = Regex.Replace(value, @"[^a-zA-Z0-9_]", "");

            if (string.IsNullOrEmpty(sanitized))
                return "_Invalid";

            if (char.IsDigit(sanitized[0]))
                sanitized = "_" + sanitized;

            return sanitized;
        }
    }
}