using System.Collections;
using CIS2991Project.Managers;
using UnityEngine;
using UnityEngine.UIElements;

namespace CIS2991Project.UI
{
    // Shared save/load slot picker - one persistent instance (see SaveLoadBootstrapper), opened from
    // both MainMenuController's Load button and PauseMenuController's Save/Load buttons, so both
    // surfaces show the literal same panel instead of two separate look-alike implementations.
    public sealed class SaveLoadPanelController : MonoBehaviour
    {
        public enum PanelMode { Save, Load }

        public static SaveLoadPanelController Instance { get; private set; }

        // Lets other panels (PauseMenuController) check before drawing themselves, so only one
        // window is ever on screen at a time.
        public bool IsOpen => _container != null && _container.style.display == DisplayStyle.Flex;

        [SerializeField] private UIDocument document;

        private VisualElement _container;
        private Label _title;
        private Label _status;
        private readonly VisualElement[] _slotRoots = new VisualElement[SaveSystem.SlotCount];
        private readonly VisualElement[] _slotIcons = new VisualElement[SaveSystem.SlotCount];
        private readonly Label[] _slotNames = new Label[SaveSystem.SlotCount];
        private readonly Label[] _slotTimestamps = new Label[SaveSystem.SlotCount];

        private PanelMode _mode;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (document == null)
            {
                document = GetComponent<UIDocument>();
            }

            var root = document.rootVisualElement;
            _container = root.Q<VisualElement>("SaveLoadContainer");
            _title = root.Q<Label>("SaveLoadTitle");
            _status = root.Q<Label>("SaveLoadStatus");

            for (var i = 0; i < SaveSystem.SlotCount; i++)
            {
                var slotNumber = i + 1;
                _slotRoots[i] = root.Q<VisualElement>($"Slot{slotNumber}");
                _slotIcons[i] = root.Q<VisualElement>($"Slot{slotNumber}Icon");
                _slotNames[i] = root.Q<Label>($"Slot{slotNumber}Name");
                _slotTimestamps[i] = root.Q<Label>($"Slot{slotNumber}Timestamp");

                var slotIndex = i;
                _slotRoots[i].RegisterCallback<ClickEvent>(_ => HandleSlotClicked(slotIndex));
            }

            var backButton = root.Q<Button>("SaveLoadBackButton");
            if (backButton != null)
            {
                backButton.clicked += Close;
            }

            _container.style.display = DisplayStyle.None;
        }

        public void Open(PanelMode mode)
        {
            _mode = mode;
            _title.text = mode == PanelMode.Save ? "SAVE GAME" : "LOAD GAME";
            _status.text = string.Empty;
            RefreshSlots();
            _container.style.display = DisplayStyle.Flex;
        }

        public void Close()
        {
            _container.style.display = DisplayStyle.None;
        }

        private void RefreshSlots()
        {
            for (var i = 0; i < SaveSystem.SlotCount; i++)
            {
                var info = SaveSystem.GetSlotInfo(i);
                if (!info.Exists)
                {
                    _slotNames[i].text = "Empty";
                    _slotTimestamps[i].text = string.Empty;
                    _slotIcons[i].style.backgroundImage = StyleKeyword.None;
                    continue;
                }

                _slotNames[i].text = info.SceneName;
                _slotTimestamps[i].text = info.Timestamp.ToString("MMM d, yyyy  h:mm tt");
                _slotIcons[i].style.backgroundImage = info.Thumbnail != null
                    ? new StyleBackground(info.Thumbnail)
                    : StyleKeyword.None;
            }
        }

        private void HandleSlotClicked(int slotIndex)
        {
            if (_mode == PanelMode.Save)
            {
                StartCoroutine(SaveWithScreenshot(slotIndex));
                return;
            }

            var info = SaveSystem.GetSlotInfo(slotIndex);
            if (!info.Exists)
            {
                _status.text = "That slot is empty.";
                return;
            }

            Close();
            SaveSystem.Load(slotIndex);
        }

        // Hides both this panel and (via the flag) PauseMenuController for one frame so the thumbnail
        // shows the game world, not whichever menu was open when Save was clicked.
        private IEnumerator SaveWithScreenshot(int slotIndex)
        {
            SaveSystem.IsCapturingScreenshot = true;
            _container.style.display = DisplayStyle.None;

            yield return new WaitForEndOfFrame();

            var screenshot = ScreenCapture.CaptureScreenshotAsTexture();

            SaveSystem.IsCapturingScreenshot = false;
            _container.style.display = DisplayStyle.Flex;

            SaveSystem.Save(slotIndex, screenshot);
            Destroy(screenshot);

            _status.text = "Saved.";
            RefreshSlots();
        }
    }
}
