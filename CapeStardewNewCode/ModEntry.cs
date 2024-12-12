using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Triggers;
using System.Xml.Serialization;
using SpaceShared.APIs;
using System.Diagnostics;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using System.Collections.Generic;
using xTile.Dimensions;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace CapeStardewCode
{



    [XmlType($"Mods_{nameof(CustomLavaLogic)}")]
[XmlInclude(typeof(CustomLavaLogic))]

    public class ModEntry : Mod
    {
        
 private BusTravelHandler? busTravelHandler;
 private Vector2? teleportLocation;
        private string? teleportMap;
        private Texture2D? portalTexture;
        private Texture2D? orbBigTexture;
        private Texture2D? portalEffectTexture;
        private int portalEffectFrame;
        private float portalEffectTimer;
        private int orbBigFrame;
        private float orbBigFrameTimer;
        private const float PortalEffectInterval = 100f; // 100ms per frame
        private const float OrbBigFrameInterval = 200f; // 200ms per frame
        private bool isWarping = false; // Flag to track if warping is active
        private double warpStartTime = 0; // Time when warp starts
        private const double WarpDisplayDuration = 2000; // 2 seconds in milliseconds
        private bool isModDisabled = false; // Flag to disable the mod temporarily
//CapeStardewCode
public static string Id = "dreamy.kickitspot";

        public static object? Instance { get; internal set; }

        public override void Entry(IModHelper helper)
        {
	     // Initialize the BusTravelHandler without inheriting from Mod
          busTravelHandler = new BusTravelHandler(helper);
        
            
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Display.RenderedStep += Display_RenderedStep;
            helper.Events.Player.Warped += OnPlayerWarped; // Subscribe to Warped event
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

            Bootstrap();
        
           
    
      this.Monitor.Log("Some log message", LogLevel.Info);



    this.Monitor.Log(typeof(CustomLavaLogic).AssemblyQualifiedName!, LogLevel.Info);
            var monitor = Monitor; // Use the built-in Monitor property
            var customEventHandler = new CustomEventHandler(monitor, helper);
            customEventHandler.InitializeEvents(); // Corrected method call
            // Log the entry point loading
            Monitor.Log("CapeStardewCode: Mod entry point loaded", LogLevel.Info);
            // Initialize ItemPatches
            ItemPatches.Initialize(Monitor, helper);
            
            // Log before patching methods
            Monitor.Log("CapeStardewCode: Patching Ring methods", LogLevel.Info);
            // Hook into ring equip/unequip methods
            var harmony = new Harmony(Id);
            harmony.Patch(
                original: AccessTools.Method(typeof(Ring), nameof(Ring.onEquip)),
                postfix: new HarmonyMethod(typeof(ItemPatches), nameof(ItemPatches.Post_onRingEquip))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Ring), nameof(Ring.onUnequip)),
                postfix: new HarmonyMethod(typeof(ItemPatches), nameof(ItemPatches.Post_onRingUnequip))
            );

            // Hook into location change to patch monster data
            _ = harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.UpdateWhenCurrentLocation)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Post_UpdateWhenCurrentLocation))
            );

            // Register custom triggers
            RegisterTriggers();

            // Initialize and hook up the CustomEventHandler
            var eventHandler = new CustomEventHandler(monitor, helper); // Pass both monitor and helper

            // Subscribe to the GameLoop.SaveLoaded event
            helper.Events.GameLoop.SaveLoaded += (sender, e) =>
            {
                helper.Events.GameLoop.UpdateTicked += eventHandler.OnUpdateTicked; // Corrected method
            };

            // Unsubscribe from UpdateTicked when the day ends to prevent memory leaks
            helper.Events.GameLoop.DayEnding += (sender, e) =>
            {
                helper.Events.GameLoop.UpdateTicked -= eventHandler.OnUpdateTicked; // Corrected method
            };
            // Subscribe to GameLaunched to ensure SpaceCore is loaded

            
        }

        private void OnUpdateTicked(object? sender, EventArgs e)
{
    // Check for the player (assumed to be the first farmer)
    if (Game1.player != null)
    {
        UpdateTicked(Game1.player);
        
    }
}



        private void Bootstrap()
        {
            teleportLocation = null;
            teleportMap = null;
            portalEffectFrame = 0;
            portalEffectTimer = 0f;
            orbBigFrame = 0;
            orbBigFrameTimer = 0f;
            isWarping = false; // Reset the warping flag at the start of the day
            warpStartTime = 0;
            isModDisabled = false; // Reset the mod disable flag
        }

 private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
{
        portalTexture = Helper.ModContent.Load<Texture2D>("assets/portal.png");
            orbBigTexture = Helper.ModContent.Load<Texture2D>("assets/OrbBig.png");
            portalEffectTexture = Helper.ModContent.Load<Texture2D>("assets/portaleffect.png");

            this.Monitor.Log("Textures loaded", LogLevel.Debug);

    
         ISpaceCoreApi? spacecoreApi = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
    spacecoreApi?.RegisterSerializerType(typeof(CustomLavaLogic));
    

    

}


