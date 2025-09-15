// Assets/Project/Scripts/World/MapService.cs
using UnityEngine;
using System;
using System.Collections.Generic;

namespace MyGameNamespace.World
{
    /// <summary>
    /// Logic-only map model/service (no UI). Holds grid, player position, movement & events.
    /// </summary>
    public class MapService
    {
        public int Width  { get; private set; }
        public int Height { get; private set; }
        public Vector2Int PlayerPosition { get; private set; }

        private LocationData[,] _grid;
        private readonly System.Random _rng = new System.Random();

        public event Action<LocationData> OnLocationChanged;
        public event Action<LocationEvent> OnEventTriggered;

        public MapService(int width, int height, Vector2Int start, LocationData defaultLocation)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            _grid = new LocationData[Width, Height];

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    _grid[x,y] = defaultLocation;

            PlayerPosition = Clamp(start);
        }

        public void SetLocation(Vector2Int pos, LocationData data)
        {
            if (!InBounds(pos) || data == default) return;
            _grid[pos.x, pos.y] = data;
            if (pos == PlayerPosition)
                OnLocationChanged?.Invoke(data);
        }

        public LocationData GetLocation(Vector2Int pos)
        {
            if (!InBounds(pos)) return _grid[PlayerPosition.x, PlayerPosition.y];
            return _grid[pos.x, pos.y];
        }

        public LocationData Current => GetLocation(PlayerPosition);

        public bool Move(Vector2Int delta)
        {
            var target = PlayerPosition + delta;
            if (!InBounds(target)) return false;

            // Movement gates from current tile
            if (!CanMoveFrom(Current, delta)) return false;

            PlayerPosition = target;
            var loc = Current;
            OnLocationChanged?.Invoke(loc);
            MaybeTriggerEvent(loc);
            return true;
        }

        public void Teleport(Vector2Int pos)
        {
            PlayerPosition = Clamp(pos);
            OnLocationChanged?.Invoke(Current);
        }

        private bool CanMoveFrom(LocationData from, Vector2Int dir)
        {
            if (dir == Vector2Int.up)    return from.canMoveNorth;
            if (dir == Vector2Int.down)  return from.canMoveSouth;
            if (dir == Vector2Int.right) return from.canMoveEast;
            if (dir == Vector2Int.left)  return from.canMoveWest;
            return false;
        }

        private void MaybeTriggerEvent(LocationData loc)
        {
            if (loc == default || loc.possibleEvents == default) return;
            foreach (var ev in loc.possibleEvents)
            {
                if (ev.isOneTime && ev.hasTriggered) continue;
                // Random chance
                var roll = (float)new System.Random().NextDouble();
                if (roll < Mathf.Clamp01(ev.triggerChance))
                {
                    ev.hasTriggered = true;
                    OnEventTriggered?.Invoke(ev);
                    break;
                }
            }
        }

        private bool InBounds(Vector2Int p) => p.x >= 0 && p.x < Width && p.y >= 0 && p.y < Height;
        private Vector2Int Clamp(Vector2Int p) => new Vector2Int(Mathf.Clamp(p.x, 0, Width-1), Mathf.Clamp(p.y, 0, Height-1));
    }
}
