// Done
using System.IO;
using UnityEngine;
using Utilities;

namespace Managers
{
    public class ScreenshotManager : Singleton<ScreenshotManager>
    {
        private const string ScreenshotPrefix = "savescreenshot_";
        
        [SerializeField] private Camera mainCamera;
        
        private Texture2D _cachedScreenshot;

        public void CaptureScreenshot()
        {
            if (!mainCamera)
                mainCamera = Camera.main;

            // Create a RenderTexture
            int width = Screen.width;
            int height = Screen.height;
            RenderTexture rt = new(width, height, 24);
            
            // Store original settings
            int originalCullingMask = mainCamera.cullingMask;
            RenderTexture originalTarget = mainCamera.targetTexture;
            
            // Exclude UI layer and render
            mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
            mainCamera.targetTexture = rt;
            mainCamera.Render();
            
            // Restore settings
            mainCamera.cullingMask = originalCullingMask;
            mainCamera.targetTexture = originalTarget;
            
            // Read pixels from RenderTexture
            RenderTexture.active = rt;
            _cachedScreenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
            _cachedScreenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            _cachedScreenshot.Apply();
            
            // Cleanup
            RenderTexture.active = null;
            rt.Release();
            Destroy(rt);
        }

        public void SaveScreenshot(int slotIndex)
        {
            if (_cachedScreenshot == null)
                return;

            string path = GetScreenshotPath(slotIndex);
            byte[] bytes = _cachedScreenshot.EncodeToJPG(75); // 75% quality
            File.WriteAllBytes(path, bytes);
        }

        public Sprite LoadScreenshot(int slotIndex)
        {
            string path = GetScreenshotPath(slotIndex);

            if (!File.Exists(path))
                return null;

            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        }

        public void DeleteScreenshot(int slotIndex)
        {
            string path = GetScreenshotPath(slotIndex);
            if (File.Exists(path))
                File.Delete(path);
        }

        private string GetScreenshotPath(int slotIndex)
        {
            return Path.Combine(Application.persistentDataPath, $"{ScreenshotPrefix}{slotIndex}.jpg");
        }
    }
}