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
        public string WorldEventObjectFilter { get; set; } = string.Empty; // door indexing

        public eDimensionIndex DimensionIndex { get; set; } 
        
        public Door From { get; set; } = new();

        public int FromDoorIndex { get; set; } = 0;

        public Door To { get; set; } = new();

        public DoorSetting Setting { get; set; } = new();
    }
}
