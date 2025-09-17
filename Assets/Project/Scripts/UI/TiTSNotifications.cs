using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

namespace MyGameNamespace.UI
{
    /// <summary>
    /// TiTS-inspired notification system with slide-in animations and auto-dismiss
    /// Features: Multiple notification types, queue system, customizable duration
    /// </summary>
    public class TiTSNotifications : VisualElement
    {
        public enum NotificationType
        {
            Info,
            Success,
            Warning,
            Error,
            Achievement,
            Quest,
            LevelUp,
            Item
        }

        private class NotificationData
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public NotificationType Type { get; set; }
            public float Duration { get; set; }
            public string Icon { get; set; }
        }

        // Notification queue and display
        private readonly Queue<NotificationData> notificationQueue = new();
        private readonly List<VisualElement> activeNotifications = new();
        private Coroutine displayCoroutine;

        // Settings
        private const float DEFAULT_DURATION = 3.0f;
        private const float SLIDE_DURATION = 0.5f;
        private const float NOTIFICATION_HEIGHT = 80f;
        private const float NOTIFICATION_SPACING = 10f;
        private const int MAX_VISIBLE_NOTIFICATIONS = 5;

        public TiTSNotifications()
        {
            AddToClassList("tits-notifications");
            style.position = Position.Absolute;
            style.top = 20;
            style.right = 20;
            style.width = 350;
            style.flexDirection = FlexDirection.Column;
            style.alignItems = Align.FlexEnd;
        }

        /// <summary>
        /// Show a notification with the specified parameters
        /// </summary>
        public void ShowNotification(string title, string message,
                                   NotificationType type = NotificationType.Info,
                                   float duration = DEFAULT_DURATION,
                                   string customIcon = null)
        {
            var notification = new NotificationData
            {
                Title = title,
                Message = message,
                Type = type,
                Duration = duration,
                Icon = customIcon ?? GetDefaultIcon(type)
            };

            notificationQueue.Enqueue(notification);

            if (displayCoroutine == null)
            {
                ProcessNotificationQueue();
            }
        }

        /// <summary>
        /// Show a success notification
        /// </summary>
        public void ShowSuccess(string title, string message, float duration = DEFAULT_DURATION)
        {
            ShowNotification(title, message, NotificationType.Success, duration);
        }

        /// <summary>
        /// Show an error notification
        /// </summary>
        public void ShowError(string title, string message, float duration = DEFAULT_DURATION)
        {
            ShowNotification(title, message, NotificationType.Error, duration);
        }

        /// <summary>
        /// Show a warning notification
        /// </summary>
        public void ShowWarning(string title, string message, float duration = DEFAULT_DURATION)
        {
            ShowNotification(title, message, NotificationType.Warning, duration);
        }

        /// <summary>
        /// Show an achievement notification
        /// </summary>
        public void ShowAchievement(string achievementName, string description)
        {
            ShowNotification($"🏆 {achievementName}", description,
                           NotificationType.Achievement, 5.0f);
        }

        /// <summary>
        /// Show a quest notification
        /// </summary>
        public void ShowQuestUpdate(string questName, string update)
        {
            ShowNotification($"📜 {questName}", update,
                           NotificationType.Quest, 4.0f);
        }

        /// <summary>
        /// Show a level up notification
        /// </summary>
        public void ShowLevelUp(int newLevel, string bonus = null)
        {
            string message = $"Reached level {newLevel}!";
            if (!string.IsNullOrEmpty(bonus))
                message += $"\n{bonus}";

            ShowNotification("⬆️ Level Up!", message,
                           NotificationType.LevelUp, 6.0f);
        }

        /// <summary>
        /// Show an item received notification
        /// </summary>
        public void ShowItemReceived(string itemName, int quantity = 1)
        {
            string message = quantity > 1 ? $"Received {quantity}x {itemName}" : $"Received {itemName}";
            ShowNotification("🎁 Item Received", message,
                           NotificationType.Item, 3.0f);
        }

