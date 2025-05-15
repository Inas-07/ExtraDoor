using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.ExtraDoor.Config
{
    public class DoorSetting
    {
        public GateType SecurityGateToEnter { get; set; }

        public List<LevelEventData> EventsOnEnter { get; set; } = new();

        public List<WardenObjectiveEventData> EventsOnApproachDoor { get; set; } = new();

        public List<WardenObjectiveEventData> EventsOnUnlockDoor { get; set; } = new();

        public List<WardenObjectiveEventData> EventsOnOpenDoor { get; set; } = new();

        public List<WardenObjectiveEventData> EventsOnDoorScanStart { get; set; } = new();

        public List<WardenObjectiveEventData> EventsOnDoorScanDone { get; set; } = new();

        public FCProgressionPuzzleToEnter ProgressionPuzzleToEnter { get; set; } = new(); // <---------------- Cannot parse

        public uint ChainedPuzzleToEnter { get; set; }

        public bool IsCheckpointDoor { get; set; } = false;

        public bool PlayScannerVoiceAudio { get; set; } = true;

        public ActiveEnemyWaveData ActiveEnemyWave { get; set; } = new();
    }
}
