using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace CapeStardewCode
{
    public class CustomEventHandler
    {
        private readonly IMonitor monitor;
        private readonly IModHelper helper;
        private bool passoutTriggered = false; // Flag to prevent repeated execution

        public CustomEventHandler(IMonitor monitor, IModHelper helper)
        {
            this.monitor = monitor;
            this.helper = helper;
        }

        public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            // Ensure the game is ready and the player exists
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            // Prevent further execution if the passout event has already been triggered
            if (passoutTriggered)
                return;

            Farmer who = Game1.player;

            // Use VerboseLog instead of Trace to hide player state logs unless verbose logging is enabled
            monitor.VerboseLog($"Player state - Swimming: {who.swimming.Value}, Health: {who.health}, Stamina: {who.Stamina}");

            // Check for swimming and health/stamina conditions
            if (who.swimming.Value && (who.health <= 0 || who.Stamina < 0))
            {
                // Keep important log at Info level
                monitor.Log("Player needs to pass out or be rescued due to swimming exhaustion or defeat.", LogLevel.Info);
                passoutTriggered = true; // Set flag to true to prevent further execution
                PerformCapeStardewPassoutEvent(who);
            }
        }

        private void PerformCapeStardewPassoutEvent(Farmer who)
        {
            monitor.Log("Triggering custom passout event.", LogLevel.Info);
            try
            {
                // Fade out the screen immediately
                Game1.globalFadeToBlack(() =>
                {
                    // Make the player face down and jitter for visual effect
                    who.jitterStrength = 1f;

                    // Perform a dizzy emote to indicate disorientation
                    who.animateOnce(293); // Example: 293 represents the dizzy emote

                    // Use DelayedAction to delay the following actions
                    DelayedAction.functionAfterDelay(() =>
                    {
                        // Rumble effect to simulate the player's weakened state
                        Rumble.rumbleAndFade(0.75f, 1500f);

                        // Set player health and stamina to half and reset other states
                        who.health = who.maxHealth / 2;
                        who.Stamina = who.maxStamina.Value / 2;
                        who.swimming.Value = false;
                        who.bathingClothes.Value = false;
                        who.changeOutOfSwimSuit();
                        who.forceUpdateTimer = 1000;
                        who.CanMove = false;

                        monitor.Log("Player has been warped and states reset after passout.", LogLevel.Info);

                        // After the dialogue is shown, fade back to clear
                        Game1.globalFadeToClear(() =>
                        {
                            // Display dialogue to inform the player
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Caperescue.001"));

                            // Hook into the after-dialogue event to automatically continue
                            Game1.afterDialogues += () =>
                            {
                                // Automatically close the dialogue menu
                                Game1.exitActiveMenu();

                                // Reset the passout flag to allow future triggers
                                passoutTriggered = false;
                            };
                        });

                    }, 3000); // 3000 milliseconds (3 second) delay before executing the actions

                });
            }
            catch (Exception ex)
            {
                monitor.Log($"Error during passout event: {ex.Message}", LogLevel.Error);
            }
        }

        public void InitializeEvents()
        {
            // Initialize event subscription for tick updates
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }
    }
}
