using StardewValley;
using StardewValley.Triggers;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace CapeStardewCode
{
    internal static class RingTriggers
    {
        private static IMonitor? Monitor;
        private static IModHelper? Helper;

        internal static void Initialize(IMonitor monitor, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;

            // Subscribe to necessary events
            Helper.Events.Player.InventoryChanged += OnInventoryChanged;
        }

        private static void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            // Check if the player unequipped any active rings
            foreach (var removedItem in e.Removed)
            {
                if (removedItem is StardewValley.Objects.Ring ring && IsTargetRing(ring))
                {
                    // Raise the custom trigger when a target ring is unequipped
                    TriggerActionManager.Raise("dreamy.kickitspot_UnequippedActiveRings", new[] { ring });
                    Monitor?.Log($"Triggered UnequippedActiveRings for {ring.DisplayName}.", LogLevel.Info);
                }
            }

            // Check if the player equipped any rings (for off-trigger)
            foreach (var addedItem in e.Added)
            {
                if (addedItem is StardewValley.Objects.Ring ring && IsTargetRing(ring))
                {
                    // Raise the custom trigger when a target ring is equipped
                    TriggerActionManager.Raise("dreamy.kickitspot_UnequippedActiveRings", new[] { ring });
                    Monitor?.Log($"Triggered UnequippedActiveRings for {ring.DisplayName}.", LogLevel.Info);
                }
            }
        }

        private static bool IsTargetRing(StardewValley.Objects.Ring ring)
        {
            var contextTags = ring.GetContextTags();
            return contextTags.IsSupersetOf(new[] { "item_ring", "color_blue", "armor_ring" })
                || contextTags.IsSupersetOf(new[] { "item_ring", "color_pink", "armor_ring" })
                || contextTags.IsSupersetOf(new[] { "item_ring", "color_cyan", "armor_ring" })
                || contextTags.IsSupersetOf(new[] { "item_ring", "color_red", "armor_ring" });
        }
    }
}
