using GameData;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.ExtraDoor.Config
{
    public class AreaDoor
    {
        public LG_LayerType Layer { get; set; }

        public eLocalZoneIndex LocalIndex { get; set; }

        public int AreaIndex { get; set; } = -1;
    }
}
