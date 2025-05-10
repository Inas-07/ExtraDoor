using GameData;
using Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.ExtraDoor.Config
{
    public class FCZonePlacementData : ExtraObjectiveSetup.BaseClasses.GlobalZoneIndex
    {
        public ZonePlacementWeights Weights { get; set; } = new();
    }

    public class FCProgressionPuzzleToEnter
    {
        public eProgressionPuzzleType PuzzleType { get; set; }

        public LocalizedText CustomText { get; set; } = null;

        public int PlacementCount { get; set; } = 1;

        public List<FCZonePlacementData> ZonePlacementData { get; set; } = new() { new() };
    }
}
