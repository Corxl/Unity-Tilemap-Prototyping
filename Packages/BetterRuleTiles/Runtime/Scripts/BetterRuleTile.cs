using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using VinTools.BetterRuleTiles.Internal;
using System;

namespace VinTools.BetterRuleTiles
{
    [CreateAssetMenu(menuName = "VinTools/Custom Tiles/Better Rule Tile")]
    public class BetterRuleTile : RuleTile<BetterRuleTile.Neighbor>
    {
        public int UniqueID;
        public BetterRuleTile[] otherTiles;
        public List<BetterRuleTile> variations = new List<BetterRuleTile>();
        public BetterRuleTile variationParent;
        public List<CustomTileProperty> customProperties = new List<CustomTileProperty>();

        public List<ExtendedTilingRule> m_ExtendedTilingRules = new List<ExtendedTilingRule>();
        public List<ExtraTilingRule> m_ExtraTilingRules = new List<ExtraTilingRule>();

        [Tooltip("Displays a logwarning when a property was not found with the desired key")]
        public bool DebugMode = false;

        public class Neighbor : RuleTile.TilingRule.Neighbor
        {
            new public const int This = 0;
            public const int Ignore = -1;
            public const int Empty = -2;
            new public const int NotThis = -3;
            public const int Any = -4;
        }

        public override bool RuleMatch(int neighbor, TileBase tile)
        {
            if (tile is RuleOverrideTile ot)
                tile = ot.m_InstanceTile;

            switch (neighbor)
            {
                case Neighbor.This: return tile == this || variations.Contains(tile);
                case Neighbor.NotThis: return tile != this && !variations.Contains(tile);
                case Neighbor.Any: return tile != null;
                case Neighbor.Empty: return tile == null;
                case Neighbor.Ignore: return true;
                default:
                    if (neighbor > 0) return tile == otherTiles[neighbor - 1] || otherTiles[neighbor - 1].variations.Contains(tile);
                    break;
            }
            return true;
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);

            if (m_TilingRules.Count != m_ExtraTilingRules.Count) return;

