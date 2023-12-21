/////////////////
// - IMPORTS - //
/////////////////

// C# Specific imports
using System;

// Required for being able to use the windows & linux check
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

// CSSharp specific imports
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

// Required for being able to use attributes for commands and defining minimum api versions
using CounterStrikeSharp.API.Core.Attributes;

// Required for using chat colors in messages amongst many other things
using CounterStrikeSharp.API.Modules.Memory;

// Required for using the enumeration for CollisionGroup
using CounterStrikeSharp.API.Modules.Entities.Constants;

// Required for using chat colors in messages amongst many other things
using CounterStrikeSharp.API.Modules.Utils;



// Specifies the namespace of the plugin, this should match the name of the plugin file
namespace NoBlock;

// Defines the minimum version of CounterStrikeSharp required in order to run this plugin on the server 
[MinimumApiVersion(96)]

// Specifies our main class, this should match the name of the namespace
public class NoBlock : BasePlugin
{
    // The retrievable information about the plugin itself
    public override string ModuleName => "[Custom] No Block";
    public override string ModuleAuthor => "Manifest @Road To Glory & WD-";
    public override string ModuleDescription => "Allows for players to walk through each other without being stopped due to colliding.";
    public override string ModuleVersion => "V. 1.0.1 [Beta]";

    // Sets the correct offset data for our collision rules change in accordance with the server's operating system
    private readonly WIN_LINUX<int> OnCollisionRulesChangedOffset = new WIN_LINUX<int>(174, 173);

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
        pawn.Value.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DISSOLVING;

        // Changes the player's CollisionAttribute to the collision type used for dissolving objects 
        pawn.Value.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DISSOLVING;

        // Updates the CollisionRulesChanged for the specific player
        VirtualFunctionVoid<nint> collisionRulesChanged = new VirtualFunctionVoid<nint>(pawn.Value.Handle, OnCollisionRulesChangedOffset.Get());

        // Invokes the updated CollisionRulesChanged information to ensure the player's collision is correctly set
        collisionRulesChanged.Invoke(pawn.Value.Handle);
    }
}



// The IsValid class is used for validating entities
internal static class IsValid
{
    // Returns true if the player's index is valid
    public static bool PlayerIndex(uint playerIndex)
    {
        // If the player's index is 0 then execute this section
        if(playerIndex == 0)
        {
            return false;
        }

        // If the client's index value is not within the range it should be then execute this section
        if(!(1 <= playerIndex && playerIndex <= Server.MaxPlayers))
        {
            return false;
        }

        return true;
    }
}



// NOTE:
//      This class was made by KillStr3aK / Bober repository and allows you to perform actions depending on whether the server is running a linux or windows operating system
//      - https://github.com/KillStr3aK/CSSharpTests/blob/master/Models/WIN_LINUX.cs
public class WIN_LINUX<T>
{
    [JsonPropertyName("Windows")]
    public T Windows { get; private set; }

    [JsonPropertyName("Linux")]
    public T Linux { get; private set; }

    public WIN_LINUX(T windows, T linux)
    {
        this.Windows = windows;
        this.Linux = linux;
    }

    public T Get()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return this.Windows;
        }
        else
        {
            return this.Linux;
        }
    }
}
