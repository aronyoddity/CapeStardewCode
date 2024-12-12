using StardewValley;
using StardewValley.Objects;
using StardewModdingAPI;
using StardewValley.Triggers;
using System;
using StardewValley.Buffs;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using HarmonyLib;

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

                // Reapply buffs based on equipped rings
                if (player.leftRing.Value != null)
                {
                    ApplyBuffForRing(player.leftRing.Value, player);
                }

                if (player.rightRing.Value != null)
                {
                    ApplyBuffForRing(player.rightRing.Value, player);
                }
            }
            catch (Exception ex)
            {
                Monitor?.Log($"Error applying ring buffs on new day: {ex}", LogLevel.Error);
            }
        }

        private static void ApplyBuffForRing(Ring ring, Farmer who)
{
    try
    {
        // Get context tags to identify the ring type
        var contextTags = ring.GetContextTags();
        string? buffId = null;

        // Define buffs based on ring context tags
        if (contextTags.Contains("item_ring,color_blue,armor_ring"))
        {
            buffId = "dreamy.kickitspot_OakBlessing";
        }
        else if (contextTags.Contains("item_ring,color_pink,armor_ring"))
        {
            buffId = "dreamy.kickitspot_PetalGrace";
        }
        else if (contextTags.Contains("item_ring,color_cyan,armor_ring"))
        {
            buffId = "dreamy.kickitspot_ArcaneCharm";
        }
        else if (contextTags.Contains("item_ring,color_red,armor_ring"))
        {
            buffId = "dreamy.kickitspot_AshenPromise";
        }

        // Apply buff if identified
        if (buffId != null)
        {
            // Remove existing buff to prevent duplication
            who.buffs.Remove(buffId);

            // Apply the buff again
            who.applyBuff(buffId);
            Monitor?.Log($"Reapplied buff {buffId} for ring {ring.DisplayName}.", LogLevel.Info);
        }
    }
    catch (Exception ex)
    {
        Monitor?.Log($"Error applying buff for ring {ring.DisplayName}: {ex}", LogLevel.Error);
    }
}

private static void OnDayStarted(object? sender, DayStartedEventArgs e)
{
    try
    {
        Farmer player = Game1.player;

        if (player == null)
        {
            Monitor?.Log("Player is null on new day start.", LogLevel.Warn);
            return;
        }

        // Apply buffs for left and right rings
        if (player.leftRing.Value != null)
        {
            ApplyBuffForRing(player.leftRing.Value, player);
        }

        if (player.rightRing.Value != null)
        {
            ApplyBuffForRing(player.rightRing.Value, player);
        }

        Monitor?.Log("Reapplied buffs for equipped rings on new day.", LogLevel.Info);
    }
    catch (Exception ex)
    {
        Monitor?.Log($"Error in OnDayStarted: {ex}", LogLevel.Error);
    }
}


        internal static void Post_onRingEquip(Ring __instance, Farmer who)
        {

            try
            {
                var contextTags = __instance.GetContextTags();
                string? buffId = null;

                if (contextTags.Contains("item_ring,color_blue,armor_ring"))
                {
                    TriggerActionManager.Raise(ModEntry.Id + "_OnRingSkyOakEquip");
                    buffId = "dreamy.kickitspot_OakBlessing";
                }
                else if (contextTags.Contains("item_ring,color_pink,armor_ring"))
                {
                    TriggerActionManager.Raise(ModEntry.Id + "_OnRingSilverPetalEquip");
                    buffId = "dreamy.kickitspot_PetalGrace";
                }
                else if (contextTags.Contains("item_ring,color_cyan,armor_ring"))
                {
                    TriggerActionManager.Raise(ModEntry.Id + "_OnRingMysticArcanaEquip");
                    buffId = "dreamy.kickitspot_ArcaneCharm";
                }
else if (contextTags.Contains("item_ring,color_red,armor_ring"))
                {
                    TriggerActionManager.Raise(ModEntry.Id + "_OnRingEndlessEmberEquip");
                    buffId = "dreamy.kickitspot_AshenPromise";
                      // Call method to apply buffs, including critical chance multiplier and Ashen Promise
                     
                }

                if (buffId != null)
                {
                    Game1.player.applyBuff(buffId);
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
                string? buffId = null;

                if (contextTags.Contains("item_ring,color_blue,armor_ring"))
                {
                    TriggerActionManager.Raise(ModEntry.Id + "_OnRingSkyOakUnequip");
                    buffId = "dreamy.kickitspot_OakBlessing";
                }
                else if (contextTags.Contains("item_ring,color_pink,armor_ring"))
                {
                    TriggerActionManager.Raise(ModEntry.Id + "_OnRingSilverPetalUnequip");
                    buffId = "dreamy.kickitspot_PetalGrace";
                }
                else if (contextTags.Contains("item_ring,color_cyan,armor_ring"))
                {
                    TriggerActionManager.Raise(ModEntry.Id + "_OnRingMysticArcanaUnequip");
                    buffId = "dreamy.kickitspot_ArcaneCharm";
                }

 else if (contextTags.Contains("item_ring,color_red,armor_ring"))
                {
                    TriggerActionManager.Raise(ModEntry.Id + "_OnRingEndlessEmberUnequip");
                    buffId = "dreamy.kickitspot_AshenPromise";
                }

                if (buffId != null)
                {
                    Game1.player.buffs.Remove(buffId);
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
            string? buffId = null;

            if (contextTags.Contains("item_ring,color_blue,armor_ring"))
                buffId = "dreamy.kickitspot_OakBlessing";
            else if (contextTags.Contains("item_ring,color_pink,armor_ring"))
                buffId = "dreamy.kickitspot_PetalGrace";
            // Add more conditions for other rings...

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



    