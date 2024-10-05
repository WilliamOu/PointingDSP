using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AtlasPacker : MonoBehaviour
{
    [SerializeField] private int blockSize = 1024;
    [SerializeField] private int atlasSizeInBlocks = 16;

    private string textureFolder = "Textures";
    private string atlasFolder = "Atlas";
    private int atlasSize;
    private Texture2D atlas;

    public void StitchAtlas()
    {
        InitializeAtlas();
        ProcessAndPackTextures();
        SaveAtlas();
    }

    private void InitializeAtlas()
    {
        atlasSize = blockSize * atlasSizeInBlocks;
        atlas = new Texture2D(atlasSize, atlasSize, TextureFormat.ARGB32, false);
        Debug.Log("Atlas initialized with size: " + atlasSize + "x" + atlasSize);
    }

    private void ProcessAndPackTextures()
    {
        string mapPath = Path.Combine(Application.persistentDataPath, "Maps", PersistentDataManager.Instance.Map, textureFolder);
        var files = Directory.GetFiles(mapPath);

        int blockIndex = 0;

        // Define the textures to pack first
        HashSet<string> requiredTextures = new HashSet<string>
        {
            Path.Combine(mapPath, "Asphalt_STANDARD.png"),
            Path.Combine(mapPath, "Brick_STANDARD.jpg"),
            Path.Combine(mapPath, "SolidBlack_STANDARD.jpg")
        };

        // Manually pack the required textures
        foreach (var requiredTexture in requiredTextures)
        {
            if (File.Exists(requiredTexture))
            {
                Texture2D texture = LoadTexture(requiredTexture);
                if (texture != null)
                {
                    Texture2D normalizedTexture = NormalizeTexture(texture);

                    int xBlock = blockIndex % atlasSizeInBlocks;
                    int yBlock = atlasSizeInBlocks - 1 - (blockIndex / atlasSizeInBlocks);

                    PlaceTextureInAtlas(normalizedTexture, xBlock * blockSize, yBlock * blockSize);
                    blockIndex++;
                }
            }
        }

        // Pack the remaining textures, skipping the required ones
        foreach (var file in files)
        {
            if (!requiredTextures.Contains(file) && IsImageFile(file))
            {
                Texture2D texture = LoadTexture(file);
                if (texture != null)
                {
                    Texture2D normalizedTexture = NormalizeTexture(texture);

                    int xBlock = blockIndex % atlasSizeInBlocks;
                    int yBlock = atlasSizeInBlocks - 1 - (blockIndex / atlasSizeInBlocks);

                    PlaceTextureInAtlas(normalizedTexture, xBlock * blockSize, yBlock * blockSize);
                    blockIndex++;
                }
            }
        }

        atlas.Apply();
        Debug.Log("Atlas Packer: " + blockIndex + " textures processed and packed.");
    }

    private bool IsImageFile(string file)
    {
        string extension = Path.GetExtension(file).ToLower();
        return extension == ".jpg" || extension == ".jpeg" || extension == ".png";
    }

    private Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileData))
        {
            return texture;
        }
        Debug.LogWarning("Atlas Packer: Failed to load texture from " + filePath);
        return null;
    }

    private Texture2D NormalizeTexture(Texture2D texture)
    {
        Texture2D resizedTexture = new Texture2D(blockSize, blockSize, TextureFormat.ARGB32, false);
        ResizeTexture(texture, resizedTexture);
        return resizedTexture;
    }

    private void ResizeTexture(Texture2D source, Texture2D destination)
    {
        RenderTexture rt = RenderTexture.GetTemporary(destination.width, destination.height);
        Graphics.Blit(source, rt);
        RenderTexture.active = rt;
        destination.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        destination.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
    }

    private void PlaceTextureInAtlas(Texture2D texture, int startX, int startY)
    {
        if (atlas == null)
        {
            Debug.LogError("Atlas Packer: Atlas texture is null when placing texture in atlas.");
            return;
        }

        if (texture == null)
        {
            Debug.LogError("Atlas Packer: Texture is null when placing in atlas.");
            return;
        }

        Color[] pixels = texture.GetPixels();
        if (pixels == null || pixels.Length == 0)
        {
            Debug.LogError("Atlas Packer: Pixels array is null or empty when placing texture in atlas.");
            return;
        }

        atlas.SetPixels(startX, startY, blockSize, blockSize, pixels);
    }

    private void SaveAtlas()
    {
        string atlasPath = Path.Combine(Application.persistentDataPath, "Maps", PersistentDataManager.Instance.Map, atlasFolder);
        byte[] bytes = atlas.EncodeToPNG();
        try
        {
            File.WriteAllBytes(Path.Combine(atlasPath, "Packed_Atlas.png"), bytes);
            Debug.Log("Atlas Packer: Atlas file successfully saved. " + bytes.Length/1000000 + " megabytes.");
        }
        catch
        {
            Debug.Log("Atlas Packer: Couldn't save atlas to file.");
        }
    }
}
