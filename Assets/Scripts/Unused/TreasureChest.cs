using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Contents")]
    public int goldAmount = 100;

    [Header("References")]
    public GameObject interactPrompt; // assign a UI "Press E" object
    public ParticleSystem sparkles;
    public Light chestLight;

    private Transform _player;
    private bool _isOpen = false;
    private bool _playerInRange = false;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("PlayerTarget")?.transform;

        if (interactPrompt) interactPrompt.SetActive(false);
    }

    void Update()
    {
        if (_isOpen || _player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);
        _playerInRange = dist <= interactDistance;

        // Show/hide prompt
        if (interactPrompt)
            interactPrompt.SetActive(_playerInRange);

        // Open on E press
        if (_playerInRange && Input.GetKeyDown(interactKey))
            OpenChest();
    }

    void OpenChest()
    {
        _isOpen = true;

        if (interactPrompt) interactPrompt.SetActive(false);

        // Animate lid open
        StartCoroutine(OpenLid());

        // Trigger effects
        if (sparkles) sparkles.Play();
        if (chestLight) StartCoroutine(PulseLight());

        // TODO: Add your gold/inventory logic here
        Debug.Log($"Chest opened! Gained {goldAmount} gold.");
    }

    System.Collections.IEnumerator OpenLid()
    {
        // Assumes first child is the lid
        Transform lid = transform.GetChild(0);
        if (lid == null) yield break;

        float elapsed = 0f;
        float duration = 0.4f;
        Quaternion startRot = lid.localRotation;
        Quaternion endRot = Quaternion.Euler(-110f, 0f, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            lid.localRotation = Quaternion.Lerp(startRot, endRot, elapsed / duration);
            yield return null;
        }

        lid.localRotation = endRot;
    }

    System.Collections.IEnumerator PulseLight()
    {
        if (chestLight == null) yield break;

        float original = chestLight.intensity;
        float peak = original * 4f;
        float elapsed = 0f;
        float duration = 0.6f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            chestLight.intensity = Mathf.Lerp(peak, original, t);
            yield return null;
        }

        chestLight.intensity = original;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}