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

            float ratio = player.Health / player.maxHealth;
            _fillImage.rectTransform.anchorMax = new Vector2(ratio, 1f);
            _fillImage.color = Color.Lerp(damagedColor, healthColor, ratio);
            _healthText.text = $"{Mathf.CeilToInt(player.Health)} / {Mathf.CeilToInt(player.maxHealth)}";
        }

        void CreateUI()
        {
            var canvasGO = new GameObject("HealthBarCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            _backgroundImage = bgGO.AddComponent<Image>();
            _backgroundImage.color = backgroundColor;
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 1f);
            bgRect.anchorMax = new Vector2(0f, 1f);
            bgRect.pivot = new Vector2(0f, 1f);
            bgRect.anchoredPosition = new Vector2(screenOffset.x, -screenOffset.y);
            bgRect.sizeDelta = new Vector2(barWidth, barHeight);

            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(bgGO.transform, false);
            _fillImage = fillGO.AddComponent<Image>();
            _fillImage.color = healthColor;
            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            var textGO = new GameObject("HealthText");
            textGO.transform.SetParent(bgGO.transform, false);
            _healthText = textGO.AddComponent<Text>();
            _healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _healthText.alignment = TextAnchor.MiddleCenter;
            _healthText.fontSize = 16;
            _healthText.color = Color.white;
            _healthText.fontStyle = FontStyle.Bold;
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }
}
