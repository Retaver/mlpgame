// Assets/Project/Scripts/World/LocationData.cs
using UnityEngine;
using System.Collections.Generic;

namespace MyGameNamespace.World
{
    [CreateAssetMenu(fileName = "New Location", menuName = "Map System/Location Data")]
    public class LocationData : ScriptableObject
    {
        [Header("Basic Info")]
        public string locationName;
        [TextArea(3, 5)] public string description;
        public Sprite locationImage;

        [Header("Navigation")]
        public bool canMoveNorth = true;
        public bool canMoveSouth = true;
        public bool canMoveEast  = true;
        public bool canMoveWest  = true;

        [Header("Special Features")]
        public bool hasShop;
        public bool hasInn;
        public bool isRestArea;

        [Header("Events")]
        public List<LocationEvent> possibleEvents = new List<LocationEvent>();

        [Header("Characters")]
        public List<string> charactersPresent = new List<string>();
    }

    [System.Serializable]
    public class LocationEvent
    {
        public string eventName;
        [TextArea(2,4)] public string eventDescription;
        [Range(0,1)] public float triggerChance = 0.1f;
        public bool isOneTime = false;
        public bool hasTriggered = false;
        public EventType eventType;
    }

    public enum EventType { Encounter, ItemFind, CharacterMeeting, StoryEvent, RandomEvent }
}