        private string GetDefaultIcon(NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => "ℹ️",
                NotificationType.Success => "✅",
                NotificationType.Warning => "⚠️",
                NotificationType.Error => "❌",
                NotificationType.Achievement => "🏆",
                NotificationType.Quest => "📜",
                NotificationType.LevelUp => "⬆️",
                NotificationType.Item => "🎁",
                _ => "ℹ️"
            };
        }

        private void ProcessNotificationQueue()
        {
            // Show notifications up to the maximum
            while (notificationQueue.Count > 0 && activeNotifications.Count < MAX_VISIBLE_NOTIFICATIONS)
            {
                var notificationData = notificationQueue.Dequeue();
                var notificationElement = CreateNotificationElement(notificationData);
                ShowNotificationElement(notificationElement, notificationData.Duration);
            }

            displayCoroutine = null;
        }

        private VisualElement CreateNotificationElement(NotificationData data)
        {
            var notification = new VisualElement();
            notification.AddToClassList("tits-notification");
            notification.AddToClassList($"tits-notification-{data.Type.ToString().ToLower()}");
            notification.style.height = NOTIFICATION_HEIGHT;
            notification.style.opacity = 0;
            notification.style.translate = new Translate(new Length(400, LengthUnit.Pixel), 0);

            // Icon
            var icon = new Label(data.Icon);
            icon.AddToClassList("tits-notification-icon");
            notification.Add(icon);

            // Content
            var content = new VisualElement();
            content.AddToClassList("tits-notification-content");
            notification.Add(content);

            // Title
            var title = new Label(data.Title);
            title.AddToClassList("tits-notification-title");
            content.Add(title);

            // Message
            var message = new Label(data.Message);
            message.AddToClassList("tits-notification-message");
            content.Add(message);

            // Close button
            var closeButton = new Button(() => DismissNotification(notification));
            closeButton.AddToClassList("tits-notification-close");
            closeButton.text = "×";
            notification.Add(closeButton);

            return notification;
        }

        private void ShowNotificationElement(VisualElement notification, float duration)
        {
            Add(notification);
            activeNotifications.Add(notification);

            // Position the notification
            UpdateNotificationPositions();

            // Animate in
            AnimateNotificationIn(notification, duration);
        }

        private void AnimateNotificationIn(VisualElement notification, float duration)
        {
            // Set initial state
            notification.style.opacity = 0;
            notification.style.translate = new Translate(new Length(400, LengthUnit.Pixel), 0);

            // Animate in using UIToolkit experimental animation
            notification.experimental.animation.Start(0f, 1f, SLIDE_DURATION * 1000f, (element, value) =>
            {
                element.style.opacity = new StyleFloat(value);
                element.style.translate = LerpTranslate(
                    new Translate(new Length(400, LengthUnit.Pixel), 0),
                    new Translate(0, 0),
                    value);
            }).OnCompleted(() =>
            {
                // Animation complete, wait for duration then animate out
                notification.schedule.Execute(() =>
                {
                    AnimateNotificationOut(notification);
                }).ExecuteLater((long)(duration * 1000));
            });
        }

        private void AnimateNotificationOut(VisualElement notification)
        {
            // Animate out using UIToolkit experimental animation
            notification.experimental.animation.Start(0f, 1f, SLIDE_DURATION * 1000f, (element, value) =>
            {
                element.style.opacity = new StyleFloat(1f - value);
                element.style.translate = LerpTranslate(
                    new Translate(0, 0),
                    new Translate(new Length(400, LengthUnit.Pixel), 0),
                    value);
            }).OnCompleted(() =>
            {
                // Animation complete, remove notification
                DismissNotification(notification);
            });
        }
        private void DismissNotification(VisualElement notification)
        {
            if (activeNotifications.Contains(notification))
            {
                activeNotifications.Remove(notification);
                Remove(notification);
                UpdateNotificationPositions();
            }
        }

        private void UpdateNotificationPositions()
        {
            for (int i = 0; i < activeNotifications.Count; i++)
            {
                var notification = activeNotifications[i];
                float yOffset = i * (NOTIFICATION_HEIGHT + NOTIFICATION_SPACING);
                notification.style.translate = new Translate(0, new Length(yOffset, LengthUnit.Pixel));
            }
        }

        private Translate LerpTranslate(Translate a, Translate b, float t)
        {
            float x = Mathf.Lerp(a.x.value, b.x.value, t);
            float y = Mathf.Lerp(a.y.value, b.y.value, t);
            return new Translate(new Length(x, a.x.unit), new Length(y, a.y.unit));
        }

        /// <summary>
        /// Clear all notifications
        /// </summary>
        public void ClearAll()
        {
            notificationQueue.Clear();
            foreach (var notification in activeNotifications.ToArray())
            {
                DismissNotification(notification);
            }
        }

        /// <summary>
        /// Get the number of pending notifications
        /// </summary>
        public int PendingCount => notificationQueue.Count;

        /// <summary>
        /// Get the number of active notifications
        /// </summary>
        public int ActiveCount => activeNotifications.Count;
    }

    public static class TiTSNotificationsExtensions
    {
        public static TiTSNotifications AddTiTSNotifications(this VisualElement parent)
        {
            var notifications = new TiTSNotifications();
            parent.Add(notifications);
            return notifications;
        }
    }
}