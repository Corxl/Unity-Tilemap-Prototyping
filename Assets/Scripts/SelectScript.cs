using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectScript : MonoBehaviour
{
    public Grid grid;
    public TileBase selectTile;
    private Vector3Int previousPosition = Vector3Int.zero;
    public Tilemap selectMap;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mPos = (Vector2)mousePos;
        Vector3Int cellPosition = grid.WorldToCell(mousePos); 
        if (cellPosition != previousPosition)
        {
            if (Vector2.Distance(mPos, transform.position) > transform.GetComponent<PlaceLand>()?.maxPlacementDistance)
            {
                selectMap?.SetTile(cellPosition, null);
            } else
            {
                selectMap?.SetTile(cellPosition, selectTile);
            }
            selectMap?.SetTile(previousPosition, null);
            previousPosition = cellPosition;
        }
        
    }
}
