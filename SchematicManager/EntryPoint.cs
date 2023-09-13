using System;
using Exiled.API.Features;
using SchematicManager.Controllers;

namespace SchematicManager;

public class EntryPoint : Plugin<Config>
{
    public override string Author { get; } = "User_NotExist";
    public override string Name { get; } = "SchematicManager";
    public override Version Version { get; } = new Version(1, 0, 0);
    public override Version RequiredExiledVersion { get; } = new Version(8, 2, 1);
    
    EventHandlers EventHandlers;
    
    public override void OnEnabled()
    {
        EventHandlers = new EventHandlers();
        
        Exiled.Events.Handlers.Server.WaitingForPlayers += EventHandlers.OnWaitingForPlayers;
        Exiled.Events.Handlers.Server.RestartingRound += EventHandlers.OnRestartingRound;
        base.OnEnabled();
    }
    
    public override void OnDisabled()
    {
        Exiled.Events.Handlers.Server.WaitingForPlayers -= EventHandlers.OnWaitingForPlayers;
        Exiled.Events.Handlers.Server.RestartingRound -= EventHandlers.OnRestartingRound;
        
        EventHandlers = null;
        base.OnDisabled();
    }
}