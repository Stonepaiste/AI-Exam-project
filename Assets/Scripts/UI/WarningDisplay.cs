using UnityEngine;
using UnityEngine.UI;

namespace BirdAI
{
    /// Simple on-screen warning message.
    /// Call Show() / Hide() from behavior graph action nodes.
    public class WarningDisplay : MonoBehaviour
    {
        private static WarningDisplay _instance;
        public static WarningDisplay Instance => _instance;

        [SerializeField] private GameObject warningPanel;
        [SerializeField] private Text warningText;
        
        [Header("Flash")]
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private float flashSpeed = 1f;
        [SerializeField] private float flashMinAlpha = 0.2f;

        private bool _showing;

        void Awake()
        {
            _instance = this;
           
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        void Update()
        {
            if (!_showing) return;
            
            // Makes the text flash while activeatied

            float alpha = Mathf.Lerp(flashMinAlpha, 1f,
                (Mathf.Sin(Time.time * flashSpeed * Mathf.PI * 2f) + 1f) * 0.5f);
            var flashColor = textColor;
            flashColor.a = alpha;
            warningText.color = flashColor;
        }


        /// Show the warning. Called from the behavior graph.
        public void Show()
        {
           
            warningPanel.SetActive(true);
            _showing = true;
        }

        /// Hide the warning. Called from the behavior graph.
        public void Hide()
        {
            _showing = false;
            warningPanel.SetActive(false);
            warningText.color = textColor;
        }


    }
}
