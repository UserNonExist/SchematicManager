using SchematicManager.Controllers;

namespace SchematicManager;

public class EventHandlers
{
    public void OnWaitingForPlayers()
    {
        VendingMachineController.SpawnVendingMachines();
    }

    public void OnRestartingRound()
    {
        VendingMachineController.DestroyVendingMachines();
    }
}