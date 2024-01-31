using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 2024 01 30
/// 
/// Generate icon from prefab using AssetPreview
///     * Advantages
///         * Fast
///         * Scale safe
///     * Disadvantages:
///         * Background is default color (Possible to replace default color)
///         * Background can not be transparent (Possible to replace default color)
/// </summary>

public class IconFromPrefabSimple : MonoBehaviour
{
    [Header("Directory")]
    [SerializeField] string folderName = "Generated Icons";
    string currentFolder;

    [Header("File")]
    [SerializeField] string fileName = "Simple Icon";
    [SerializeField] enum FileFormat
    {
        //exr (Texture2D::EncodeToEXR needs an uncompressed HDR texture format)
        jpg,
        png,
        tga
    }
    [SerializeField] FileFormat fileFormat;

    [Header("Prefab")]
    [SerializeField] List<GameObject> prefab;

    [Header("File background color")]
    // AssetPreview default background color is RGBA(82, 82, 82, 255)
    [SerializeField] Color32 assetPreviewBackgroundColor = new Color32(82, 82, 82, 255);
    [SerializeField] Color iconBackgroundColor = new Color();



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
        for (int i = 0; i < prefab.Count; i++)
        {
            // Use AssetPreview to get texture (Comes with default background color)
            Texture2D texture2D = AssetPreview.GetAssetPreview(prefab[i]);

            // Create texture copy (Bug or memory leak)
            Texture2D texture2DCopy = new Texture2D(texture2D.width, texture2D.height);
            texture2DCopy.SetPixels(texture2D.GetPixels());
            texture2DCopy.Apply();

            // Change background color
            for (int y = 0; y < texture2D.height; y++)
            {
                for (int x = 0; x < texture2D.width; x++)
                {
                    if (texture2DCopy.GetPixel(x, y) == assetPreviewBackgroundColor)
                    {
                        texture2DCopy.SetPixel(x, y, iconBackgroundColor);
                    }
                }
            }
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
            else if(fileFormat == FileFormat.tga)
            {
                bytes = texture2DCopy.EncodeToTGA();
            }

            // File name
            string path = string.Format("{0}/{1} {2}.{3}", currentFolder, fileName, prefab[i].name, fileFormat.ToString());

            // Save
            if (bytes == null)
            {
                return;
            }
            File.WriteAllBytes(path, bytes);
        }
    }
}