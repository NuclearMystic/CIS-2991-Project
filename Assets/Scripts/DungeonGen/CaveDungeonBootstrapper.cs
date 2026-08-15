using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.DungeonGen
{
    // Wires the ported generator pipeline together and kicks off the first generation.
    // RogueAdjacent did this wiring via Inspector-configured UnityEvent persistent calls on its own
    // prefabs; wiring it here in code instead, since a hand-authored UnityEvent<DungeonData>
    // persistent call in scene YAML isn't something that can be verified without a Unity Editor to
    // test in - AddListener achieves the exact same runtime behavior and is easy to read/verify.
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
        }
    }
}
