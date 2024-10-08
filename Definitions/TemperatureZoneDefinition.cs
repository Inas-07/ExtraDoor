namespace EOSExt.EnvTemperature.Definitions
{
    public class TemperatureZoneDefinition: ExtraObjectiveSetup.BaseClasses.GlobalZoneIndex
    {
        public float Temperature_Downlimit { get; set; } = TemperatureDefinitionManager.MIN_TEMP;

        public float Temperature_Normal { get; set; } = 0.5f;

        public float Temperature_Uplimit { get; set; } = TemperatureDefinitionManager.MAX_TEMP;

        public float FluctuationIntensity { get; set; } = 0.0f;
    }
}