private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked += ReapplyRingBuffs;
            teleportLocation = null;
            teleportMap = null;
            portalEffectFrame = 0;
            portalEffectTimer = 0f;
            orbBigFrame = 0;
            orbBigFrameTimer = 0f;
            isWarping = false; // Reset the warping flag at the start of the day
            warpStartTime = 0;
            isModDisabled = false; // Reset the mod disable flag
            ItemPatches.ApplyRingBuffsOnNewDay();
             ReapplyRingBuffs();
        }

        private void ReapplyRingBuffs()
{
    Farmer player = Game1.player;

    foreach (Ring? ring in new[] { player.leftRing.Value, player.rightRing.Value })
    {
        if (ring == null) continue;

        var contextTags = ring.GetContextTags();
        string? buffId = null;

        if (contextTags.Contains("item_ring,color_blue,armor_ring"))
            buffId = "dreamy.kickitspot_OakBlessing";
        else if (contextTags.Contains("item_ring,color_pink,armor_ring"))
            buffId = "dreamy.kickitspot_PetalGrace";
        // Add more ring conditions here...

        if (buffId != null)
        {
            player.applyBuff(buffId);
            Monitor.Log($"Reapplied buff {buffId} for ring {ring.DisplayName}.", LogLevel.Info);
        }
    }
}


private void ReapplyRingBuffs(object? sender, OneSecondUpdateTickedEventArgs e)
{
    Helper.Events.GameLoop.OneSecondUpdateTicked -= ReapplyRingBuffs; // Remove handler after use
    ApplyRingBuffs();
}

private void ApplyRingBuffs()
{
    try
    {
        foreach (Ring ring in new[] { Game1.player.leftRing.Value, Game1.player.rightRing.Value })
        {
            if (ring == null) continue;

            var contextTags = ring.GetContextTags();
            string? buffId = null;

            if (contextTags.Contains("item_ring,color_blue,armor_ring"))
                buffId = "dreamy.kickitspot_OakBlessing";
            else if (contextTags.Contains("item_ring,color_pink,armor_ring"))
                buffId = "dreamy.kickitspot_PetalGrace";
            else if (contextTags.Contains("item_ring,color_cyan,armor_ring"))
                buffId = "dreamy.kickitspot_ArcaneCharm";
            else if (contextTags.Contains("item_ring,color_red,armor_ring"))
                buffId = "dreamy.kickitspot_AshenPromise";

            if (buffId != null)
            {
                Game1.player.applyBuff(buffId);
                Monitor?.Log($"Reapplied buff {buffId} for ring {ring.DisplayName}.", LogLevel.Info);
            }
        }
    }
    catch (Exception ex)
    {
        Monitor?.Log($"Failed to reapply buffs:\n{ex}", LogLevel.Error);
    }
}


         private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree || Game1.activeClickableMenu != null || Game1.dialogueUp)
                return;

            Farmer player = Game1.player;

            if (player.CurrentItem == null || player.CurrentItem.QualifiedItemId != "(O)OrbOfTheTides")
                return;

            // Check if the player is interacting with an action tile or building
            if (isModDisabled)
                return;

