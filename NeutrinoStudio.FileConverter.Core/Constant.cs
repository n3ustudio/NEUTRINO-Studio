using System.Collections.Generic;

namespace NeutrinoStudio.FileConverter.Core
{

    internal static class Constant
    {

        public const int TickNumberForOneBeat = 480;

        public const int TickNumberForOneBar = 4 * TickNumberForOneBeat;

        public const int KeyForOneOctave = 12;

        public static List<string> KeyList = new List<string>
            {"C", "C", "D", "D", "E", "F", "F", "G", "G", "A", "A", "B"};

        public static List<int> AlterList = new List<int> {0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0};

    }

}
