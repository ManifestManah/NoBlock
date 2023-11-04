/////////////////
// - IMPORTS - //
/////////////////

// C# Specific imports
using System;

// CSSharp specific imports
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

// Required for using chat colors in messages amongst many other things
using CounterStrikeSharp.API.Modules.Utils;


// Specifies the namespace of the plugin, this should match the name of the plugin file
namespace NoBlock;


// Specifies our main class, this should match the name of the namespace
public class NoBlock : BasePlugin
{
    // The retrievable information about the plugin itself
    public override string ModuleName => "[Custom] No Block";
    public override string ModuleAuthor => "Manifest @Road To Glory & WD-";
    public override string ModuleDescription => "Allows for players to walk through each other without being stopped due to colliding.";
    public override string ModuleVersion => "V. 1.0.0 [Beta]";


    // This happens when the plugin is loaded
    public override void Load(bool hotReload)
    {
        // Registers and hooks in to the game events we intend to use
        RegisterEventHandler<EventPlayerSpawn>(Event_PlayerSpawn, HookMode.Post);
    }


    /////////////////////
    // - Game Events - //
    /////////////////////
    
    // A list of the supported events and their names can be found here:
    // -    https://github.com/roflmuffin/CounterStrikeSharp/blob/739dcf4da98dbf16291ef466536d2b27cfd56cdb/managed/CounterStrikeSharp.API/Core/GameEvents.g.cs


    // This happens when a player spawns
    private HookResult Event_PlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        // Finds and validates the CCSPlayerController, despite it being referenced as "Userid" it is in fact the CCSPlayerController
        if (!@event.Userid.IsValid)
        {
            return HookResult.Continue;
        }
    
        // Obtains the CCSPlayerController and store it within the 'player' variable
        CCSPlayerController player = @event.Userid;

        // If the CCSPlayerController connection status is anything else than connected then execute this section
        if(player.Connected != PlayerConnectedState.PlayerConnected)
        {
            return HookResult.Continue;
        }

        // If the pawn associated with the CCSPlayerController is invalid then execute this section
        if (!player.PlayerPawn.IsValid)
        {
            return HookResult.Continue;
        }

        // Stores the PlayerPawn associated with the CCSPlayerController within the variable pawn
        CHandle<CCSPlayerPawn> pawn = player.PlayerPawn;

        // Lambda version is required to pass custom parameters
        Server.NextFrame(() => PlayerSpawnNextFrame(player, pawn));

        return HookResult.Continue;
    }


    // This is called upon just after the player spawns
    private void PlayerSpawnNextFrame(CCSPlayerController player, CHandle<CCSPlayerPawn> pawn)
    {
        // Changes the player's collision to 16, allowing the player to pass through other players while still take damage from bullets and knife attacks
        pawn.Value.Collision.CollisionGroup = 16;
    }
}