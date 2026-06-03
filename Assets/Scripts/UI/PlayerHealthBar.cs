using UnityEngine;
using UnityEngine.UI;

namespace BirdAI
{
    public class PlayerHealthBar : MonoBehaviour
    {
        [Header("Layout")]
        [Tooltip("Width of the health bar in pixels.")]
        public float barWidth = 300f;

        [Tooltip("Height of the health bar in pixels.")]
        public float barHeight = 30f;

        [Tooltip("Offset from the top-left corner of the screen.")]
        public Vector2 screenOffset = new Vector2(20f, 20f);

        [Header("Colors")]
        public Color healthColor = new Color(0.2f, 0.8f, 0.2f);
        public Color damagedColor = new Color(0.8f, 0.2f, 0.2f);
        public Color backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);

        private Canvas _canvas;
        private Image _backgroundImage;
        private Image _fillImage;
        private Text _healthText;

        void Start()
        {
            CreateUI();
        }

        void Update()
        {
            var player = PlayerTarget.Instance;
            if (player == null) return;

            float healthPercent = player.Health / player.maxHealth;
            _fillImage.rectTransform.anchorMax = new Vector2(healthPercent, 1f);
            _fillImage.color = Color.Lerp(damagedColor, healthColor, healthPercent);
            _healthText.text = $"{Mathf.CeilToInt(player.Health)} / {Mathf.CeilToInt(player.maxHealth)}";
        }

        void CreateUI()
        {
            var canvasObject = new GameObject("HealthBarCanvas");
            canvasObject.transform.SetParent(transform);
            _canvas = canvasObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            var backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(canvasObject.transform, false);
            _backgroundImage = backgroundObject.AddComponent<Image>();
            _backgroundImage.color = backgroundColor;
            var backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, 1f);
            backgroundRect.anchorMax = new Vector2(0f, 1f);
            backgroundRect.pivot = new Vector2(0f, 1f);
            backgroundRect.anchoredPosition = new Vector2(screenOffset.x, -screenOffset.y);
            backgroundRect.sizeDelta = new Vector2(barWidth, barHeight);

            var fillObject = new GameObject("Fill");
            fillObject.transform.SetParent(backgroundObject.transform, false);
            _fillImage = fillObject.AddComponent<Image>();
            _fillImage.color = healthColor;
            var fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            var textObject = new GameObject("HealthText");
            textObject.transform.SetParent(backgroundObject.transform, false);
            _healthText = textObject.AddComponent<Text>();
            _healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _healthText.alignment = TextAnchor.MiddleCenter;
            _healthText.fontSize = 16;
            _healthText.color = Color.white;
            _healthText.fontStyle = FontStyle.Bold;
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }
}
