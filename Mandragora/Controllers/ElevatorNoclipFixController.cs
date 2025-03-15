using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Interactables.Interobjects;
using MEC;

namespace Mandragora.Controllers
{
    class ElevatorNoclipFixController : IDisposable
    {
        public ElevatorNoclipFixController()
        {
            if (PluginFeature.ElevatorNoclipFix.IsKillswitched())
                return;

            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public void Dispose()
        {
            if (PluginFeature.ElevatorNoclipFix.IsKillswitched())
                return;

            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
        }

        IEnumerator<float> OnWaitingForPlayers()
        {
            yield return Timing.WaitForSeconds(3f);

            foreach (var elevator in Lift.List)
            {
                if (elevator?.GameObject == null)
                    continue;

                if (EntryPoint.Instance.Config.ElevatorsToExcludeNoclipProtection.Contains(elevator.Type))
                    continue;

                foreach (var elevDoor in elevator.Doors)
                {
                    elevDoor.Base._anticheatPassableThreshold = EntryPoint.Instance.Config.ElevatorNoclipThreshold;
                }
            }
        }
    }
}
