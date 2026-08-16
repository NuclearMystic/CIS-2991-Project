using System.Collections;
using CIS2991Project.Managers;
using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.Items
{
    [RequireComponent(typeof(Collider2D))]
    public class ConsumablePickup : MonoBehaviour
    {
        [SerializeField] private global::Item item;
        [SerializeField, Min(1)] private int amount = 1;
        [SerializeField] private AudioClip dropSound;

        private const float BounceDuration = 0.35f;
        private const float BounceHeight = 0.35f;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        public void Configure(global::Item pickupItem, int pickupAmount = 1, AudioClip pickupDropSound = null)
        {
            // Runtime scene builders use this to turn PostApocalypse props into inventory loot.
            item = pickupItem;
            amount = Mathf.Max(1, pickupAmount);
            dropSound = pickupDropSound;
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private void Start()
        {
            StartCoroutine(BounceRoutine());
            PlaySfx(dropSound, transform.position);
        }

        private IEnumerator BounceRoutine()
        {
            var startPos = transform.position;
            var elapsed = 0f;

            while (elapsed < BounceDuration)
            {
                elapsed += Time.deltaTime;
                var arc = Mathf.Sin(Mathf.Clamp01(elapsed / BounceDuration) * Mathf.PI) * BounceHeight;
                transform.position = startPos + Vector3.up * arc;
                yield return null;
            }

            transform.position = startPos;
        }

        private static void PlaySfx(AudioClip clip, Vector2 position)
        {
            if (clip == null)
            {
                return;
            }

            var go = new GameObject("SFX_" + clip.name);
            go.transform.position = position;

            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.outputAudioMixerGroup = AudioManager.SfxGroup;
            source.spatialBlend = 0f;
            source.Play();

            Destroy(go, clip.length);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (item == null)
            {
                return;
            }

            var inventory = other.GetComponentInParent<PlayerInventory>();
            if (inventory == null)
            {
                return;
            }

            if (inventory.TryAdd(item, amount))
            {
                inventory.NotifyItemPickedUp(item, amount);
                Destroy(gameObject);
                return;
            }

            Debug.Log($"Inventory full. Could not pick up {item.displayName}.", this);
        }
    }
}
