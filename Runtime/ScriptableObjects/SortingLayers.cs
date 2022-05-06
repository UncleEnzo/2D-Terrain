using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using System.Linq;

namespace Nevelson.Terrain
{
    [System.Serializable]
    public class SortingLayerDict : SerializableDictionaryBase<string, int> { }

    [CreateAssetMenu(fileName = "SortingLayerSO", menuName = "Terrain/SortingLayers")]
    public class SortingLayers : ScriptableObject
    {
        [SerializeField] private SortingLayerDict sortingLayers;
        [SerializeField] private List<string> excludeTopLayers;
        [SerializeField] private List<string> respawnUnfriendly;

        private void OnValidate()
        {
            var intersect = sortingLayers.Where(i => sortingLayers.Any(t => t.Key != i.Key && t.Value == i.Value))
    .ToDictionary(i => i.Key, i => i.Value);
            foreach (var kvPair in intersect)
            {
                Debug.LogError($"Key: {kvPair.Key} with Value has a duplicate: {kvPair.Value}");
            }

            foreach (var layer in excludeTopLayers)
            {
                if (!sortingLayers.ContainsKey(layer))
                {
                    Debug.Log($"Key: {layer} in ignoreOnTop list does not exist in {sortingLayers}");
                }
            }

            foreach (var layer in respawnUnfriendly)
            {
                if (!sortingLayers.ContainsKey(layer))
                {
                    Debug.Log($"Key: {layer} in respawnUnfriendly list does not exist in {sortingLayers}");
                }
            }
        }

        public int GetValue(string layerName, bool excludeTopLayer = false)
        {
            if (excludeTopLayer && excludeTopLayers.Contains(layerName))
            {
                Debug.LogError($"Layer: {layerName} is excluded");
                return -1;
            }
            return sortingLayers[layerName];
        }

        public bool TryGetValue(string layerName, out int layer, bool excludeTopLayer = false)
        {
            if (excludeTopLayer && excludeTopLayers.Contains(layerName))
            {
                layer = -1;
                return false;
            }
            return sortingLayers.TryGetValue(layerName, out layer);
        }

        public bool ContainsKey(string layerName, bool excludeTopLayer = false)
        {
            if (excludeTopLayer && excludeTopLayers.Contains(layerName))
            {
                return false;
            }
            return sortingLayers.ContainsKey(layerName);
        }

        public bool IsRespawnFriendly(string layerName, bool excludeTopLayer = false)
        {
            if (excludeTopLayer && excludeTopLayers.Contains(layerName))
            {
                return false;
            }

            if (!sortingLayers.ContainsKey(layerName))
            {
                Debug.LogError($"Sorting layers does not contain layer {layerName}");
                return false;
            }

            return !respawnUnfriendly.Contains(layerName);
        }

        public bool IsRespawnFriendly(int layer, bool excludeTopLayer = false)
        {
            if (!sortingLayers.ContainsValue(layer))
            {
                Debug.LogError($"Sorting layers does not contain layer {layer}");
                return false;
            }

            string layerName = sortingLayers.FirstOrDefault(x => x.Value == layer).Key;
            if (excludeTopLayer && excludeTopLayers.Contains(layerName))
            {
                return false;
            }

            return !respawnUnfriendly.Contains(layerName);
        }
    }
}