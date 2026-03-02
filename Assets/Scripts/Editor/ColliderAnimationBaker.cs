using UnityEngine;
using UnityEditor;

namespace Editor
{
    public class ColliderAnimationBaker : EditorWindow
    {
        private Animator _animator;
        private BoxCollider2D _collider;

        [MenuItem("Tools/Bake BoxCollider2D to Animation Clip")]
        public static void ShowWindow()
        {
            ColliderAnimationBaker window = GetWindow<ColliderAnimationBaker>("Collider Baker");
            window.minSize = new Vector2(350, 140);
            window.maxSize = new Vector2(500, 140);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Bake BoxCollider2D to Animation Clips", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            _animator = (Animator)EditorGUILayout.ObjectField("Animator", _animator, typeof(Animator), true);
            _collider = (BoxCollider2D)EditorGUILayout.ObjectField("BoxCollider2D", _collider, typeof(BoxCollider2D), true);

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.enabled = _animator && _collider;
            if (GUILayout.Button("Apply", GUILayout.Width(80)))
            {
                Bake(_animator, _collider);
            }
            GUI.enabled = true;

            if (GUILayout.Button("Cancel", GUILayout.Width(80)))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void Bake(Animator animator, BoxCollider2D collider)
        {
            GameObject go = animator.gameObject;
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();

            if (!sr)
            {
                Debug.LogError("The Animator's GameObject needs a SpriteRenderer.");
                return;
            }

            string colliderPath = GetRelativePath(go.transform, collider.transform);

            if (colliderPath == null)
            {
                Debug.LogError("BoxCollider2D must be on the Animator's GameObject or one of its children.");
                return;
            }

            RuntimeAnimatorController controller = animator.runtimeAnimatorController;

            if (!controller)
            {
                Debug.LogError("Animator has no RuntimeAnimatorController assigned.");
                return;
            }

            foreach (AnimationClip clip in controller.animationClips)
            {
                BakeClip(clip, go, sr, colliderPath);
            }

            Debug.Log($"Done! Collider keyframes baked using path: \"{colliderPath}\"");
        }

        private static string GetRelativePath(Transform root, Transform child)
        {
            if (child == root)
                return "";

            string path = child.name;
            Transform current = child.parent;

            while (current && current != root)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return current == root ? path : null;
        }

        // ... existing code ...
        private static void BakeClip(AnimationClip clip, GameObject go, SpriteRenderer sr, string colliderPath)
        {
            float[] spriteTimes = GetSpriteKeyframeTimes(clip);

            if (spriteTimes == null || spriteTimes.Length == 0)
            {
                Debug.LogWarning($"No sprite keyframes found in clip '{clip.name}', skipping.");
                return;
            }

            int count = spriteTimes.Length;

            Keyframe[] sizeXKeys = new Keyframe[count];
            Keyframe[] sizeYKeys = new Keyframe[count];
            Keyframe[] offsetXKeys = new Keyframe[count];
            Keyframe[] offsetYKeys = new Keyframe[count];

            for (int i = 0; i < count; i++)
            {
                float time = spriteTimes[i];
                clip.SampleAnimation(go, time);

                Sprite sprite = sr.sprite;

                if (!sprite)
                    continue;

                Rect pixelRect = GetVisiblePixelRect(sprite);

                float ppu = sprite.pixelsPerUnit;
                Vector2 pivot = sprite.pivot;

                float minX = (pixelRect.xMin - pivot.x) / ppu;
                float maxX = (pixelRect.xMax - pivot.x) / ppu;
                float minY = (pixelRect.yMin - pivot.y) / ppu;
                float maxY = (pixelRect.yMax - pivot.y) / ppu;

                Vector2 size = new(maxX - minX, maxY - minY);
                Vector2 offset = new((minX + maxX) / 2f, (minY + maxY) / 2f);

                sizeXKeys[i] = new Keyframe(time, size.x) { weightedMode = WeightedMode.None };
                sizeYKeys[i] = new Keyframe(time, size.y) { weightedMode = WeightedMode.None };
                offsetXKeys[i] = new Keyframe(time, offset.x) { weightedMode = WeightedMode.None };
                offsetYKeys[i] = new Keyframe(time, offset.y) { weightedMode = WeightedMode.None };
            }

            AnimationCurve sizeXCurve = new(sizeXKeys);
            AnimationCurve sizeYCurve = new(sizeYKeys);
            AnimationCurve offsetXCurve = new(offsetXKeys);
            AnimationCurve offsetYCurve = new(offsetYKeys);

            SetConstantTangents(sizeXCurve);
            SetConstantTangents(sizeYCurve);
            SetConstantTangents(offsetXCurve);
            SetConstantTangents(offsetYCurve);

            // Use SetEditorCurve to set only the specific bindings without removing other collider curves
            EditorCurveBinding sizeXBinding = EditorCurveBinding.FloatCurve(colliderPath, typeof(BoxCollider2D), "m_Size.x");
            EditorCurveBinding sizeYBinding = EditorCurveBinding.FloatCurve(colliderPath, typeof(BoxCollider2D), "m_Size.y");
            EditorCurveBinding offsetXBinding = EditorCurveBinding.FloatCurve(colliderPath, typeof(BoxCollider2D), "m_Offset.x");
            EditorCurveBinding offsetYBinding = EditorCurveBinding.FloatCurve(colliderPath, typeof(BoxCollider2D), "m_Offset.y");

            AnimationUtility.SetEditorCurve(clip, sizeXBinding, sizeXCurve);
            AnimationUtility.SetEditorCurve(clip, sizeYBinding, sizeYCurve);
            AnimationUtility.SetEditorCurve(clip, offsetXBinding, offsetXCurve);
            AnimationUtility.SetEditorCurve(clip, offsetYBinding, offsetYCurve);

            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
        }
// ... existing code ...

        private static float[] GetSpriteKeyframeTimes(AnimationClip clip)
        {
            EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);

            foreach (EditorCurveBinding binding in bindings)
            {
                if (binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite")
                {
                    ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                    float[] times = new float[keyframes.Length];

                    for (int i = 0; i < keyframes.Length; i++)
                        times[i] = keyframes[i].time;

                    return times;
                }
            }

            return null;
        }

        private static Rect GetVisiblePixelRect(Sprite sprite)
        {
            Texture2D texture = sprite.texture;
            Rect spriteRect = sprite.rect;

            int xStart = Mathf.FloorToInt(spriteRect.x);
            int yStart = Mathf.FloorToInt(spriteRect.y);
            int width = Mathf.FloorToInt(spriteRect.width);
            int height = Mathf.FloorToInt(spriteRect.height);

            Color32[] pixels = texture.GetPixels32();
            int texW = texture.width;

            int minX = width, minY = height, maxX = 0, maxY = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = (yStart + y) * texW + (xStart + x);

                    if (pixels[idx].a > 0)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (maxX < minX)
                return new Rect(0, 0, 0, 0);

            return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        private static void SetConstantTangents(AnimationCurve curve)
        {
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
            }
        }
    }
}