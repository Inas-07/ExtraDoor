using GameData;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.CircularZones
{
    public class AreaDoor
    {
        public LG_LayerType Layer { get; set; }

        public eLocalZoneIndex LocalIndex { get; set; }

        public int AreaIndex { get; set; } = -1;
    }

    public class DoorSetting
    {

    }

    public class ForceConnectCfg
    {
        public eDimensionIndex DimensionIndex { get; set; } 
        
        public AreaDoor From { get; set; } = new();

        public AreaDoor To { get; set; } = new();

        public DoorSetting Setting { get; set; } = new();
    }
}
