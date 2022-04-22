using UnityEngine;

namespace Nevelson.Terrain
{
    public class RespawnData : MonoBehaviour
    {
        public GameObject[] respawnLocations;

        void Start()
        {
            if (respawnLocations.Length <= 0) Debug.LogError("Respawn Trigger missing respawnLocation data");
        }


        public void OnTriggerStay2D(Collider2D collision)
        {
            OnTriggerStay_Player(collision);
        }

        //If player enters trigger sends him the respawn points for the location
        private void OnTriggerStay_Player(Collider2D collision)
        {
            if (collision.CompareTag(ConstantValues.OBJECT_COLLIDER))
            {
                if (collision.transform.parent.CompareTag(ConstantValues.PLAYER))
                {
                    collision.transform.parent.GetComponent<PlayerController>().SetRespawnPoints(respawnLocations);
                }

            }
        }
    }
}