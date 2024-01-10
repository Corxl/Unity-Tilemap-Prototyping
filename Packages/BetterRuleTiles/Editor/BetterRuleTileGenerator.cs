#if UNITY_EDITOR
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Tilemaps;
using VinTools.BetterRuleTiles;
using VinTools.BetterRuleTiles.Internal;

namespace VinToolsEditor.BetterRuleTiles
{
    public class BetterRuleTileGenerator
    {
        public static void GenerateTiles(BetterRuleTileContainer container)
        {
            List<BetterRuleTile> tiles = new List<BetterRuleTile>();

            //generate tiles
            for (int i = 0; i < container.Tiles.Count; i++)
            {
                tiles.Add(GenerateTile(container, container.Tiles[i].UniqueID));
            }

            //assign the other tiles
            for (int i = 0; i < tiles.Count; i++)
            {
                //set tiles to their correct place
                foreach (var item in tiles) tiles[i].otherTiles[item.UniqueID - 1] = item;
            }

            //set variations
            SetVariations(container, tiles);
            //copy extended tiling rule to regular tiling rule
            foreach (var tile in tiles) ExportTilingRules(tile);

            //delete unused previous tiles 
            container.DeleteUnusedTileObjects();
            //create Tiles
            foreach (var tile in tiles) container.SaveObjectToAsset(tile);

            //create palette
            //CreatePalette(container);

            //highlight object
            Selection.activeObject = container;
            EditorGUIUtility.PingObject(container);
        }

        static BetterRuleTile GenerateTile(BetterRuleTileContainer container, int UniqueID)
        {
            BetterRuleTile tile = container._tileObjects.Find(t => t.UniqueID == UniqueID);
            if (tile == null) tile = ScriptableObject.CreateInstance<BetterRuleTile>();

            var templateTile = container.Tiles.Find(t => t.UniqueID == UniqueID);

            tile.name = templateTile.Name;
            tile.UniqueID = UniqueID;
            tile.variationParent = null;
            tile.otherTiles = new BetterRuleTile[container._tiles.Max(t => t.UniqueID)];
            tile.m_DefaultColliderType = templateTile.ColliderType;

            tile.m_ExtendedTilingRules = GenerateRules(container, UniqueID);
            tile.m_DefaultSprite = templateTile.DefaultSprite;
            tile.m_DefaultGameObject = templateTile.DefaultGameObject;

            tile.customProperties.Clear();
            for (int i = 0; i < templateTile.customProperties.Count; i++)
            {
                tile.customProperties.Add(new CustomTileProperty(templateTile.customProperties[i]));
            }

            if (tile.m_DefaultSprite == null)
            {
                Debug.LogWarning($"Default sprite of tile \"{tile}\" is not set, which could result in the tile displaying as a blank space.");
            }

            return tile;
        }
        static List<BetterRuleTile.ExtendedTilingRule> GenerateRules(BetterRuleTileContainer container, int UniqueID)
        {
            //create new tiling rule list
            List<BetterRuleTile.ExtendedTilingRule> tilingRules = new List<BetterRuleTile.ExtendedTilingRule>();

            //find the tile object
            var tileOf = container.Tiles.Find(t => t.UniqueID == UniqueID);

            foreach (var item in container.Grid.FindAll(t => t.TileID == UniqueID))
            {
                //ignore tile if has no sprite
                if (item.Sprite == null) continue;
                item.NeighborPositions = item.NeighborPositions.OrderBy(t => t.y * -10000 + t.x).ToList();
                List<Vector3Int> NeighborPositions = new List<Vector3Int>();
                foreach (var neighbor in item.NeighborPositions) NeighborPositions.Add((Vector3Int)container.EditorToUnityCoord(new Vector2Int(neighbor.x, neighbor.y), item.Position));   

                //create tiling
                int[] neighbors = new int[NeighborPositions.Count];

                //set neighbors
                for (int i = 0; i < neighbors.Length; i++)
                {
                    //neighbors[i] = GetNeighborRule(container, container.Grid.Find(t => t.Position == new Vector2Int(item.Position.x + NeighborPositions[i].x, item.Position.y - NeighborPositions[i].y)), UniqueID);
                    neighbors[i] = GetNeighborRule(container, container.Grid.Find(t => t.Position == item.Position + container.EditorToUnityCoord((Vector2Int)NeighborPositions[i], item.Position)), UniqueID);
                }

                //create sprites array
                List<Sprite> sprites = new List<Sprite>();
                if (item.OutputSprite == BetterRuleTile.ExtendedOutputSprite.Single || item.IncludeSpriteInOutput || item.Sprites.Length <= 0) sprites.Add(item.Sprite);
                if (item.OutputSprite != BetterRuleTile.ExtendedOutputSprite.Single) sprites.AddRange(item.Sprites);

                //create tiling rule
                BetterRuleTile.ExtendedTilingRule tilingRule = new BetterRuleTile.ExtendedTilingRule();
                tilingRule.m_NeighborPositions = container.DisplaceRules(NeighborPositions, item.Position);
                tilingRule.m_Neighbors = neighbors.ToList();
                tilingRule.m_Sprites = sprites.ToArray();
                tilingRule.m_ColliderType = item.UseDefaultSettings ? tileOf.ColliderType : item.ColliderType;
                tilingRule.m_GameObject = item.UseDefaultSettings ? tileOf.DefaultGameObject : item.GameObject;
                tilingRule.m_ExtendedOutputSprite = item.OutputSprite;
                tilingRule.m_RuleTransform = item.Transform;
                tilingRule.m_PatternSize = item.PatternSize;
                if (item.OutputSprite == BetterRuleTile.ExtendedOutputSprite.Random)
                {
                    tilingRule.m_PerlinScale = item.NoiseScale;
                    tilingRule.m_RandomTransform = item.RandomTransform;
                }
                if (item.OutputSprite == BetterRuleTile.ExtendedOutputSprite.Animation)
                {
                    tilingRule.m_MaxAnimationSpeed = item.MaxAnimationSpeed;
                    tilingRule.m_MinAnimationSpeed = item.MinAnimationSpeed;
                }
                //tilingRule.m_Id

                //add tiling rule to list
                tilingRules.Add(tilingRule);
            }

            //return list
            tilingRules = RemoveDuplicates(tilingRules);
            if (container.settings._collapseSimilarRules) tilingRules = SimplifyNeighborRules(tilingRules);
            tilingRules = SortRules(tilingRules);
            return tilingRules;
        }
        static List<BetterRuleTile.ExtendedTilingRule> RemoveDuplicates(List<BetterRuleTile.ExtendedTilingRule> tilingRules)
        {
            //for loop, but lets me remove things from the list
            int i = 0;
            while (i < tilingRules.Count)
            {
                for (int b = tilingRules.Count - 1; b > i; b--)
                {
                    if (CheckSame(tilingRules[i], tilingRules[b])) tilingRules.RemoveAt(b);
                }

                //increase index
                i++;
            }

            //return
            return tilingRules;
        }
        static List<BetterRuleTile.ExtendedTilingRule> SimplifyNeighborRules(List<BetterRuleTile.ExtendedTilingRule> tilingRules)
        {
            //Debug.Log("Simplifying");

            //create a temporary list to store tiles
            List<BetterRuleTile.ExtendedTilingRule> rulesToAdd = new List<BetterRuleTile.ExtendedTilingRule>();

            //for loop, but lets me remove things from the list
            int i = 0;
            while (i < tilingRules.Count)
            {
                //find sprites with the same 
                var match = tilingRules.FindAll(t => Equals(t, tilingRules[i]));

                //if there ar more than one tiles with the same sprite
                if (match.Count > 1)
                {
                    //Debug.Log("Found match");

                    //remove duplicate sprites
                    foreach (var item in match) tilingRules.Remove(item);

                    //add simplified version
                    rulesToAdd.Add(SimlifyRules(match));
                }
                else
                {
                    //increase index
                    i++;
                }
            }

            //add rules and return it
            tilingRules.AddRange(rulesToAdd);
            return tilingRules;
        }

