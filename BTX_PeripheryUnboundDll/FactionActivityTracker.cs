using System;

namespace BTX_PeripheryUnbound
{
    internal class FactionActivityTracker
    {
        public static int LastCheckedMonth = -1;
        public static int LastCheckedYear = -1;

        public static bool IsFactionActive(string factionID, DateTime currentDate)
        {
            switch (factionID)
            {
                case "Rasalhague":
                    return currentDate >= new DateTime(3034, 03, 14) && currentDate < new DateTime(3081, 04, 02);

                case "Tikonov":
                    return currentDate >= new DateTime(3029, 03, 03) && currentDate < new DateTime(3031, 09, 01);

                case "Ives":
                    return currentDate >= new DateTime(3029, 01, 01) && currentDate < new DateTime(3063, 06, 10);

                case "Andurien":
                    return currentDate >= new DateTime(3030, 09, 11) && currentDate < new DateTime(3040, 01, 31);

                case "Arc-RoyalDC":
                    return currentDate >= new DateTime(3057, 12, 01) && currentDate < new DateTime(3067, 04, 24);

                case "Rim":
                    return currentDate >= new DateTime(3048, 01, 24);

                case "Calderon":
                    return currentDate >= new DateTime(3066, 04, 01);

                case "NewColonyRegion":
                    return currentDate >= new DateTime(3057, 01, 09);

                case "Illyrian":
                    return currentDate < new DateTime(3063, 06, 30);

                case "Lothian":
                    return currentDate < new DateTime(3054, 12, 31);

                case "Circinus":
                    return currentDate < new DateTime(3081, 05, 01);

                case "Valkyrate":
                    return currentDate < new DateTime(3049, 09, 27);

                case "Oberon":
                    return currentDate < new DateTime(3049, 09, 02);

                default:
                    return true;
            }
        }
    }
}
