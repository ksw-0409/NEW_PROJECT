using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;

namespace NimbleGames.BackgroundAssets
{
    public class RandomAssetSpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> snowAssets;
        [SerializeField] private List<GameObject> magmaAssets;
        [SerializeField] private List<GameObject> grassAssets;
        [SerializeField] private List<GameObject> crackedMarbleAssets;
        [SerializeField] private List<GameObject> blueStoneAssets;
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private int numClusters;
        [SerializeField] private int minClusterSize;
        [SerializeField] private int maxClusterSize;
        [SerializeField] private int density;
        [SerializeField] private float fadeOutTime = 0.8f;
        [SerializeField] private float fadeInTime = 0.3f;

        [SerializeField] private float positionOffset;
        [SerializeField] private float sizeOffset;

        [Tooltip("Debug")]
        [SerializeField] private bool enableCoroutine = false;
        [SerializeField] private float updateAssetsInterval = 1.5f;
        [SerializeField] private assetsType type;




        private static RandomAssetSpawner m_I;
        public static RandomAssetSpawner I
        {
            get { return m_I; }
        }

        private List<GameObject> spawnedAssets = new List<GameObject>();


        public enum assetsType
        {
            snow,
            magma,
            grass,
            grassAndStone,
            grassAndStone2,
            crackedMarble,
            crackedRock,
            blueStone,
            blueCrystal,
            blueCrystal2
        }

        private void Awake()
        {
            m_I = this;
        }

        private List<GameObject> GetAssetsList(assetsType type)
        {
            switch (type)
            {
                case assetsType.magma:
                    return magmaAssets;
                case assetsType.snow:
                    return snowAssets;
                case assetsType.grass:
                    return grassAssets;
                case assetsType.grassAndStone:
                    return grassAssets;
                case assetsType.grassAndStone2:
                    return grassAssets;
                case assetsType.crackedMarble:
                    return crackedMarbleAssets;
                case assetsType.crackedRock:
                    return grassAssets;
                case assetsType.blueStone:
                    return blueStoneAssets;
                case assetsType.blueCrystal:
                    return blueStoneAssets;
                case assetsType.blueCrystal2:
                    return blueStoneAssets;
                default:
                    return null;
            }
        }

        public void SpawnRandomAssetsInClusters(assetsType assetsType)
        {
            List<GameObject> assetPrefabs = GetAssetsList(assetsType);

            // Delete any previously spawned assets
            foreach (GameObject asset in spawnedAssets)
            {
                StartCoroutine(FadeOut(asset.GetComponent<SpriteRenderer>()));
                Destroy(asset, fadeOutTime);
            }
            spawnedAssets.Clear();

            // Choose random positions on the tilemap for each cluster
            List<Vector3Int> clusterPositions = new List<Vector3Int>();
            for (int i = 0; i < numClusters; i++)
            {
                Vector3Int position = tilemap.cellBounds.min + new Vector3Int(
                    Random.Range(0, tilemap.cellBounds.size.x),
                    Random.Range(0, tilemap.cellBounds.size.y),
                    0);
                clusterPositions.Add(position);
            }

            // Spawn holes in clusters
            for (int i = 0; i < numClusters; i++)
            {
                Vector3Int clusterPosition = clusterPositions[i];
                int clusterSize = Random.Range(minClusterSize, maxClusterSize + 1);

                for (int j = 0; j < clusterSize; j++)
                {
                    // Choose a random position within the cluster bounds
                    Vector3Int position = clusterPosition + new Vector3Int(
                        Random.Range(-5, 6),
                        Random.Range(-5, 6),
                        0);

                    // Set the tile at the position to null (create a hole)
                    tilemap.SetTile(position, null);
                }
            }
        }

        public void SpawnRandomAssets(assetsType assetsType)
        {
            List<GameObject> assetPrefabs = GetAssetsList(assetsType);

            // Delete any previously spawned assets
            foreach (GameObject asset in spawnedAssets)
            {
                StartCoroutine(FadeOut(asset.GetComponent<SpriteRenderer>()));
                Destroy(asset, fadeOutTime);
            }
            spawnedAssets.Clear();

            // Spawn assets individually
            for (int i = 0; i < density; i++)
            {
                // Choose a random asset prefab from the list
                int index = Random.Range(0, assetPrefabs.Count);
                GameObject prefab = assetPrefabs[index];

                // Choose a random position on the tilemap
                Vector3Int position = tilemap.cellBounds.min + new Vector3Int(
                    Random.Range(0, tilemap.cellBounds.size.x),
                    Random.Range(0, tilemap.cellBounds.size.y),
                    0);

                // Check if the tile at the position is non-null
                TileBase tile = tilemap.GetTile(position);
                if (tile != null)
                {
                    // Instantiate the asset prefab at the chosen position
                    Vector3 offsetPos = new Vector2(Random.Range(-positionOffset, positionOffset), Random.Range(-positionOffset, positionOffset));
                    float scaleOffset = Random.Range(-sizeOffset, sizeOffset);
                    GameObject spawnedAsset = Instantiate(prefab, tilemap.GetCellCenterWorld(position) + offsetPos, Quaternion.identity);
                    StartCoroutine(FadeIn(spawnedAsset.GetComponent<SpriteRenderer>()));
                    spawnedAsset.transform.localScale += new Vector3(scaleOffset, scaleOffset, scaleOffset);
                    spawnedAssets.Add(spawnedAsset);
                }
            }
        }
        private IEnumerator C_SwapAsset()
        {
            while (enableCoroutine)
            {
                SpawnRandomAssets(type);
                yield return new WaitForSeconds(updateAssetsInterval);
            }
        }

        private void OnEnable()
        {
            if (enableCoroutine)
            {
                StartCoroutine(C_SwapAsset());
            }
        }

        private void OnDisable()
        {
            StopCoroutine(C_SwapAsset());
        }

        private IEnumerator FadeIn(SpriteRenderer spriteRenderer)
        {
            float elapsedTime = 0f;
            Color originalColor = spriteRenderer.color;
            while (elapsedTime < fadeInTime)
            {
                float alpha = Mathf.Lerp(0f, originalColor.a, elapsedTime / fadeInTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            spriteRenderer.color = originalColor;
        }

        private IEnumerator FadeOut(SpriteRenderer spriteRenderer)
        {
            float elapsedTime = 0f;
            Color originalColor = spriteRenderer.color;
            while (elapsedTime < fadeOutTime)
            {
                float alpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / fadeOutTime);
                if (spriteRenderer != null)
                    spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}