            for (int i = 0; i < m_TilingRules.Count; i++)
            {
                Matrix4x4 transform = tileData.transform;
                if (RuleMatches(m_TilingRules[i], position, tilemap, ref transform))
                {
                    //TODO check for extra output
                    ExtraTilingRule extraData = m_ExtraTilingRules[i];
                    switch (extraData.m_ExtendedOutputSprite)
                    {
                        case ExtendedOutputSprite.Pattern:
                            Sprite output = GetSprite(position, m_TilingRules[i], extraData);
                            if (output != null) tileData.sprite = output;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public Sprite GetSprite(Vector3Int pos, TilingRule data, ExtraTilingRule extraData)
        {
            //check if array lenght matches the dimensions
            if (data.m_Sprites.Length != extraData.m_Patternsize.x * extraData.m_Patternsize.y) return null;

            //prevents the values to be negative
            while (pos.x < extraData.m_Patternsize.x) { pos.x += extraData.m_Patternsize.x; }
            while (pos.y < extraData.m_Patternsize.y) { pos.y += extraData.m_Patternsize.y; }

            //get the index on each axis
            int x = pos.x % extraData.m_Patternsize.x;
            int y = pos.y % extraData.m_Patternsize.y;

            //get the index in the array
            int index = x + (((extraData.m_Patternsize.y - 1) * extraData.m_Patternsize.x) - y * extraData.m_Patternsize.x);

            //returns the correct sprite
            return data.m_Sprites[index];
        }


        public void SetInt(string key, int value) { if (GetProperty(key, out var p)) p.SetInt(value); }
        public void SetFloat(string key, float value) { if (GetProperty(key, out var p)) p.SetFloat(value); }
        public void SetDouble(string key, double value) { if (GetProperty(key, out var p)) p.SetDouble(value); }
        public void SetChar(string key, char value) { if (GetProperty(key, out var p)) p.SetChar(value); }
        public void SetString(string key, string value) { if (GetProperty(key, out var p)) p.SetString(value); }
        public void SetBool(string key, bool value) { if (GetProperty(key, out var p)) p.SetBool(value); }

        /// <summary>
        /// Gets the integer with the specified key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="defaultValue">The default value if the key was not found</param>
        /// <returns></returns>
        public int GetInt(string key, int defaultValue = default)
        {
            if (GetProperty(key, out var p)) return p.GetInt();
            else return defaultValue;
        }
        /// <summary>
        /// Gets the float with the specified key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="defaultValue">The default value if the key was not found</param>
        /// <returns></returns>
        public float GetFloat(string key, float defaultValue = default)
        {
            if (GetProperty(key, out var p)) return p.GetFloat();
            else return defaultValue;
        }
        /// <summary>
        /// Gets the double with the specified key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="defaultValue">The default value if the key was not found</param>
        /// <returns></returns>
        public double GetDouble(string key, double defaultValue = default)
        {
            if (GetProperty(key, out var p)) return p.GetDouble();
            else return defaultValue;
        }
        /// <summary>
        /// Gets the character with the specified key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="defaultValue">The default value if the key was not found</param>
        /// <returns></returns>
        public char GetChar(string key, char defaultValue = default)
        {
            if (GetProperty(key, out var p)) return p.GetChar();
            else return defaultValue;
        }
        /// <summary>
        /// Gets the string with the specified key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="defaultValue">The default value if the key was not found</param>
        /// <returns></returns>
        public string GetString(string key, string defaultValue = default)
        {
            if (GetProperty(key, out var p)) return p.GetString();
            else return defaultValue;
        }
        /// <summary>
        /// Gets the boolean with the specified key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="defaultValue">The default value if the key was not found</param>
        /// <returns></returns>
        public bool GetBool(string key, bool defaultValue = default)
        {
            if (GetProperty(key, out var p)) return p.GetBool();
            else return defaultValue;
        }

        private bool GetProperty(string key, out CustomTileProperty prop)
        {
            prop = customProperties.Find(t => t._key == key);
            if (prop == null)
            {
                if (DebugMode) Debug.LogWarning($"Custom property \"{key}\" in object \"{this.name}\" was not found.", this);
                return false;
            }
            else return true;
        }

        [Serializable]
        public class ExtendedTilingRule : TilingRule
        {
            public ExtendedOutputSprite m_ExtendedOutputSprite;
            public Vector2Int m_PatternSize = Vector2Int.one;

            public RuleTile.TilingRule.OutputSprite ConvertOutputSprite() => ConvertOutputSprite(m_ExtendedOutputSprite);
            public static RuleTile.TilingRule.OutputSprite ConvertOutputSprite(ExtendedOutputSprite outputSprite)
            {
                switch (outputSprite)
                {
                    case ExtendedOutputSprite.Single: return TilingRuleOutput.OutputSprite.Single;
                    case ExtendedOutputSprite.Random: return TilingRuleOutput.OutputSprite.Random;
                    case ExtendedOutputSprite.Animation: return TilingRuleOutput.OutputSprite.Animation;
                    case ExtendedOutputSprite.Pattern: return default;
                    default: return default;
                }
            }

            public TilingRule ExportTilingRule() => ExportTilingRule(this);
            public static TilingRule ExportTilingRule(ExtendedTilingRule rule)
            {
                TilingRule newRule = new TilingRule();

                newRule.m_ColliderType = rule.m_ColliderType;
                newRule.m_GameObject = rule.m_GameObject;
                newRule.m_MaxAnimationSpeed = rule.m_MaxAnimationSpeed;
                newRule.m_MinAnimationSpeed = rule.m_MinAnimationSpeed;
                newRule.m_NeighborPositions = rule.m_NeighborPositions;
                newRule.m_Neighbors = rule.m_Neighbors;
                newRule.m_Output = rule.ConvertOutputSprite();
                newRule.m_PerlinScale = rule.m_PerlinScale;
                newRule.m_RandomTransform = rule.m_RandomTransform;
                newRule.m_RuleTransform = rule.m_RuleTransform;
                newRule.m_Sprites = new Sprite[rule.m_Sprites.Length];
                Array.Copy(rule.m_Sprites, newRule.m_Sprites, rule.m_Sprites.Length);

                return newRule;
            }


            public ExtraTilingRule ExportExtras() => ExportExtras(this);
            public static ExtraTilingRule ExportExtras(ExtendedTilingRule rule)
            {
                ExtraTilingRule newRule = new ExtraTilingRule();

                newRule.m_ExtendedOutputSprite = rule.m_ExtendedOutputSprite;
                newRule.m_Patternsize = rule.m_PatternSize;

                return newRule;
            }
        }

        public enum ExtendedOutputSprite
        {
            Single,
            Random,
            Animation,
            Pattern
        }

        [Serializable]
        public class ExtraTilingRule
        {
            public ExtendedOutputSprite m_ExtendedOutputSprite = ExtendedOutputSprite.Single;
            public Vector2Int m_Patternsize = Vector2Int.one;
        }
    }
}