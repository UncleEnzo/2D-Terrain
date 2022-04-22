using UnityEngine;

namespace Nevelson.Terrain
{
    public class RespawnData : MonoBehaviour
    {
        [SerializeField] private string respawnTrigger = "";
        [SerializeField] private GameObject[] respawnLocations = new GameObject[0];

        private void Start()
        {
            if (respawnLocations.Length <= 0)
            {
                Debug.LogError("Respawn Trigger Does Not Contain Respawn Locations");
            }
        }


        private void OnTriggerStay2D(Collider2D collision)
        {
            OnTriggerStay_Player(collision);
        }

        //If player enters trigger sends him the respawn points for the location
        private void OnTriggerStay_Player(Collider2D collision)
        {
            if (collision.CompareTag(Constants.OBJECT_COLLIDER))
            {
                if (collision.transform.parent.CompareTag(respawnTrigger))
                {
                    //Need to think of a better way to send respawn data
                    collision.transform.parent.GetComponent<PlayerController>().SetRespawnPoints(respawnLocations);
                }

            }
        }
    }
}