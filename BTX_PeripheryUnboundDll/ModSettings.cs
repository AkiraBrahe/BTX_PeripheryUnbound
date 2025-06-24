namespace BTX_PeripheryUnbound
{
    public class ModSettings
    {
        public ModDebugSettings Debug { get; set; } = new ModDebugSettings();
        public MapVisualSettings MapVisuals { get; set; } = new MapVisualSettings();
        public IntelSettings ContractIntel { get; set; } = new IntelSettings();
    }

    public class ModDebugSettings
    {
        public bool Debug { get; set; } = false;
        public bool UpdateStarSystemDefsOnLoad { get; set; } = false;
    }

    public class MapVisualSettings
    {
        public bool HighlightStarClusters { get; set; } = true;
        public bool HighlightInhabitedSystems { get; set; } = true;
        public bool ShowPopulationLevels { get; set; } = true;
        public bool HideJumpPoints { get; set; } = true;
    }

    public class IntelSettings
    {
        public bool IntelShowTarget { get; set; } = true;
        public bool IntelShowVariant { get; set; } = true;
    }
}
