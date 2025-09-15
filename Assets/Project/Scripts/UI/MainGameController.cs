
using UnityEngine;
using UnityEngine.UIElements;
using MyGameNamespace;

[DisallowMultipleComponent]
public class MainGameController : MonoBehaviour
{
    [SerializeField] private UIDocument gameUIDocument;

    private VisualElement root;
    private Label storyText;
    private VisualElement choicesPanel;

    private void Awake()
    {
        if (gameUIDocument == default)
            gameUIDocument = GetComponent<UIDocument>();

        root = gameUIDocument ? gameUIDocument.rootVisualElement : null;
        if (root == default)
        {
            Debug.LogWarning("[MainGameController] No UIDocument / rootVisualElement found.");
            return;
        }

        storyText = root.Q<Label>("StoryText");
        choicesPanel = root.Q<VisualElement>("ChoicesPanel");

        // Try to bind to GameManager player/story
        var gm = GameManager.Instance ?? FindFirstObjectByType<GameManager>();
        if (gm != default)
        {
            var player = gm.GetPlayer(); // <-- use method; not a property
            // Fallback to PlayerState.Current if GameManager has no player yet
            if (player == default)
            {
                player = MyGameNamespace.PlayerState.Current;
            }
            if (gm.StoryManager != default && player != default)
            {
                gm.StoryManager.SetPlayer(player);
            }
        }
    }
}
