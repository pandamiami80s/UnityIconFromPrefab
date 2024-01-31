using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 2024 01 30
/// 
/// Generate icon from prefab using Screenshot
///     * Advantages:
///         * High resolution
///         * Background can be any color
///         * Background is transparent 
///     * Disadvantages:
///         * Not scale safe (Use class for scale offset)
/// </summary>

public class IconFromPrefabAdvanced : MonoBehaviour
{
    [Header("Directory")]
    [SerializeField] string folderName = "Generated Icons";
    string currentFolder;

    [Header("File")]
    [SerializeField] string fileName = "Advanced Icon";
    [SerializeField]  enum FileFormat
    {
        //EXR (Texture2D::EncodeToEXR needs an uncompressed HDR texture format)
        jpg,
        png,
        tga
    }
    [SerializeField] FileFormat fileFormat;

    [Header("Prefab")]
    [SerializeField] Transform prefabPosition;
    [System.Serializable] class PrefabInfo
    {
        public GameObject prefab;
        public float prefabOffsetY;
    }
    [SerializeField] List<PrefabInfo> prefabInfo = new List<PrefabInfo>();

    [Header("Screenshot")]
    [SerializeField] Vector2Int iconSize = new Vector2Int(256, 256);
    // Quallity
    [SerializeField] int superSize = 1;
    
    [Header("Background")]
    [SerializeField] Color backgroundColor = new Color();
    enum BackgroundType
    {
        solid,
        transparent
    }
    [SerializeField] BackgroundType backgroundType;
    


    void Start()
    {
        // Directory
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath);
        currentFolder = string.Format("{0}/{1}", directoryInfo.Parent.ToString(), folderName);
        if (Directory.Exists(currentFolder) == false)
        {
            Directory.CreateDirectory(currentFolder);
        }

        GenerateIcons();
    }
    
    void GenerateIcons()
    {
        // Exclude TRENSPARENT JPG case
        if (backgroundType == BackgroundType.transparent && fileFormat == FileFormat.jpg)
        {
            Debug.Log("Transparent JPG is not possible");

            return;
        }

        // Solid/Transparent icon
        if (backgroundType == BackgroundType.solid)
        {
            Camera.main.clearFlags = CameraClearFlags.Color;
            Camera.main.backgroundColor = backgroundColor;
        }
        else if (backgroundType == BackgroundType.transparent)
        {
            Camera.main.clearFlags = CameraClearFlags.Depth;
        }

        StartCoroutine(GenerateIconCoroutine());
    }

    IEnumerator GenerateIconCoroutine()
    {
        for (int i = 0; i < prefabInfo.Count; i++)
        {
            // Need to spawn in a front of main camera obviously
            GameObject iconPrefab = Instantiate(
                prefabInfo[i].prefab, 
                new Vector3(prefabPosition.position.x, prefabPosition.position.y, prefabPosition.position.z + prefabInfo[i].prefabOffsetY),
                prefabPosition.rotation,
                prefabPosition);

            // Called once the frame rendering has ended
            yield return new WaitForEndOfFrame();

            Texture2D texture2D = ScreenCapture.CaptureScreenshotAsTexture(superSize);

            // Clear everything including camera view (Prevent other objects from last frame to appear)
            Destroy(iconPrefab);
            Camera.main.clearFlags = CameraClearFlags.Color;
            yield return new WaitForEndOfFrame();
            Camera.main.clearFlags = CameraClearFlags.Depth;

            // Create texture copy (Bug + memory leak)
            Texture2D texture2DCopy = new Texture2D(iconSize.x, iconSize.y);
            // Get certain pixels from texure
            Color[] getPixels = texture2D.GetPixels(
                texture2D.width / 2 - iconSize.x / 2, 
                texture2D.height / 2 - iconSize.y / 2,
                iconSize.x, 
                iconSize.y);
            texture2DCopy.SetPixels(getPixels);
            texture2DCopy.Apply();

            // Set file type
            byte[] bytes = null;
            if (fileFormat == FileFormat.jpg)
            {
                bytes = texture2DCopy.EncodeToJPG();
            }
            else if (fileFormat == FileFormat.png)
            {
                bytes = texture2DCopy.EncodeToPNG();
            }
            else if (fileFormat == FileFormat.tga)
            {
                bytes = texture2DCopy.EncodeToTGA();
            }

            // File name
            string path = string.Format("{0}/{1} {2}.{3}", currentFolder, fileName, prefabInfo[i].prefab.name, fileFormat.ToString());

            // Save
            if (bytes == null)
            {
                yield break;
            }
            File.WriteAllBytes(path, bytes);
        }
    }
}
