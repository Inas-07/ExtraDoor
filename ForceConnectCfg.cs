using GameData;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.CircularZones
{
    public class ForceConnectCfg
    {
        public eDimensionIndex DimensionIndex { get; set; } 
        public LG_LayerType Layer { get; set; }
        public eLocalZoneIndex FromLocalIndex { get; set; } 
        public eLocalZoneIndex ToLocalIndex { get; set; } 
    }
}
