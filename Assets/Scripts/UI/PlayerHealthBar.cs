using UnityEngine;
using UnityEngine.UI;

namespace BirdAI
{
    public class PlayerHealthBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Text healthText;

        [Header("Colors")]
        public Color healthColor = new Color(0.2f, 0.8f, 0.2f);
        public Color damagedColor = new Color(0.8f, 0.2f, 0.2f);

        void Update()
        {
            var player = PlayerTarget.Instance;
            if (player == null) return;

            float healthPercent = player.Health / player.maxHealth;
            fillImage.fillAmount = healthPercent;
            fillImage.color = Color.Lerp(damagedColor, healthColor, healthPercent);
            healthText.text = $"{Mathf.CeilToInt(player.Health)} / {Mathf.CeilToInt(player.maxHealth)}";
        }
    }
}