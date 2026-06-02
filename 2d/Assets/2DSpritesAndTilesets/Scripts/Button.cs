using UnityEngine;
using UnityEngine.Tilemaps;

namespace NimbleGames.BackgroundAssets
{
    public class Button : MonoBehaviour
    {
        [SerializeField] private RuleTile m_RuleTile;
        private Tilemap[] m_Tilemaps; // Array to store all Tilemaps in the scene
        public event System.Action OnButtonClicked;
        [SerializeField] private RandomAssetSpawner.assetsType m_Type;
        [SerializeField] private KeyCode m_Shortcut;

        private void Awake()
        {
            m_Tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None); // Find and store all Tilemaps in the scene
        }

        private void Update()
        {
            if (Input.GetKeyDown(m_Shortcut))
            {
                SwapTiles();
            }
        }

        public void SwapTiles()
        {
            Debug.Log("Swap tiles to: " + m_RuleTile.name);

            // Loop through all the Tilemaps in the scene
            foreach (Tilemap tilemap in m_Tilemaps)
            {
                // Loop through all the tiles in the current Tilemap
                foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
                {
                    TileBase tile = tilemap.GetTile(position);

                    // If the tile matches the original RuleTile, replace it with the new RuleTile
                    if (tile is RuleTile)
                    {
                        tilemap.SetTile(position, m_RuleTile);
                    }
                }
            }

            if (RandomAssetSpawner.I != null)
                UpdateAssets();
        }

        public void UpdateAssets()
        {
            RandomAssetSpawner.I.SpawnRandomAssets(m_Type);
        }
    }
}