// Get the current toolbar instance and check if the mouse is hovering over it
    Toolbar? toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
    if (toolbar != null && toolbar.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
    {
        // If hovering over toolbar, do not trigger teleport actions
        return;
    }

            if (e.Button == SButton.MouseRight || e.Button == SButton.ControllerA)
            {
                if (teleportLocation.HasValue)
                {
                    List<Response> responses = new List<Response>
                    {
                        new Response("yes", "Yes"),
                        new Response("no", "No")
                    };
                    Game1.currentLocation.createQuestionDialogue(
                        "Do you want to reset the teleport location?",
                        responses.ToArray(),
                        OnResetPortalQuestionAnswered
                    );
                }
                else
                {
                    SetTeleportLocation(player, e.Cursor.Tile);
                }
            }

            if (e.Button == SButton.MouseLeft || e.Button == SButton.ControllerX)
            {
                if (teleportLocation.HasValue && teleportMap != null)
                {
                    List<Response> responses = new List<Response>
                    {
                        new Response("yes", "Yes"),
                        new Response("no", "No")
                    };
                    Game1.currentLocation.createQuestionDialogue(
                        "Do you want to use the Orb of the Tides?",
                        responses.ToArray(),
                        OnQuestionAnswered
                    );
                }
            }
        }


         private void OnResetPortalQuestionAnswered(Farmer who, string answer)
        {
            if (answer == "yes")
            {
                teleportLocation = null;
                teleportMap = null;
                Helper.Data.WriteSaveData("teleportLocation", (TeleportLocationData)null!); // Reset saved location
                Game1.addHUDMessage(new HUDMessage("Teleport point reset", HUDMessage.newQuest_type));
                this.Monitor.Log("Teleport point reset", LogLevel.Info);
            }
        }



          private void SetTeleportLocation(Farmer player, Vector2 location)
        {
            GameLocation gameLocation = player.currentLocation;
            if (IsValidTeleportLocation(gameLocation, location))
            {
                teleportLocation = location;
                teleportMap = gameLocation.Name;
                var data = new TeleportLocationData { X = location.X, Y = location.Y, Map = teleportMap };
                Helper.Data.WriteSaveData("teleportLocation", data); // Save the location
                Game1.addHUDMessage(new HUDMessage("Teleport location set", HUDMessage.newQuest_type));
                this.Monitor.Log($"Teleport location set to: {teleportMap} at {teleportLocation}", LogLevel.Info);
            }
            else
            {
                Game1.addHUDMessage(new HUDMessage("Invalid teleport location", HUDMessage.error_type));
                this.Monitor.Log("Invalid teleport location", LogLevel.Warn);
            }
        }


         private void OnQuestionAnswered(Farmer who, string answer)
        {
            if (answer == "yes")
            {
                isWarping = true; // Set the warping flag to true
                warpStartTime = Game1.currentGameTime.TotalGameTime.TotalMilliseconds; // Record the start time
                Game1.warpFarmer(teleportMap, (int)teleportLocation!.Value.X, (int)teleportLocation.Value.Y, false);
                Game1.addHUDMessage(new HUDMessage("Teleported", HUDMessage.newQuest_type));
                this.Monitor.Log($"Teleported to: {teleportMap} at {teleportLocation}", LogLevel.Info);
                Game1.playSound("portalActive");

                DelayedAction.functionAfterDelay(() =>
                    {
                        // Rumble effect to simulate the player's weakened state
                        Rumble.rumbleAndFade(0.75f, 1500f);

                        // Set player health and stamina to half and reset other states
                        
                        who.swimming.Value = false;
                        who.bathingClothes.Value = false;
                        who.changeOutOfSwimSuit();
                        who.forceUpdateTimer = 1000;
                        who.CanMove = true;

                        this.Monitor.Log("Player has been warped and states reset after passout.", LogLevel.Info);  }, 1000); // 2000 milliseconds (3 second) delay before executing the actions

                }
                }
        


         private void OnPlayerWarped(object? sender, WarpedEventArgs e)
        {
            if (isWarping)
            {
                isWarping = false;
            }
        }



         private void Display_RenderedStep(object? sender, RenderedStepEventArgs e)
        {
            if (e.Step == StardewValley.Mods.RenderSteps.World_Sorted)
            {
                if (teleportLocation.HasValue)
                {
                    // Draw the portal image
                    if (portalTexture != null && teleportLocation.HasValue && Game1.currentLocation.Name == teleportMap)
                    {
                        Vector2 drawPosition = teleportLocation.Value * Game1.tileSize - new Vector2(Game1.viewport.X, Game1.viewport.Y);
                        float layerDepth = drawPosition.Y / 10000f + 0.004f;

                        e.SpriteBatch.Draw(
                            portalTexture,
                            drawPosition,
                            null,
                            Color.White,
                            0f,
                            Vector2.Zero,
                            4f,
                            SpriteEffects.None,
                            layerDepth
                        );
                    }

                    // Draw the animated orbBigTexture if warping
                    if (isWarping && orbBigTexture != null)
                    {
                        double elapsed = Game1.currentGameTime.TotalGameTime.TotalMilliseconds - warpStartTime;
                        if (elapsed <= WarpDisplayDuration)
                        {
                            orbBigFrameTimer += Game1.currentGameTime.ElapsedGameTime.Milliseconds;

                            if (orbBigFrameTimer >= OrbBigFrameInterval)
                            {
                                orbBigFrameTimer -= OrbBigFrameInterval;
                                orbBigFrame = (orbBigFrame + 1) % 5; // Assuming 5 frames in the animation
                            }

                            int frameWidth = 64; // Frame width (64x64px)
                            int frameHeight = 64; // Frame height (64x64px)
                            var sourceRectangle = new Microsoft.Xna.Framework.Rectangle(frameWidth * orbBigFrame, 0, frameWidth, frameHeight);
                            Vector2 orbBigPosition = Game1.player.Position - new Vector2(Game1.viewport.X, Game1.viewport.Y);
                            orbBigPosition.X -= (frameWidth / 2f) * 4f; // Center the texture on the player
                            orbBigPosition.Y -= (frameHeight / 2f) * 4f; // Center the texture on the player

                            // Calculate fade effect
                            float fade = 1f;
                            if (elapsed < 500)
                                fade = (float)(elapsed / 500); // Fade in
                            else if (elapsed > WarpDisplayDuration - 500)
                                fade = (float)((WarpDisplayDuration - elapsed) / 500); // Fade out

                            e.SpriteBatch.Draw(
                                orbBigTexture,
                                orbBigPosition,
                                sourceRectangle,
                                Color.White * fade,
                                0f,
                                Vector2.Zero,
                                4f, // Scale the sprite 4 times larger
                                SpriteEffects.None,
                                0.89f // Above the player
                            );
                        }
                    }

                    // Draw the portal effect
if (portalEffectTexture != null && teleportLocation.HasValue && Game1.currentLocation.Name == teleportMap)
{
    // Update the animation timer for the portal effect
    portalEffectTimer += Game1.currentGameTime.ElapsedGameTime.Milliseconds;

    // Loop through animation frames if the timer reaches the interval
    if (portalEffectTimer >= PortalEffectInterval)
    {
        portalEffectTimer -= PortalEffectInterval;
        portalEffectFrame = (portalEffectFrame + 1) % 4; // Assuming 4 frames in the animation
    }

    // Define the frame size for the portal effect
    int frameWidth = 16;  // Width of each frame in the texture (16px)
    int frameHeight = 16; // Height of each frame in the texture (16px)

    // Calculate the source rectangle of the current frame in the texture
    var sourceRectangle = new Microsoft.Xna.Framework.Rectangle(
        frameWidth * portalEffectFrame, // X position of the frame in the texture
        0,                              // Y position (top) in the texture
        frameWidth,                     // Width of the frame
        frameHeight                     // Height of the frame
    );

    // Determine the position to draw the portal effect in the game world
    Vector2 drawPosition = teleportLocation.Value * Game1.tileSize - new Vector2(Game1.viewport.X, Game1.viewport.Y);

    // Set the layer depth to draw the portal effect above certain elements
    float layerDepth = drawPosition.Y / 10000f + 0.005f;

    // Draw the portal effect texture at the calculated position and size
    e.SpriteBatch.Draw(
        portalEffectTexture, // The texture to draw (portal effect)
        drawPosition,        // The position to draw at (on the screen)
        sourceRectangle,     // The portion of the texture to draw (current frame)
        Color.White,         // Color to tint the texture (none in this case)
        0f,                  // Rotation (none)
        Vector2.Zero,        // Origin (top-left corner)
        4f,                  // Scale (enlarge 4 times)
        SpriteEffects.None,  // Effects (none)
        layerDepth           // Layer depth (for drawing order)
    );
}
                }
            }
        }


         private bool IsValidTeleportLocation(GameLocation location, Vector2 tile)
        {
            return location.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport)
                && location.isTileLocationOpen(tile);
        }
        private void RegisterTriggers()
        {
            // Log trigger registration
            Monitor.Log("CapeStardewCode: Registering custom triggers", LogLevel.Info);

            TriggerActionManager.RegisterTrigger(Id + "_OnRingSkyOakEquip");
            TriggerActionManager.RegisterTrigger(Id + "_OnRingSilverPetalEquip");
            TriggerActionManager.RegisterTrigger(Id + "_OnRingMysticArcanaEquip");
	        TriggerActionManager.RegisterTrigger(Id + "_OnRingEndlessEmberEquip");
	        TriggerActionManager.RegisterTrigger(Id + "_OnRingEndlessEmberUnequip");
            TriggerActionManager.RegisterTrigger(Id + "_OnRingSkyOakUnequip");
            TriggerActionManager.RegisterTrigger(Id + "_OnRingSilverPetalUnequip");
            TriggerActionManager.RegisterTrigger(Id + "_OnRingMysticArcanaUnequip");

            // Log completion of trigger registration
            Monitor.Log("CapeStardewCode: Custom triggers registered successfully", LogLevel.Info);
        }

        // Postfix method to handle location updates
        public static void Post_UpdateWhenCurrentLocation(GameLocation __instance)
        {
            if (__instance.IsFarm)
            {
                // Handle location changes here
                MonsterDataPatches.ApplyMonsterDataPatch(__instance.Name);
            }
        }


        public static void UpdateTicked(Farmer who)
{
    // Check if the player is wearing the Endless Ember Ring
    if (who.hasBuff("dreamy.kickitspot_AshenPromise"))
    {
        // Negate the Burnt debuff continuously
        if (who.hasBuff("12")) // Burnt buff ID
        {
            Game1.player.buffs.Remove("12");
        }
    }
}


        

    }

    internal class IntegrationHelper
    {
        private IMonitor monitor;
        private ITranslationHelper translation;
        private IModRegistry modRegistry;
        private LogLevel error;

        public IntegrationHelper(IMonitor monitor, ITranslationHelper translation, IModRegistry modRegistry, LogLevel error)
        {
            this.monitor = monitor;
            this.translation = translation;
            this.modRegistry = modRegistry;
            this.error = error;
        }

        
    }

    

     public class TeleportLocationData
    {
        public float X { get; set; }
        public float Y { get; set; }
        public string? Map { get; set; }
    }


 
    
}
