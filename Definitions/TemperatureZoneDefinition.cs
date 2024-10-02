namespace EOSExt.EnvTemperature.Definitions
{
    public class TemperatureZoneDefinition: ExtraObjectiveSetup.BaseClasses.GlobalZoneIndex
    {
        public float DecreaseRate { get; set; } = 0.0f;
    }
}
