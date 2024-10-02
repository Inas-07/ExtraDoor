using Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.EnvTemperature.Definitions
{
    public class TemperatureSetting
    {
        public float Temperature { get; set; } = 1.0f;

        public float Damage { get; set; } = -1.0f;

        public float DamageTick { get; set; }

        public float SlowDownMultiplier_Reload { get; set; } = -1.0f;

        public float SlowDownMultiplier_Melee { get; set; } = -1.0f;

        public TemperatureSetting() { }

        public TemperatureSetting(TemperatureSetting o)
        {
            Temperature = o.Temperature;
            Damage = o.Damage;
            DamageTick = o.DamageTick;
            SlowDownMultiplier_Reload = o.SlowDownMultiplier_Reload;
            SlowDownMultiplier_Melee = o.SlowDownMultiplier_Melee;
        }
    }
}
