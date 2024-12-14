using StardewValley;
using StardewValley.Objects;
using StardewModdingAPI;
using StardewValley.Triggers;
using System;
using StardewValley.Buffs;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using HarmonyLib;
using System.Collections.Generic;

namespace CapeStardewCode
{
    internal static class ItemPatches
    {
        private static IMonitor? Monitor;
        private static IModHelper? Helper;

        internal static void Initialize(IMonitor monitor, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;

            // Subscribe to the DayStarted event
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

         internal static void ApplyRingBuffsOnNewDay()
        {
            try
            {
                Farmer player = Game1.player;

                if (player == null)
                {
                    Monitor?.Log("Player is null; cannot apply buffs.", LogLevel.Warn);
                    return;
                }

                ApplyBuffForRing(player.leftRing.Value, player);
                ApplyBuffForRing(player.rightRing.Value, player);
            }
            catch (Exception ex)
            {
                Monitor?.Log($"Error applying ring buffs on new day: {ex}", LogLevel.Error);
            }
        }

        private static void ApplyBuffForRing(Ring? ring, Farmer who)
        {
            if (ring == null) return;

            try
            {
                var contextTags = ring.GetContextTags();
                string? buffId = GetBuffIdForRing(contextTags);

                if (buffId != null)
                {
                    who.buffs.Remove(buffId); // Remove existing buff to prevent duplication
                    who.applyBuff(buffId);    // Apply the buff again
                    Monitor?.Log($"Reapplied buff {buffId} for ring {ring.DisplayName}.", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                Monitor?.Log($"Error applying buff for ring {ring.DisplayName}: {ex}", LogLevel.Error);
            }
        }

        private static string? GetBuffIdForRing(ISet<string> contextTags)
        {
            if (contextTags.IsSupersetOf(new[] { "item_ring", "color_blue", "armor_ring" }))
                return "dreamy.kickitspot_OakBlessing";

            if (contextTags.IsSupersetOf(new[] { "item_ring", "color_pink", "armor_ring" }))
                return "dreamy.kickitspot_PetalGrace";

            if (contextTags.IsSupersetOf(new[] { "item_ring", "color_cyan", "armor_ring" }))
                return "dreamy.kickitspot_ArcaneCharm";

            if (contextTags.IsSupersetOf(new[] { "item_ring", "color_red", "armor_ring" }))
                return "dreamy.kickitspot_AshenPromise";

            return null;
        }

        private static void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            ApplyRingBuffsOnNewDay();
        }

        internal static void Post_onRingEquip(Ring __instance, Farmer who)
        {
            try
            {
                var contextTags = __instance.GetContextTags();
                string? buffId = GetBuffIdForRing(contextTags);

                if (buffId != null)
                {
                    who.applyBuff(buffId);
                    Monitor?.Log($"Applied buff {buffId} for ring {__instance.DisplayName}.", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                Monitor?.Log($"Failed in Post_onRingEquip:\n{ex}", LogLevel.Error);
            }
        }

        internal static void Post_onRingUnequip(Ring __instance, Farmer who)
        {
            try
            {
                var contextTags = __instance.GetContextTags();
                string? buffId = GetBuffIdForRing(contextTags);

                if (buffId != null)
                {
                    who.buffs.Remove(buffId);
                    Monitor?.Log($"Removed buff {buffId} for ring {__instance.DisplayName}.", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                Monitor?.Log($"Failed in Post_onRingUnequip:\n{ex}", LogLevel.Error);
            }
        }

// Ensure this method is in the right class and registered correctly


[HarmonyPatch(typeof(Ring), nameof(Ring.AddEquipmentEffects))]
public static class Ring_AddEquipmentEffects_Patch
{ 
            public static void Postfix(Ring __instance, Farmer who)
            {
                try
                {
                    var contextTags = __instance.GetContextTags();
                    string? buffId = GetBuffIdForRing(contextTags);

                    if (buffId != null)
                    {
                        who.applyBuff(buffId);
                        Monitor?.Log($"Applied buff {buffId} via AddEquipmentEffects for ring {__instance.DisplayName}.", LogLevel.Info);
                    }
                }
                catch (Exception ex)
                {
                    Monitor?.Log($"Error in Ring.AddEquipmentEffects postfix:\n{ex}", LogLevel.Error);
                }
            }
        }
    
}
        
    
   
    }



    