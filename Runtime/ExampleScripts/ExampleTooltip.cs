using Nevelson.Utils;
using TMPro;
using UnityEngine;

namespace Nevelson.Terrain
{
    public class ExampleTooltip : MonoBehaviour
    {
        private GameObject tooltip;
        private ExamplePlayer player;
        private TextMeshProUGUI text;

        void Start()
        {
            player = FindObjectOfType<ExamplePlayer>();
            tooltip = transform.GetChild(0).gameObject;
            text = tooltip.GetComponent<TextMeshProUGUI>();
            tooltip.SetActive(false);
        }

        void Update()
        {
            transform.position = Camera.main.WorldToScreenPoint(player.transform.Position2D() + new Vector2(0, 2));

            tooltip.SetActive(!player.InteractTooltip.Equals(""));
            text.text = player.InteractTooltip;
        }
    }
}