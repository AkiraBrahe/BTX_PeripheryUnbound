using System;

namespace BTX_PeripheryUnbound.Tools
{
    internal class FactionActivityTracker
    {
        public static DateTime LastDayUpdated = new(1999, 1, 1);
        public static bool IsFactionActive(string factionID, DateTime currentDate)
        {
            return factionID switch
            {
                "Rasalhague" => currentDate >= new DateTime(3034, 03, 14) && currentDate < new DateTime(3152, 12, 31),
                "Tikonov" => currentDate >= new DateTime(3029, 03, 03) && currentDate < new DateTime(3031, 09, 01),
                "Ives" => currentDate >= new DateTime(3029, 01, 01) && currentDate < new DateTime(3063, 06, 10),
                "Andurien" => (currentDate >= new DateTime(3030, 09, 11) && currentDate < new DateTime(3040, 01, 31)) ||
                              (currentDate >= new DateTime(3079, 01, 25) && currentDate < new DateTime(3152, 12, 31)),
                "Arc-RoyalDC" => currentDate >= new DateTime(3057, 12, 01) && currentDate < new DateTime(3067, 04, 24),
                "Rim" => currentDate >= new DateTime(3048, 01, 24) && currentDate < new DateTime(3148, 06, 24),
                "Elysia" => currentDate >= new DateTime(2900, 09, 28) && currentDate < new DateTime(3049, 09, 18),
                "Calderon" => currentDate >= new DateTime(3066, 04, 01) && currentDate < new DateTime(3152, 12, 31),
                "NewColonyRegion" => currentDate >= new DateTime(3057, 01, 09) && currentDate < new DateTime(3152, 12, 31),
                "Illyrian" => currentDate >= new DateTime(2444, 02, 22) && currentDate < new DateTime(3063, 06, 30),
                "Lothian" => currentDate >= new DateTime(2821, 12, 27) && currentDate < new DateTime(3054, 12, 31),
                "Circinus" => currentDate >= new DateTime(2785, 03, 14) && currentDate < new DateTime(3081, 05, 01),
                "Valkyrate" => currentDate >= new DateTime(3021, 04, 14) && currentDate < new DateTime(3049, 09, 27),
                "Oberon" => currentDate >= new DateTime(2783, 01, 07) && currentDate < new DateTime(3049, 09, 02),
                _ => true,
            };
        }
    }
}
