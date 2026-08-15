using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.Levels
{
    // Drop this on an empty GameObject sized to a level's playable area (via the Collider2D) to
    // stop the camera from showing past the level's edges. The player/camera persist across scene
    // loads (see GameManager/GameBootstrapper), so each level registers its own bounds on load;
    // levels with no LevelBounds object are left unbounded.
    [RequireComponent(typeof(Collider2D))]
    public sealed class LevelBounds : MonoBehaviour
    {
        private const string LayerName = "LevelBounds";

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Awake()
        {
            // Never let this block movement - it exists purely to describe an area for the camera.
            GetComponent<Collider2D>().isTrigger = true;

            var layer = LayerMask.NameToLayer(LayerName);
            if (layer < 0)
            {
                Debug.LogWarning($"LevelBounds: layer \"{LayerName}\" is not defined in " +
                                  "Project Settings > Tags and Layers - collisions/triggers were not disabled.");
                return;
            }

            gameObject.layer = layer;

            // Give this layer no collision/trigger interactions with anything - it exists purely to be
            // read via GetComponent<Collider2D>().bounds and should never fire OnTrigger callbacks on
            // other objects (a level-spanning trigger that the player always stands inside would
            // otherwise look like a "wall" to anything checking for unrecognized trigger overlaps).
            for (var otherLayer = 0; otherLayer < 32; otherLayer++)
            {
                Physics2D.IgnoreLayerCollision(layer, otherLayer, true);
            }
        }

        private void Start()
        {
            var cameraController = FindAnyObjectByType<CameraController>();
            if (cameraController == null)
            {
                Debug.LogWarning("LevelBounds: no CameraController found in scene to register bounds with.");
                return;
            }

            cameraController.SetBounds(GetComponent<Collider2D>());
        }

        private void OnDrawGizmos()
        {
            var collider = GetComponent<Collider2D>();
            if (collider == null)
                return;

            Gizmos.color = Color.cyan;
            var bounds = collider.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}
