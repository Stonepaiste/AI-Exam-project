using UnityEngine;
using UnityEngine.UI;

namespace BirdAI
{
    /// Simple on-screen warning message.
    /// Builds its own Canvas at runtime — just drop on any GameObject.
    /// Call Show() / Hide() from behavior graph action nodes.
    public class TargetedWarningUI : MonoBehaviour
    {
        private static TargetedWarningUI _instance;
        public static TargetedWarningUI Instance => _instance;

        [Header("Message")]
        public string warningMessage = "YOU ARE TARGETED\nGET TO COVER NOW";

        [Header("Style")]
        public int fontSize = 42;
        public Color textColor = new Color(1f, 0.15f, 0.15f);
        public Color outlineColor = new Color(0.1f, 0f, 0f);

        [Header("Flash")]
        public float flashSpeed = 2f;
        [Range(0f, 1f)]
        public float flashMinAlpha = 0.3f;

        private Text _warningText;
        private bool _showing;

        void Awake()
        {
            _instance = this;
            CreateUI();
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        void Update()
        {
            if (!_showing) return;

            float alpha = Mathf.Lerp(flashMinAlpha, 1f,
                (Mathf.Sin(Time.time * flashSpeed * Mathf.PI * 2f) + 1f) * 0.5f);
            var c = textColor;
            c.a = alpha;
            _warningText.color = c;
        }

        /// Show the warning. Called from the behavior graph.
        public void Show()
        {
            if (_showing) return;
            _showing = true;
            _warningText.gameObject.SetActive(true);
        }

        /// Hide the warning. Called from the behavior graph.
        public void Hide()
        {
            if (!_showing) return;
            _showing = false;
            _warningText.gameObject.SetActive(false);
        }

        void CreateUI()
        {
            var canvasGO = new GameObject("WarningCanvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            var textGO = new GameObject("WarningText");
            textGO.transform.SetParent(canvasGO.transform, false);

            _warningText = textGO.AddComponent<Text>();
            _warningText.text = warningMessage;
            _warningText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _warningText.fontSize = fontSize;
            _warningText.fontStyle = FontStyle.Bold;
            _warningText.color = textColor;
            _warningText.alignment = TextAnchor.MiddleCenter;
            _warningText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _warningText.verticalOverflow = VerticalWrapMode.Overflow;

            var outline = textGO.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(2f, -2f);

            var rect = textGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -80f);
            rect.sizeDelta = new Vector2(800f, 120f);

            textGO.SetActive(false);
            _showing = false;
        }
    }
}
