using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.DungeonGen
{
    // Wires the ported generator pipeline together and kicks off the first generation.
    // RogueAdjacent did this wiring via Inspector-configured UnityEvent persistent calls on its own
    // prefabs; wiring it here in code instead, since a hand-authored UnityEvent<DungeonData>
    // persistent call in scene YAML isn't something that can be verified without a Unity Editor to
    // test in - AddListener achieves the exact same runtime behavior and is easy to read/verify.
    //
    // Deliberately not following GameBootstrapper's pattern (dynamically-created, singleton-guarded,
    // DontDestroyOnLoad): this component's whole job is wiring together THIS scene's own generator,
    // room-content-generator, and player-room references, which only makes sense placed directly in
    // the CaveDungeon scene with those Inspector slots filled in - there's nothing here that should
    // (or could) survive into a different scene.
    public sealed class CaveDungeonBootstrapper : MonoBehaviour
    {
        [SerializeField] private CorridorFirstDungeonGenerator generator;
        [SerializeField] private RoomContentGenerator roomContentGenerator;

        [Tooltip("Same object as RoomContentGenerator's own Player Room field - typed concretely " +
                 "here so PlayerSpawnPosition (only on PlayerRoom, not the abstract RoomGenerator) " +
                 "is readable once the room has been built.")]
        [SerializeField] private PlayerRoom playerRoom;

        private void Awake()
        {
            generator.OnDungeonFloorReady.AddListener(HandleDungeonReady);
        }

        private void Start()
        {
            generator.GenerateDungeon();
        }

        private void HandleDungeonReady(DungeonData data)
        {
            roomContentGenerator.GenerateRoomContent(data);
            PositionPlayer();
        }

        private void PositionPlayer()
        {
            var player = FindAnyObjectByType<PlayerHealth>();
            if (player == null)
                return;

            var spawnPosition = playerRoom.PlayerSpawnPosition;
            player.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, player.transform.position.z);
            Debug.Log($"[SaveDebug] CaveDungeonBootstrapper.PositionPlayer: moved player to generated spawn room {spawnPosition} - this runs after any load, so a saved position inside a cave dungeon is always overridden by the newly generated layout.");
        }
    }
}