        static bool Equals(RuleTile.TilingRule t1, RuleTile.TilingRule t2)
        {
            //TODO there should be a more specific check to prevent accidental simplification

            if (t1.m_Sprites[0] != t2.m_Sprites[0]) return false;
            if (t2.m_Sprites.Length > 1) return false;
            if (t1.m_NeighborPositions.Count != t2.m_NeighborPositions.Count) return false;

            for (int i = 0; i < t1.m_NeighborPositions.Count; i++) if (t1.m_NeighborPositions[i] != t2.m_NeighborPositions[i]) return false;

            return true;
        }
        static BetterRuleTile.ExtendedTilingRule SimlifyRules(List<BetterRuleTile.ExtendedTilingRule> tilingRules)
        {
            //create a tiling rule for the 
            BetterRuleTile.ExtendedTilingRule tilingRule = tilingRules[0];

            //check every neighbor
            for (int i = 0; i < tilingRule.m_NeighborPositions.Count; i++)
            {
                //check in every tile at that position
                for (int t = 0; t < tilingRules.Count; t++)
                {
                    //if not same just make the tile ignore that position
                    if (tilingRules[t].m_Neighbors[i] != tilingRule.m_Neighbors[i])
                    {
                        tilingRule.m_Neighbors[i] = BetterRuleTile.Neighbor.Ignore;
                        break;
                    }
                }
            }

            //return that tile
            return tilingRule;
        }
        static bool CheckSame(RuleTile.TilingRule tr1, RuleTile.TilingRule tr2)
        {
            if (tr1.m_Neighbors.Count != tr2.m_Neighbors.Count) return false;
            if (tr1.m_NeighborPositions.Count != tr2.m_NeighborPositions.Count) return false;
            if (tr1.m_Sprites.Length != tr2.m_Sprites.Length) return false;

            for (int i = 0; i < tr1.m_Neighbors.Count; i++) if (tr1.m_Neighbors[i] != tr2.m_Neighbors[i]) return false;
            for (int i = 0; i < tr1.m_NeighborPositions.Count; i++) if (tr1.m_NeighborPositions[i] != tr2.m_NeighborPositions[i]) return false;
            for (int i = 0; i < tr1.m_Sprites.Length; i++) if (tr1.m_Sprites[i] != tr2.m_Sprites[i]) return false;

            return true;
        }
        static List<BetterRuleTile.ExtendedTilingRule> SortRules(List<BetterRuleTile.ExtendedTilingRule> tilingRules) => tilingRules.OrderByDescending(t => GetNumberOfNeighbors(t)).ToList();
        static int GetNumberOfNeighbors(RuleTile.TilingRule tr)
        {
            int num = 0;
            for (int i = 0; i < tr.m_Neighbors.Count; i++)
            {
                if (tr.m_Neighbors[i] != BetterRuleTile.Neighbor.Ignore) num++;
                if (tr.m_Neighbors[i] > 0) num++;
            }
            return num;
        }
        static int GetNeighborRule(BetterRuleTileContainer container, BetterRuleTileContainer.GridCell cell, int TileID)
        {
            if (cell == null) return BetterRuleTile.Neighbor.Ignore;
            if (cell.TileID == TileID) return BetterRuleTile.Neighbor.This;
            if (cell.TileID == -3) return BetterRuleTile.Neighbor.NotThis;
            if (cell.TileID == -4) return BetterRuleTile.Neighbor.Any;
            if (cell.TileID == -2) return BetterRuleTile.Neighbor.Empty;

            if (container.Tiles.Exists(t => t.UniqueID == cell.TileID)) return cell.TileID;

            return BetterRuleTile.Neighbor.Ignore;
        }

