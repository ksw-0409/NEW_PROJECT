#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace NimbleGames.BackgroundAssets
{
    public class CreatePrefabsFromSpritesheet : EditorWindow
    {
        [MenuItem("2DAssetsAndTilesets/Create Prefabs From Spritesheet")]
        static void CreatePrefabs()
        {
            // Get the selected texture in the project window
            Texture2D texture = Selection.activeObject as Texture2D;

            if (texture == null)
            {
                Debug.LogError("Please select a texture in the project window.");
                return;
            }

            // Create a folder to hold the prefabs
            string folderName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(texture));
            string folderPath = Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(texture)), folderName);

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(AssetDatabase.GetAssetPath(texture)), folderName);
            }

            // Get all the sprites in the texture
            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture));

            foreach (Object sprite in sprites)
            {
                if (sprite is Sprite)
                {
                    // Create a new game object with the sprite's name
                    GameObject newPrefab = new GameObject(sprite.name);

                    // Add a sprite renderer component to the new game object
                    SpriteRenderer renderer = newPrefab.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite as Sprite;

                    // Create a prefab from the new game object and save it to the folder
                    PrefabUtility.SaveAsPrefabAsset(newPrefab, Path.Combine(folderPath, sprite.name + ".prefab"));

                    // Destroy the new game object
                    DestroyImmediate(newPrefab);
                }
            }

            // Refresh the project window to show the new prefabs
            AssetDatabase.Refresh();
        }
    }
}
#endif
