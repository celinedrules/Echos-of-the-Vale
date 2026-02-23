
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Serialization;

public class SpritePhysicsShapeGenerator : OdinEditorWindow
{
    // ───────────────────────────── Menu ─────────────────────────────

    [MenuItem("Tools/Sprite Physics Shape Generator")]
    private static void OpenWindow()
    {
        SpritePhysicsShapeGenerator window = GetWindow<SpritePhysicsShapeGenerator>();
        window.titleContent = new GUIContent("Sprite Physics Shapes");
        window.Show();
    }

    // ───────────────────────────── Texture ──────────────────────────

    [FormerlySerializedAs("SourceTexture")]
    [TitleGroup("Texture")]
    [OnValueChanged(nameof(OnTextureChanged))]
    [Required("Drag a spritesheet texture here.")]
    public Texture2D sourceTexture;

    // ───────────────────────────── Settings ─────────────────────────

    [FormerlySerializedAs("AlphaThreshold")]
    [TitleGroup("Settings")]
    [Range(0f, 1f)]
    [Tooltip("Pixels with alpha above this value are considered solid.")]
    [OnValueChanged(nameof(RegeneratePreview))]
    public float alphaThreshold = 0.5f;

    [FormerlySerializedAs("SimplifyTolerance")]
    [TitleGroup("Settings")]
    [Range(0f, 5f)]
    [Tooltip("Higher values produce simpler shapes with fewer vertices.")]
    [OnValueChanged(nameof(RegeneratePreview))]
    public float simplifyTolerance = 0.8f;

    // ───────────────────────────── Ignored Colors ───────────────────

