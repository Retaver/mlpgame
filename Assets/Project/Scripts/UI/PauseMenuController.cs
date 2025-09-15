using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace MyGameNamespace
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument pauseDocument;
        [Header("Overlay order (int)")]
        [SerializeField] private int desiredSortingOrder = 200;

        private VisualElement root, panel;
        private Button closeBtn;

        // Optional mapped buttons
        private Button resumeBtn;
        private Button optionsBtn;
        private Button saveBtn;
        private Button loadBtn;
        private Button mainMenuBtn;
        private Button quitBtn;

        // Events for other systems (save/load/menu/quit)
        public static Action SaveRequested;
        public static Action LoadRequested;
        public static Action ReturnToMainMenuRequested;
        public static Action QuitRequested;

        private static bool _openOnNextSceneScheduled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _openOnNextSceneScheduled = false;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_openOnNextSceneScheduled)
            {
                _openOnNextSceneScheduled = false;
                var inst = FindFirstInstance();
                if (inst != default) inst.ShowPauseMenu();
            }
        }

        public static void ScheduleOpenOnNextScene() => _openOnNextSceneScheduled = true;

        public static void OpenPauseMenu()
        {
            var inst = FindFirstInstance();
            if (inst != default) inst.ShowPauseMenu();
            else Debug.LogWarning("[PauseMenuController] No instance found to open pause menu.");
        }

        private static PauseMenuController FindFirstInstance()
        {
            var inst = UnityEngine.Object.FindFirstObjectByType<PauseMenuController>();
            if (inst == default) inst = UnityEngine.Object.FindAnyObjectByType<PauseMenuController>();
            return inst;
        }

        private void Awake()
        {
            if (pauseDocument == default) pauseDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            root = pauseDocument != default ? pauseDocument.rootVisualElement : null;
            if (root == default)
            {
                // Try to find any UIDocument in the scene that contains known pause menu buttons
                var docs = UnityEngine.Object.FindObjectsOfType<UIDocument>(FindObjectsSortMode.None);
                foreach (var d in docs)
                {
                    if (d == default || d.rootVisualElement == default) continue;
                    // quick heuristic: does this document contain a PauseResume/Resume button?
                    var any = d.rootVisualElement.Query<Button>().ToList();
                    if (any.Exists(b => (b.name != null && (b.name.Equals("PauseResume", StringComparison.OrdinalIgnoreCase) || b.name.Equals("Resume", StringComparison.OrdinalIgnoreCase))) ||
                                        (!string.IsNullOrEmpty(b.text) && b.text.ToLowerInvariant().Contains("resume"))))
                    {
                        pauseDocument = d;
                        root = d.rootVisualElement;
                        if (verboseLogging) Debug.Log("[PauseMenuController] Found UIDocument with pause controls in scene.");
                        break;
                    }
                }
            }
            if (root == default)
            {
                Debug.LogWarning("[PauseMenuController] Missing UIDocument/root.");
                return;
            }

            panel = root.Q<VisualElement>("PauseMenu")
                  ?? root.Q<VisualElement>("PauseMenuRoot")
                  ?? root;

            closeBtn = root.Q<Button>("PauseClose")
                     ?? root.Q<Button>("pause-close")
                     ?? root.Q<Button>("CloseButton")
                     ?? root.Q<Button>("close-button");
            if (closeBtn != default)
            {
                closeBtn.clicked -= HidePauseMenu;
                closeBtn.clicked += HidePauseMenu;
            }

            WireButtons();
            HidePauseMenu();  // start hidden
        }

        private void ForceTopMost()
        {
            try
            {
                if (root != default) root.pickingMode = PickingMode.Position;
                if (panel != default) panel.pickingMode = PickingMode.Position;
                panel?.BringToFront();

                var ps = pauseDocument != default ? pauseDocument.panelSettings : null;
                if (ps != default)
                {
                    bool shared = false;
                    int maxOrder = 0;

                    var docs = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
                    foreach (var d in docs)
                    {
                        if (d == default || d.panelSettings == default) continue;

                        if (d != pauseDocument && d.panelSettings == ps) shared = true;

                        int so;
                        try { so = (int)d.panelSettings.sortingOrder; } catch { so = 0; }
                        if (so > maxOrder) maxOrder = so;
                    }

                    int targetOrder = desiredSortingOrder;
                    int candidate = maxOrder + 10;
                    if (candidate > targetOrder) targetOrder = candidate;

                    if (shared)
                    {
                        var clone = ScriptableObject.Instantiate(ps);
                        clone.sortingOrder = targetOrder;      // int → int
                        pauseDocument.panelSettings = clone;
                    }
                    else if (ps.sortingOrder < targetOrder)
                    {
                        ps.sortingOrder = targetOrder;         // int → int
                    }
                }
            }
            catch { /* noop */ }
        }

        public void ShowPauseMenu()
        {
            if (panel == default) return;
            panel.style.display = DisplayStyle.Flex;
            ForceTopMost();
        }

        public void HidePauseMenu()
        {
            if (panel == default) return;
            panel.style.display = DisplayStyle.None;
        }

        public void ShowOptions()
        {
            ShowPauseMenu();
            var optionsTab = root.Q<Button>("OptionsTab") ?? root.Q<Button>("options-tab");
            optionsTab?.Focus();
        }

        public void OpenPauseMenuInstance() => ShowPauseMenu();

        // ---- Button finding & wiring ----
        private Button FindByIdOrClassOrText(VisualElement rootElement, string[] ids, string[] classes, string[] texts)
        {
            if (rootElement == default) return null;

            if (ids != default)
            {
                foreach (var id in ids)
                {
                    var b = rootElement.Q<Button>(id);
                    if (b != default) return b;
                }
            }
            if (classes != default)
            {
                foreach (var cls in classes)
                {
                    var b = rootElement.Q<Button>(className: cls);
                    if (b != default) return b;
                }
            }
            if (texts != default)
            {
                var all = rootElement.Query<Button>().ToList();
                foreach (var t in texts)
                {
                    var low = t.ToLowerInvariant();
                    var byText = all.Find(b => !string.IsNullOrEmpty(b.text) && b.text.ToLowerInvariant().Contains(low));
                    if (byText != default) return byText;
                }
            }
            return null;
        }

        private void WireButtons()
        {
            resumeBtn = FindByIdOrClassOrText(root,
                            new[] { "PauseResume", "ResumeButton" },
                            new[] { "pause-resume", "resume-button" },
                            new[] { "resume", "continue" });

            optionsBtn = FindByIdOrClassOrText(root,
                            new[] { "OptionsButton", "PauseOptions" },
                            new[] { "options-button", "pause-options" },
                            new[] { "options" });

            saveBtn = FindByIdOrClassOrText(root,
                            new[] { "SaveButton", "PauseSave" },
                            new[] { "save-button", "pause-save" },
                            new[] { "save" });

            loadBtn = FindByIdOrClassOrText(root,
                            new[] { "LoadButton", "PauseLoad" },
                            new[] { "load-button", "pause-load" },
                            new[] { "load" });

            mainMenuBtn = FindByIdOrClassOrText(root,
                            new[] { "MainMenuButton", "PauseMainMenu" },
                            new[] { "main-menu-button", "pause-mainmenu" },
                            new[] { "main menu", "title" });

            quitBtn = FindByIdOrClassOrText(root,
                            new[] { "QuitButton", "PauseQuit" },
                            new[] { "quit-button", "pause-quit" },
                            new[] { "quit", "exit" });

            if (resumeBtn != default) { resumeBtn.clicked -= OnResume; resumeBtn.clicked += OnResume; }
            if (optionsBtn != default) { optionsBtn.clicked -= OnOptions; optionsBtn.clicked += OnOptions; }
            if (saveBtn != default) { saveBtn.clicked -= OnSave; saveBtn.clicked += OnSave; }
            if (loadBtn != default) { loadBtn.clicked -= OnLoad; loadBtn.clicked += OnLoad; }
            if (mainMenuBtn != default) { mainMenuBtn.clicked -= OnMainMenu; mainMenuBtn.clicked += OnMainMenu; }
            if (quitBtn != default) { quitBtn.clicked -= OnQuit; quitBtn.clicked += OnQuit; }
        }

        private void OnResume() { HidePauseMenu(); Debug.Log("[PauseMenu] Resume clicked -> HidePauseMenu"); }
        private void OnOptions() { ShowOptions(); Debug.Log("[PauseMenu] Options clicked -> ShowOptions"); }
        private void OnSave() { SaveRequested?.Invoke(); Debug.Log("[PauseMenu] Save clicked -> SaveRequested"); }
        private void OnLoad() { LoadRequested?.Invoke(); Debug.Log("[PauseMenu] Load clicked -> LoadRequested"); }
        private void OnMainMenu() { ReturnToMainMenuRequested?.Invoke(); Debug.Log("[PauseMenu] Main Menu clicked"); }
        private void OnQuit()
        {
            QuitRequested?.Invoke();
            Debug.Log("[PauseMenu] Quit clicked -> QuitRequested");
#if !UNITY_EDITOR
            Application.Quit();
#endif
        }
    }
}
