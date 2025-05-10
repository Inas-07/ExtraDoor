using GameData;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.ExtraDoor.Config
{
    public class ForceConnectCfg
    {
        public eDimensionIndex DimensionIndex { get; set; } 
        
        public AreaDoor From { get; set; } = new();

        public AreaDoor To { get; set; } = new();

        public DoorSetting Setting { get; set; } = new();
    }
}
