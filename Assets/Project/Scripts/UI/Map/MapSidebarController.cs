// Assets/Project/Scripts/UI/Map/MapSidebarController.cs
// UI Toolkit map sidebar controller: renders a minimap + location info in the left sidebar.
using UnityEngine;
using UnityEngine.UIElements;
using MyGameNamespace.World;

namespace MyGameNamespace.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MapSidebarController : MonoBehaviour
    {
        [Header("Map Model")]
        public int mapWidth = 10;
        public int mapHeight = 10;
        public Vector2Int startPosition = new Vector2Int(5,5);
        public LocationData defaultLocation;

        [Header("UI Binding (auto if empty)")]
        public string mapContainerName = "SidebarMap";  // container card in sidebar
        public string mapGridName      = "MapGrid";     // grid element where cells go
        public string mapTitleName     = "MapTitle";    // Label
        public string mapCoordsName    = "MapCoords";   // Label
        public string mapImageName     = "MapImage";    // VisualElement (backgroundImage)

        [Header("Behavior")]
        public bool enableKeyboardMovement = true;      // WASD / arrows

        private UIDocument _doc;
        private VisualElement _root;
        private VisualElement _container;
        private VisualElement _grid;
        private Label _title;
        private Label _coords;
        private VisualElement _image;

        private MapService _map;
        private const int ViewRadius = 4; // 9x9

        private void Awake()
        {
            _doc  = GetComponent<UIDocument>();
            _root = _doc.rootVisualElement;

            // Auto-locate likely containers if not found
            _container = _root.Q<VisualElement>(mapContainerName) 
                         ?? _root.Q<VisualElement>("MapCard") 
                         ?? _root.Q<VisualElement>("Map") 
                         ?? CreateFallbackContainer();

            _grid   = _container.Q<VisualElement>(mapGridName)  ?? EnsureChild(_container, mapGridName);
            _title  = _container.Q<Label>(mapTitleName)         ?? EnsureLabel(_container, mapTitleName, "Map");
            _coords = _container.Q<Label>(mapCoordsName)        ?? EnsureLabel(_container, mapCoordsName, "");
            _image  = _container.Q<VisualElement>(mapImageName) ?? EnsureChild(_container, mapImageName);

            // Attach USS if present
            var ss = Resources.Load<StyleSheet>("UI/MapSidebar/MapSidebar");
            if (ss != default) _root.styleSheets.Add(ss);

            // Build map model
            if (defaultLocation == default)
            {
                defaultLocation = ScriptableObject.CreateInstance<LocationData>();
                defaultLocation.locationName = "Unknown";
                defaultLocation.description = "A quiet, uncharted place.";
            }
            _map = new MapService(mapWidth, mapHeight, startPosition, defaultLocation);
            _map.OnLocationChanged += LocChanged;
            _map.OnEventTriggered  += EvTriggered;

            BuildGrid();
            LocChanged(_map.Current);
        }

        private void Update()
        {
            if (!enableKeyboardMovement) return;
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))    TryMove(Vector2Int.up);
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) TryMove(Vector2Int.down);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))TryMove(Vector2Int.right);
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) TryMove(Vector2Int.left);
        }

        private void TryMove(Vector2Int d)
        {
            if (_map.Move(d))
                RefreshGrid();
        }

        private VisualElement CreateFallbackContainer()
        {
            // If your UXML doesn't already have a Map card, we create one and try to insert
            // into a likely sidebar element.
            var sidebar = _root.Q<VisualElement>("Sidebar") ?? _root.Q<VisualElement>("LeftSidebar") ?? _root;
            var card = new VisualElement { name = mapContainerName };
            card.AddToClassList("map-card");
            sidebar.Add(card);
            return card;
        }

        private VisualElement EnsureChild(VisualElement parent, string name)
        {
            var ve = new VisualElement { name = name };
            parent.Add(ve);
            return ve;
        }
        private Label EnsureLabel(VisualElement parent, string name, string text)
        {
            var l = new Label(text) { name = name };
            parent.Add(l);
            return l;
        }

        private void BuildGrid()
        {
            _grid.Clear();
            _grid.AddToClassList("map-grid");
            int size = ViewRadius*2 + 1;
            for (int y = 0; y < size; y++)
            {
                var row = new VisualElement();
                row.AddToClassList("map-row");
                for (int x = 0; x < size; x++)
                {
                    var cell = new VisualElement();
                    cell.AddToClassList("map-cell");
                    row.Add(cell);
                }
                _grid.Add(row);
            }
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            int size = ViewRadius*2 + 1;
            Vector2Int center = _map.PlayerPosition;
            int rowIndex = 0;
            foreach (var row in _grid.Children())
            {
                int colIndex = 0;
                foreach (var cell in row.Children())
                {
                    int dx = colIndex - ViewRadius;
                    int dy = ViewRadius - rowIndex;
                    Vector2Int pos = new Vector2Int(center.x + dx, center.y + dy);
                    bool outOf = (pos.x < 0 || pos.x >= _map.Width || pos.y < 0 || pos.y >= _map.Height);
                    cell.EnableInClassList("is-current", pos == center);
                    cell.EnableInClassList("is-out", outOf);
                    colIndex++;
                }
                rowIndex++;
            }

            var loc = _map.Current;
            if (_title != default) _title.text = string.IsNullOrEmpty(loc.locationName) ? "Unknown" : loc.locationName;
            if (_coords != default) _coords.text = $"({center.x}, {center.y})";
            if (_image != default && loc.locationImage != default)
                _image.style.backgroundImage = new StyleBackground(loc.locationImage);
        }

        private void LocChanged(LocationData loc) => RefreshGrid();
        private void EvTriggered(LocationEvent ev) => Debug.Log($"[Map] Event: {ev.eventName} — {ev.eventDescription}");

        // Public hooks if other systems need to move
        public void MoveNorth() => TryMove(Vector2Int.up);
        public void MoveSouth() => TryMove(Vector2Int.down);
        public void MoveEast()  => TryMove(Vector2Int.right);
        public void MoveWest()  => TryMove(Vector2Int.left);
        public Vector2Int GetPlayerPosition() => _map.PlayerPosition;
    }
}
