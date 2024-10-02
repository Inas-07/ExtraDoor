using ExtraObjectiveSetup.BaseClasses;
using System.Collections.Generic;

namespace EOSExt.EnvTemperature.Definitions
{
    public class TemperatureDefinition
    {
        public float StartTemperature { get; set; } = 0.5f;

        public float JumpActionHeatGained { get; set; }

        public float SprintActionHeatGained { get; set; }

        public float CrouchActionHeatGained { get; set; }

        public float StandingActionHeatGained { get; set; }

        public float LadderClimbingActionHeatGained { get; set; }

        public List<TemperatureZoneDefinition> Zones { get; set; } = new() { new() };

        public List<TemperatureSetting> Settings { get; set; } = new() { new() };
    }
}