        static void SetVariations(BetterRuleTileContainer container, List<BetterRuleTile> tiles)
        {
            //for every tile
            for (int i = 0; i < tiles.Count; i++)
            {
                //find tiles where the variated tile is this tile
                var variations = container._tiles.Where(t => !t.uniqueTile && t.variationOf == tiles[i].UniqueID).ToArray();
                //add those tiles
                tiles[i].variations.Clear();
                foreach (var item in variations)
                {
                    var obj = tiles.Find(t => t.UniqueID == item.UniqueID);

                    obj.variationParent = tiles[i];
                    //tiles[i].variations.Add(obj); //not needed since the parent is only for adding the missing rules
                }

                //add variations set up manually by the user
                var tileOf = container.Tiles.Find(t => t.UniqueID == tiles[i].UniqueID);
                foreach (var item in tileOf.variations)
                {
                    var brTile = tiles.Find(t => t.UniqueID == item);
                    if (brTile != null && !tiles[i].variations.Contains(brTile)) tiles[i].variations.Add(brTile);
                }

                //get the data of the tile
                if (tiles[i].variationParent != null && tiles[i].variationParent != tiles[i]) AddParentRules(tiles[i]);
            }
        }
        static void AddParentRules(BetterRuleTile tile)
        {
            //add extra rules for parent tile
            foreach (var item in tile.variationParent.m_ExtendedTilingRules)
            {
                //var neighbors = item.m_Neighbors;
                //for (int i = 0; i < neighbors.Count; i++) if (neighbors[i] == BetterRuleTile.Neighbor.This) neighbors[i] = tile.variationParent.UniqueID;

                tile.m_ExtendedTilingRules.Add(new BetterRuleTile.ExtendedTilingRule
                {
                    m_Neighbors = item.m_Neighbors,
                    m_NeighborPositions = item.m_NeighborPositions,
                    m_Sprites = item.m_Sprites,
                    m_ColliderType = item.m_ColliderType,
                    m_GameObject = item.m_GameObject,
                    m_MaxAnimationSpeed = item.m_MaxAnimationSpeed,
                    m_MinAnimationSpeed = item.m_MinAnimationSpeed,
                    m_Output = item.m_Output,
                    m_Id = item.m_Id,
                    m_PerlinScale = item.m_PerlinScale,
                    m_RandomTransform = item.m_RandomTransform,
                    m_RuleTransform = item.m_RuleTransform,
                });
            }
            //clear duplicate rules
            tile.m_ExtendedTilingRules = RemoveDuplicates(tile.m_ExtendedTilingRules);
        }
        
        static void ExportTilingRules(BetterRuleTile tile)
        {
            int count = tile.m_ExtendedTilingRules.Count();

            var tempTilingRules = new RuleTile.TilingRule[count];
            var tempExtras = new BetterRuleTile.ExtraTilingRule[count];

            for (int i = 0; i < count; i++)
            {
                tempTilingRules[i] = tile.m_ExtendedTilingRules[i].ExportTilingRule();
                tempExtras[i] = tile.m_ExtendedTilingRules[i].ExportExtras();
            }

            tile.m_TilingRules = tempTilingRules.ToList();
            tile.m_ExtraTilingRules = tempExtras.ToList();

            tile.m_ExtendedTilingRules.Clear();
        }

        
        /*public static void CreatePalette(BetterRuleTileContainer container)
        {
            var palette = GridPaletteUtility.CreateNewPalette("Assets", "Test palette", GridLayout.CellLayout.Rectangle, GridPalette.CellSizing.Automatic, Vector2.one, GridLayout.CellSwizzle.XYZ);
        }*/
    }
}

#endif