    [FormerlySerializedAs("IgnoredColors")]
    [TitleGroup("Ignored Colors")]
    [InfoBox(
        "Colors in this list will be treated as transparent (ignored for collision). Use the color pickers or the eyedropper below.")]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)]
    [OnValueChanged(nameof(RegeneratePreview))]
    public List<Color> ignoredColors = new List<Color>();

    [FormerlySerializedAs("ColorTolerance")]
    [TitleGroup("Ignored Colors")]
    [Range(0f, 1f)]
    [Tooltip("How closely a pixel must match an ignored color (0 = exact match, 1 = match anything).")]
    [OnValueChanged(nameof(RegeneratePreview))]
    public float colorTolerance = 0.05f;

    [TitleGroup("Ignored Colors")]
    [ShowIf(nameof(HasSprites))]
    [DisplayAsString, HideLabel]
    [ShowInInspector]
    private string _eyedropperHint = "";

    [TitleGroup("Ignored Colors")]
    [ShowIf(nameof(HasSprites))]
    [Button("Enable Eyedropper (click preview to pick color)")]
    [GUIColor(0.6f, 0.85f, 1f)]
    private void ToggleEyedropper()
    {
        _eyedropperActive = !_eyedropperActive;
        _eyedropperHint = _eyedropperActive
            ? "🔍 Eyedropper ACTIVE — click on the preview image to pick a color to ignore."
            : "";
        Repaint();
    }

    private bool _eyedropperActive;

    // ───────────────────────────── Sprite Selection ─────────────────

    [FormerlySerializedAs("SelectedSpriteIndex")]
    [TitleGroup("Sprite Selection")]
    [ValueDropdown(nameof(GetSpriteNames))]
    [OnValueChanged(nameof(RegeneratePreview))]
    [ShowIf(nameof(HasSprites))]
    public int selectedSpriteIndex;

    // ───────────────────────────── Preview ──────────────────────────

    [FormerlySerializedAs("PreviewInfo")]
    [TitleGroup("Preview")]
    [ShowIf(nameof(HasPreview))]
    [HideLabel]
    [DisplayAsString]
    public string previewInfo;

    // ───────────────────────────── Internal State ───────────────────

    private Sprite[] _sprites;
    private List<Vector2> _previewOutline;
    private List<List<Vector2>> _existingOutlines; // Current physics shape from the asset
    private Texture2D _readableTexture;

    // Cached for eyedropper hit-testing
    private Rect _lastPreviewDrawRect;

    private bool HasSprites => _sprites is { Length: > 0 };
    private bool HasPreview => _previewOutline is { Count: >= 3 } && HasSprites;
    private bool HasExistingOutline => _existingOutlines is { Count: > 0 };

    // ───────────────────────────── Dropdown Values ──────────────────

    private IEnumerable<ValueDropdownItem<int>> GetSpriteNames()
    {
        if (_sprites == null)
            yield break;

        for (int i = 0; i < _sprites.Length; i++)
            yield return new ValueDropdownItem<int>(_sprites[i].name, i);
    }

    // ───────────────────────────── Pixel Solidity Check ─────────────

    private bool IsPixelSolid(Color32 pixel)
    {
        if ((pixel.a / 255f) <= alphaThreshold)
            return false;

        if (ignoredColors is { Count: > 0 })
        {
            Color pixelColor = new Color(pixel.r / 255f, pixel.g / 255f, pixel.b / 255f);

            foreach (Color ignored in ignoredColors)
            {
                float dr = pixelColor.r - ignored.r;
                float dg = pixelColor.g - ignored.g;
                float db = pixelColor.b - ignored.b;
                float distance = Mathf.Sqrt(dr * dr + dg * dg + db * db);

                if (distance <= colorTolerance * 1.732f)
                    return false;
            }
        }

        return true;
    }

    // ───────────────────────────── Load Existing Physics Shape ───────

    private void LoadExistingOutlines()
    {
        _existingOutlines = null;

        if (!sourceTexture || _sprites == null || _sprites.Length == 0) return;
        if (selectedSpriteIndex < 0 || selectedSpriteIndex >= _sprites.Length) return;

        string path = AssetDatabase.GetAssetPath(sourceTexture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (!importer) return;

        SpriteDataProviderFactories factory = new();
        factory.Init();

        ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        if (dataProvider == null) return;

        dataProvider.InitSpriteEditorDataProvider();

        ISpritePhysicsOutlineDataProvider physicsProvider =
            dataProvider.GetDataProvider<ISpritePhysicsOutlineDataProvider>();
        if (physicsProvider == null) return;

        SpriteRect[] spriteRects = dataProvider.GetSpriteRects();
        Sprite selectedSprite = _sprites[selectedSpriteIndex];

        foreach (SpriteRect spriteRect in spriteRects)
        {
            if (spriteRect.name != selectedSprite.name) continue;

            List<Vector2[]> outlines = physicsProvider.GetOutlines(spriteRect.spriteID);
            if (outlines != null && outlines.Count > 0)
            {
                _existingOutlines = new List<List<Vector2>>();
                foreach (Vector2[] outline in outlines)
                {
                    if (outline != null && outline.Length >= 3)
                        _existingOutlines.Add(new List<Vector2>(outline));
                }

                if (_existingOutlines.Count == 0)
                    _existingOutlines = null;
            }

            break;
        }
    }

    // ───────────────────────────── Callbacks ────────────────────────

    private void OnTextureChanged()
    {
        _sprites = null;
        _previewOutline = null;
        _existingOutlines = null;
        _readableTexture = null;
        previewInfo = "";
        selectedSpriteIndex = 0;

        if (!sourceTexture)
            return;

        string path = AssetDatabase.GetAssetPath(sourceTexture);
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        _sprites = assets.OfType<Sprite>().OrderBy(s => s.name).ToArray();

        if (_sprites.Length == 0)
        {
            previewInfo = "No sprites found. Is the texture set to Sprite mode?";
            return;
        }

        CreateReadableTexture(path);
        RegeneratePreview();
    }

    private void CreateReadableTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (!importer)
            return;

        if (importer.isReadable)
        {
            _readableTexture = sourceTexture;
            return;
        }

        RenderTexture rt =
            RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(sourceTexture, rt);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        _readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);
        _readableTexture.ReadPixels(new Rect(0, 0, sourceTexture.width, sourceTexture.height), 0, 0);
        _readableTexture.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
    }

    private void RegeneratePreview()
    {
        _previewOutline = null;
        _existingOutlines = null;
        previewInfo = "";

        if (_sprites == null || _sprites.Length == 0 || _readableTexture == null)
            return;

        if (selectedSpriteIndex < 0 || selectedSpriteIndex >= _sprites.Length)
            return;

        // Load the existing physics shape from the asset
        LoadExistingOutlines();

        Sprite sprite = _sprites[selectedSpriteIndex];
        Rect rect = sprite.rect;

        int x0 = Mathf.FloorToInt(rect.x);
        int y0 = Mathf.FloorToInt(rect.y);
        int w = Mathf.FloorToInt(rect.width);
        int h = Mathf.FloorToInt(rect.height);

        Color32[] allPixels = _readableTexture.GetPixels32();
        int texW = _readableTexture.width;

        bool[,] solid = new bool[w, h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color32 c = allPixels[(y0 + y) * texW + (x0 + x)];
                solid[x, y] = IsPixelSolid(c);
            }
        }

        List<Vector2> outline = MarchOutline(solid, w, h);

        if (outline.Count < 3)
        {
            previewInfo = "No outline found (sprite may be fully transparent or all colors ignored).";
            return;
        }

        List<Vector2> simplified = DouglasPeucker(outline, simplifyTolerance);

        if (simplified.Count < 3)
        {
            previewInfo = "Outline too simple after simplification. Try a lower tolerance.";
            return;
        }

        _previewOutline = simplified;

        string ignoredInfo = ignoredColors.Count > 0 ? $"  |  {ignoredColors.Count} color(s) ignored" : "";
        string existingInfo = HasExistingOutline ? "  |  Has existing shape" : "  |  No existing shape";
        previewInfo = $"Sprite: {sprite.name}  |  {simplified.Count} vertices  |  {w}×{h} px{ignoredInfo}{existingInfo}";
        Repaint();
    }

    // ───────────────────────────── Preview Drawing ──────────────────

    [TitleGroup("Preview")]
    [ShowIf(nameof(HasPreview))]
    [OnInspectorGUI]
    private void DrawPreview()
    {
        if (_sprites == null || selectedSpriteIndex < 0 || selectedSpriteIndex >= _sprites.Length)
            return;

        if (_previewOutline == null || _previewOutline.Count < 3)
            return;

        Sprite sprite = _sprites[selectedSpriteIndex];
        Rect spriteRect = sprite.rect;
        int w = Mathf.FloorToInt(spriteRect.width);
        int h = Mathf.FloorToInt(spriteRect.height);

        // ── Draw side-by-side: Before (left) | After (right) ──

        bool hasBefore = HasExistingOutline;
        float totalWidth = position.width - 40f;
        float singlePreviewSize;

        if (hasBefore)
        {
            singlePreviewSize = Mathf.Min((totalWidth - 20f) * 0.5f, 250f);
        }
        else
        {
            singlePreviewSize = Mathf.Min(totalWidth, 300f);
        }

        // Labels
        if (hasBefore)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
            GUILayout.Label("Before (Existing)", labelStyle, GUILayout.Width(singlePreviewSize));
            GUILayout.Space(10);
            GUILayout.Label("After (New)", labelStyle, GUILayout.Width(singlePreviewSize));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
            GUILayout.Label("Preview (No existing shape)", labelStyle, GUILayout.Width(singlePreviewSize));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        // Reserve area for both previews
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (hasBefore)
        {
            // ── Before panel ──
            Rect beforeArea = GUILayoutUtility.GetRect(singlePreviewSize, singlePreviewSize, GUILayout.ExpandWidth(false));
            DrawSpritePanel(beforeArea, spriteRect, w, h, _existingOutlines, Color.red, new Color(1f, 0.5f, 0.5f), true);

            GUILayout.Space(10);
        }

        // ── After panel ──
        Rect afterArea = GUILayoutUtility.GetRect(singlePreviewSize, singlePreviewSize, GUILayout.ExpandWidth(false));
        _lastPreviewDrawRect = DrawSpritePanel(afterArea, spriteRect, w, h,
            new List<List<Vector2>> { _previewOutline }, Color.green, Color.cyan, false);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // Eyedropper: handle click on the After preview
        if (_eyedropperActive && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            Vector2 mousePos = Event.current.mousePosition;
            if (_lastPreviewDrawRect.Contains(mousePos))
            {
                const float padding = 10f;
                float drawW = afterArea.width - padding * 2;
                float drawH = afterArea.height - padding * 2;
                float scale = Mathf.Min(drawW / w, drawH / h);
                float offsetX = afterArea.x + padding + (drawW - w * scale) * 0.5f;
                float offsetY = afterArea.y + padding + (drawH - h * scale) * 0.5f;

                float pixelX = (mousePos.x - offsetX) / scale;
                float pixelY = h - (mousePos.y - offsetY) / scale;

                int px = Mathf.FloorToInt(pixelX);
                int py = Mathf.FloorToInt(pixelY);

                if (px >= 0 && px < w && py >= 0 && py < h && _readableTexture != null)
                {
                    int texX = Mathf.FloorToInt(spriteRect.x) + px;
                    int texY = Mathf.FloorToInt(spriteRect.y) + py;

                    Color pickedColor = _readableTexture.GetPixel(texX, texY);
                    Color rgbOnly = new Color(pickedColor.r, pickedColor.g, pickedColor.b, 1f);

                    bool alreadyExists = false;

                    foreach (Color existing in ignoredColors)
                    {
                        float dr = rgbOnly.r - existing.r;
                        float dg = rgbOnly.g - existing.g;
                        float db = rgbOnly.b - existing.b;

                        if (!(Mathf.Sqrt(dr * dr + dg * dg + db * db) < 0.01f))
                            continue;

                        alreadyExists = true;
                        break;
                    }

                    if (!alreadyExists)
                    {
                        ignoredColors.Add(rgbOnly);
                        Debug.Log(
                            $"[Eyedropper] Picked color: {rgbOnly} (R:{rgbOnly.r:F2} G:{rgbOnly.g:F2} B:{rgbOnly.b:F2})");
                        RegeneratePreview();
                    }
                    else
                    {
                        Debug.Log($"[Eyedropper] Color already in ignored list: {rgbOnly}");
                    }

                    _eyedropperActive = false;
                    _eyedropperHint = "";
                }

                Event.current.Use();
            }
        }

        if (_eyedropperActive)
            EditorGUIUtility.AddCursorRect(_lastPreviewDrawRect, MouseCursor.Zoom);
    }

    /// <summary>
    /// Draws a sprite preview panel with outline(s) overlaid. Returns the draw rect used for the sprite image.
    /// For the "before" panel, outlines are in sprite-local space (centered at 0,0).
    /// For the "after" panel, outlines are in pixel space (0..w, 0..h).
    /// </summary>
    private Rect DrawSpritePanel(Rect area, Rect spriteRect, int w, int h,
        List<List<Vector2>> outlines, Color lineColor, Color vertexColor, bool outlinesAreCentered)
    {
        EditorGUI.DrawRect(area, new Color(0.15f, 0.15f, 0.15f, 1f));

        const float padding = 10f;
        float drawW = area.width - padding * 2;
        float drawH = area.height - padding * 2;
        float scale = Mathf.Min(drawW / w, drawH / h);

        float offsetX = area.x + padding + (drawW - w * scale) * 0.5f;
        float offsetY = area.y + padding + (drawH - h * scale) * 0.5f;

        Rect texCoords = new Rect(
            spriteRect.x / sourceTexture.width,
            spriteRect.y / sourceTexture.height,
            spriteRect.width / sourceTexture.width,
            spriteRect.height / sourceTexture.height
        );

        Rect drawRect = new Rect(offsetX, offsetY, w * scale, h * scale);
        GUI.DrawTextureWithTexCoords(drawRect, sourceTexture, texCoords);

        if (outlines != null && outlines.Count > 0)
        {
            Handles.BeginGUI();

            Vector2 center = new Vector2(w * 0.5f, h * 0.5f);

            foreach (List<Vector2> outline in outlines)
            {
                if (outline == null || outline.Count < 3) continue;

                // Draw edges
                Handles.color = lineColor;
                for (int i = 0; i < outline.Count; i++)
                {
                    Vector2 a = outline[i];
                    Vector2 b = outline[(i + 1) % outline.Count];

                    // If centered (existing outlines from ISpritePhysicsOutlineDataProvider),
                    // convert back to pixel-space (0..w, 0..h)
                    if (outlinesAreCentered)
                    {
                        a += center;
                        b += center;
                    }

                    Vector2 screenA = new Vector2(offsetX + a.x * scale, offsetY + (h - a.y) * scale);
                    Vector2 screenB = new Vector2(offsetX + b.x * scale, offsetY + (h - b.y) * scale);

                    Handles.DrawLine(new Vector3(screenA.x, screenA.y, 0), new Vector3(screenB.x, screenB.y, 0));
                }

                // Draw vertices
                Handles.color = vertexColor;
                for (int i = 0; i < outline.Count; i++)
                {
                    Vector2 p = outline[i];
                    if (outlinesAreCentered)
                        p += center;

                    Vector2 screenP = new Vector2(offsetX + p.x * scale, offsetY + (h - p.y) * scale);
                    Handles.DrawSolidDisc(new Vector3(screenP.x, screenP.y, 0), Vector3.forward, 2.5f);
                }
            }

            Handles.EndGUI();
        }

        return drawRect;
    }

    // ───────────────────────────── Actions ──────────────────────────

    [TitleGroup("Actions")]
    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0.4f)]
    [ShowIf(nameof(HasSprites))]
    [Tooltip("Generate and apply physics shapes for ALL sprites in this texture.")]
    private void ApplyToAllSprites()
    {
        if (!sourceTexture || _sprites == null || _sprites.Length == 0)
            return;

        string path = AssetDatabase.GetAssetPath(sourceTexture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (!importer)
        {
            Debug.LogError("[SpritePhysicsShapeGenerator] Could not get TextureImporter.");
            return;
        }

        int count = ApplyPhysicsShapes(importer);

        if (count > 0)
        {
            Debug.Log(
                $"[SpritePhysicsShapeGenerator] Applied physics shapes to {count}/{_sprites.Length} sprites in: {path}");
            EditorUtility.DisplayDialog("Sprite Physics Shapes",
                $"Successfully applied physics shapes to {count} sprite(s).", "OK");
            RegeneratePreview(); // Refresh so "before" shows the newly applied shape
        }
        else
        {
            Debug.LogWarning("[SpritePhysicsShapeGenerator] No sprites were processed.");
            EditorUtility.DisplayDialog("Sprite Physics Shapes",
                "No sprites were processed. Check that sprites have visible pixels.", "OK");
        }
    }

    [TitleGroup("Actions")]
    [Button(ButtonSizes.Medium)]
    [ShowIf(nameof(HasPreview))]
    [Tooltip("Generate and apply physics shape for ONLY the selected sprite.")]
    private void ApplyToSelectedSpriteOnly()
    {
        if (!sourceTexture || _sprites == null)
            return;

        if (selectedSpriteIndex < 0 || selectedSpriteIndex >= _sprites.Length)
            return;

        string path = AssetDatabase.GetAssetPath(sourceTexture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (!importer)
            return;

        Sprite targetSprite = _sprites[selectedSpriteIndex];
        int count = ApplyPhysicsShapes(importer, targetSprite.name);

        if (count > 0)
        {
            Debug.Log($"[SpritePhysicsShapeGenerator] Applied physics shape to: {targetSprite.name}");
            RegeneratePreview();
        }
    }

    // ───────────────────────────── Remove Actions ───────────────────

    [TitleGroup("Remove Physics Shapes")]
    [Button(ButtonSizes.Medium), GUIColor(1f, 0.5f, 0.5f)]
    [ShowIf(nameof(HasSprites))]
    [Tooltip("Remove custom physics shapes from ALL sprites in this texture.")]
    private void RemoveAllPhysicsShapes()
    {
        if (!sourceTexture || _sprites == null || _sprites.Length == 0) return;

        if (!EditorUtility.DisplayDialog("Remove All Physics Shapes",
                "Are you sure you want to remove custom physics shapes from ALL sprites in this texture?",
                "Remove All", "Cancel"))
            return;

        string path = AssetDatabase.GetAssetPath(sourceTexture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (!importer) return;

        int count = RemovePhysicsShapes(importer);

        if (count > 0)
        {
            Debug.Log($"[SpritePhysicsShapeGenerator] Removed physics shapes from {count} sprite(s) in: {path}");
            EditorUtility.DisplayDialog("Sprite Physics Shapes",
                $"Removed physics shapes from {count} sprite(s).", "OK");
            RegeneratePreview();
        }
        else
        {
            Debug.Log("[SpritePhysicsShapeGenerator] No physics shapes to remove.");
        }
    }

    [TitleGroup("Remove Physics Shapes")]
    [Button(ButtonSizes.Medium), GUIColor(1f, 0.7f, 0.5f)]
    [ShowIf(nameof(HasExistingOutline))]
    [Tooltip("Remove custom physics shape from ONLY the selected sprite.")]
    private void RemoveSelectedSpritePhysicsShape()
    {
        if (!sourceTexture || _sprites == null) return;
        if (selectedSpriteIndex < 0 || selectedSpriteIndex >= _sprites.Length) return;

        Sprite targetSprite = _sprites[selectedSpriteIndex];

        string path = AssetDatabase.GetAssetPath(sourceTexture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (!importer) return;

        int count = RemovePhysicsShapes(importer, targetSprite.name);

        if (count > 0)
        {
            Debug.Log($"[SpritePhysicsShapeGenerator] Removed physics shape from: {targetSprite.name}");
            RegeneratePreview();
        }
    }

    // ───────────────────────────── Apply Logic ──────────────────────

    private int ApplyPhysicsShapes(TextureImporter importer, string onlySpriteName = null)
    {
        SpriteDataProviderFactories factory = new();
        factory.Init();

        ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);

        if (dataProvider == null)
            return 0;

        dataProvider.InitSpriteEditorDataProvider();

        ISpritePhysicsOutlineDataProvider physicsProvider =
            dataProvider.GetDataProvider<ISpritePhysicsOutlineDataProvider>();

        if (physicsProvider == null)
            return 0;

        SpriteRect[] spriteRects = dataProvider.GetSpriteRects();

        string assetPath = importer.assetPath;
        bool wasReadable = importer.isReadable;

        if (!wasReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        Color32[] allPixels = texture.GetPixels32();
        int texW = texture.width;

        int processed = 0;

        foreach (SpriteRect spriteRect in spriteRects)
        {
            if (onlySpriteName != null && spriteRect.name != onlySpriteName)
                continue;

            Rect rect = spriteRect.rect;
            int x0 = Mathf.FloorToInt(rect.x);
            int y0 = Mathf.FloorToInt(rect.y);
            int w = Mathf.FloorToInt(rect.width);
            int h = Mathf.FloorToInt(rect.height);

            bool[,] solid = new bool[w, h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color32 c = allPixels[(y0 + y) * texW + (x0 + x)];
                    solid[x, y] = IsPixelSolid(c);
                }
            }

            List<Vector2> outline = MarchOutline(solid, w, h);

            if (outline.Count < 3)
                continue;

            List<Vector2> simplified = DouglasPeucker(outline, simplifyTolerance);

            if (simplified.Count < 3)
                continue;

            Vector2 center = new(w * 0.5f, h * 0.5f);
            List<Vector2> finalOutline = new(simplified.Count);

            for (int i = 0; i < simplified.Count; i++)
                finalOutline.Add(simplified[i] - center);

            physicsProvider.SetOutlines(spriteRect.spriteID, new List<Vector2[]> { finalOutline.ToArray() });
            processed++;
        }

        dataProvider.Apply();

        if (!wasReadable)
            importer.isReadable = false;

        importer.SaveAndReimport();

        CreateReadableTexture(assetPath);

        return processed;
    }

    // ───────────────────────────── Remove Logic ─────────────────────

    private int RemovePhysicsShapes(TextureImporter importer, string onlySpriteName = null)
    {
        SpriteDataProviderFactories factory = new();
        factory.Init();

        ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        if (dataProvider == null) return 0;

        dataProvider.InitSpriteEditorDataProvider();

        ISpritePhysicsOutlineDataProvider physicsProvider =
            dataProvider.GetDataProvider<ISpritePhysicsOutlineDataProvider>();
        if (physicsProvider == null) return 0;

        SpriteRect[] spriteRects = dataProvider.GetSpriteRects();
        int removed = 0;

        foreach (SpriteRect spriteRect in spriteRects)
        {
            if (onlySpriteName != null && spriteRect.name != onlySpriteName)
                continue;

            // Check if there's an existing outline to remove
            List<Vector2[]> existing = physicsProvider.GetOutlines(spriteRect.spriteID);
            if (existing == null || existing.Count == 0)
                continue;

            // Set to empty list to clear the custom physics shape
            physicsProvider.SetOutlines(spriteRect.spriteID, new List<Vector2[]>());
            removed++;
        }

        dataProvider.Apply();
        importer.SaveAndReimport();

        string assetPath = importer.assetPath;
        CreateReadableTexture(assetPath);

        return removed;
    }

    // ───────────────────────────── Outline Tracing ──────────────────

    private static List<Vector2> MarchOutline(bool[,] solid, int w, int h)
    {
        List<(Vector2 from, Vector2 to)> edges = new();

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (!solid[x, y])
                    continue;

                if (y == 0 || !solid[x, y - 1])
                    edges.Add((new Vector2(x, y), new Vector2(x + 1, y)));

                if (y == h - 1 || !solid[x, y + 1])
                    edges.Add((new Vector2(x + 1, y + 1), new Vector2(x, y + 1)));

                if (x == 0 || !solid[x - 1, y])
                    edges.Add((new Vector2(x, y + 1), new Vector2(x, y)));

                if (x == w - 1 || !solid[x + 1, y])
                    edges.Add((new Vector2(x + 1, y), new Vector2(x + 1, y + 1)));
            }
        }

        if (edges.Count == 0)
            return new List<Vector2>();

        Dictionary<Vector2, List<int>> fromMap = new();

        for (int i = 0; i < edges.Count; i++)
        {
            if (!fromMap.ContainsKey(edges[i].from))
                fromMap[edges[i].from] = new List<int>();

            fromMap[edges[i].from].Add(i);
        }

        bool[] used = new bool[edges.Count];
        List<Vector2> outline = new();
        used[0] = true;
        outline.Add(edges[0].from);
        Vector2 next = edges[0].to;

        int safety = edges.Count + 1;

        while (safety-- > 0)
        {
            outline.Add(next);

            if (next == outline[0] && outline.Count > 2)
                break;

            if (!fromMap.TryGetValue(next, out List<int> candidates))
                break;

            bool found = false;

            foreach (int idx in candidates)
            {
                if (used[idx])
                    continue;

                used[idx] = true;
                next = edges[idx].to;
                found = true;
                break;
            }

            if (!found)
                break;
        }

        if (outline.Count > 1 && outline[0] == outline[^1])
            outline.RemoveAt(outline.Count - 1);

        return outline;
    }

    // ───────────────────────────── Simplification ───────────────────

    private static List<Vector2> DouglasPeucker(List<Vector2> points, float epsilon)
    {
        if (points.Count < 3)
            return new List<Vector2>(points);

        float maxDist = 0;
        int maxIdx = 0;
        Vector2 first = points[0];
        Vector2 last = points[^1];

        for (int i = 1; i < points.Count - 1; i++)
        {
            float dist = PerpendicularDistance(points[i], first, last);

            if (dist > maxDist)
            {
                maxDist = dist;
                maxIdx = i;
            }
        }

        if (!(maxDist > epsilon))
            return new List<Vector2>
                { first, last };

        List<Vector2> left = DouglasPeucker(points.GetRange(0, maxIdx + 1), epsilon);
        List<Vector2> right = DouglasPeucker(points.GetRange(maxIdx, points.Count - maxIdx), epsilon);

        left.RemoveAt(left.Count - 1);
        left.AddRange(right);
        return left;
    }

    private static float PerpendicularDistance(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 line = lineEnd - lineStart; 
        float len = line.magnitude;

        if (len < 0.0001f)
            return Vector2.Distance(point, lineStart);

        return Mathf.Abs((point.x - lineStart.x) * (lineEnd.y - lineStart.y) -
                         (point.y - lineStart.y) * (lineEnd.x - lineStart.x)) / len;
    }
}