using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Player_Move))]
public class PlaceLand : MonoBehaviour
{
    public RuleTile placementTile;
    public Grid grid;
    public Tilemap targetMap;
    [Range(0, 15)] public float maxPlacementDistance = 8f;
    // Start is called before the first frame update

    private Vector3Int prevPlacedPos = Vector3Int.zero, prevRemovedPos = Vector3Int.zero;
    private Player_Move player;
    public GenerateWorld world;
    void OnEnable()
    {
        this.player = GetComponent<Player_Move>();
    }
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            UpdateTile(true);
        }
        if (Input.GetMouseButton(1))
        {
            UpdateTile(false);
        }
    }

    private void UpdateTile(bool place)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mPos = (Vector2) mousePos;
        //int count = 0;
        if (Vector2.Distance(mPos, transform.position) <= maxPlacementDistance)
        {
            //count++;
            Vector3Int cellPosition = grid.WorldToCell(mousePos);

            world.UpdateTile(cellPosition);
            
            //if (targetMap?.GetTile(cellPosition) == null)
            //{
                //count++;
            //Debug.Log("TileMap updated.");
            //targetMap.SetTile(cellPosition, place ? placementTile : null);
            //}
        }
        //Debug.Log(count);
    }
}
