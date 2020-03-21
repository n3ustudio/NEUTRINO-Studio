using System;
using System.Linq;

namespace NeutrinoStudio.FileConverter.Core
{

    public class Lyric
    {

        public Lyric(Data data, bool analyse)
        {
            Data = data;
            if (analyse) LyricTypeAnalyse();
        }

        private readonly Data Data;

        public LyricType TypeAnalysed = LyricType.None;

        private void LyricTypeAnalyse()
        {

            LyricType projectLyricType = LyricType.None;
            foreach (Track track in Data.TrackList)
            {
                var noteList = track.NoteList;
                int[] typeCount = { 0, 0, 0, 0 };
                foreach (Note note in noteList)
                {
                    if (note.NoteLyric.Contains(" "))
                    {
                        try
                        {
                            string subLyric =
                                note.NoteLyric.Substring(note.NoteLyric.IndexOf(" ", StringComparison.Ordinal) + 1);
                            for (int i = 1; i < 4; i++)
                            {
                                if (FindKana(subLyric.Substring(0, i)) != -1)
                                {
                                    typeCount[3]++;
                                    break;
                                }

                                if (FindRomaji(subLyric.Substring(0, i)) == -1) continue;
                                typeCount[1]++;
                                break;
                            }
                        }
                        catch
                        {
                            // Ignore
                        }
                    }
                    else
                    {
                        try
                        {
                            for (int i = 1; i < 4; i++)
                            {
                                if (FindKana(note.NoteLyric.Substring(0, i)) != -1)
                                {
                                    typeCount[2]++;
                                    break;
                                }

                                if (FindRomaji(note.NoteLyric.Substring(0, i)) == -1) continue;
                                typeCount[0]++;
                                break;
                            }
                        }
                        catch
                        {
                            // Ignore
                        }
                    }
                }
                LyricType trackLyricType = LyricType.None;
                for (int i = 0; i < 4; i++)
                {
                    if (100 * typeCount[i] / noteList.Count <= 50) continue;
                    if (trackLyricType == LyricType.None)
                    {
                        trackLyricType = (LyricType)(i + 1);
                    }
                    else
                    {
                        trackLyricType = LyricType.None;
                        break;
                    }
                }

                if (trackLyricType <= 0) continue;
                if (projectLyricType == LyricType.None)
                {
                    projectLyricType = trackLyricType;
                }
                else
                {
                    if (projectLyricType != trackLyricType)
                    {
                        return;
                    }
                }
            }
            TypeAnalysed = projectLyricType;
        }

        public void Transform(LyricType fromType, LyricType toType)
        {
            if (fromType == LyricType.None || toType == LyricType.None) return;
            CleanLyric(fromType);
            switch (fromType - toType)
            {
                case 0: break;
                case 1:
                {
                    if (fromType != LyricType.KanaTandoku)
                    {
                        TransLyricsTan2Ren(TransDirection.Reverse);
                    }
                    else
                    {
                        TransLyricsRomaji2Kana(TransDirection.Reverse);
                        TransLyricsTan2Ren(TransDirection.Sequential);
                    }
                    break;
                }
                case 2:
                {
                    TransLyricsRomaji2Kana(TransDirection.Reverse);
                    break;
                }
                case 3:
                {
                    TransLyricsRomaji2Kana(TransDirection.Reverse);
                    TransLyricsTan2Ren(TransDirection.Reverse);
                    break;
                }
                case -1:
                {
                    if (fromType != LyricType.RomajiRenzoku)
                    {
                        TransLyricsTan2Ren(TransDirection.Sequential);
                    }
                    else
                    {
                        TransLyricsRomaji2Kana(TransDirection.Sequential);
                        TransLyricsTan2Ren(TransDirection.Reverse);
                    }
                    break;
                }
                case -2:
                {
                    TransLyricsRomaji2Kana(TransDirection.Sequential);
                    break;
                }
                case -3:
                {
                    TransLyricsRomaji2Kana(TransDirection.Sequential);
                    TransLyricsTan2Ren(TransDirection.Sequential);
                    break;
                }
            }
        }

        private void CleanLyric(LyricType type)
        {
            foreach (Track track in Data.TrackList)
            {
                var noteList = track.NoteList;
                switch (type)
                {
                    case LyricType.RomajiTandoku:
                    {
                        foreach (Note note in noteList)
                        {
                            if (note.NoteLyric == "") continue;
                            note.NoteLyric = note.NoteLyric.ToLower();
                            if (note.NoteLyric.Substring(0, 1) == "?")
                            {
                                note.NoteLyric = note.NoteLyric.Remove(0, 1);
                            }

                            try
                            {
                                for (int j = 3; j >= 1; j--)
                                {
                                    if (FindRomaji(note.NoteLyric.Substring(0, j)) != -1)
                                    {
                                        note.NoteLyric = note.NoteLyric.Substring(0, j);
                                        break;
                                    }
                                }
                            }
                            catch
                            {
                                // Ignore
                            }
                        }

                        break;
                    } // Prefix other than “?” is not considered for Romaji Tandoku
                    case LyricType.RomajiRenzoku:
                    {
                        foreach (Note note in noteList)
                        {
                            if (note.NoteLyric == "") continue;
                            note.NoteLyric = note.NoteLyric.ToLower();
                            if (note.NoteLyric.Contains(" "))
                            {
                                int blankPos = note.NoteLyric.IndexOf(" ", StringComparison.Ordinal);
                                string body = "";
                                try
                                {
                                    for (int j = 1; j <= 3; j++)
                                    {
                                        if (FindRomaji(note.NoteLyric.Substring(blankPos + 1, j)) != -1)
                                        {
                                            body = note.NoteLyric.Substring(blankPos + 1, j);
                                        }
                                    }
                                }
                                catch
                                {
                                    // Ignore
                                }
                                if (body != "" && IsEnd(note.NoteLyric.Substring(blankPos - 1, 1)))
                                {
                                    note.NoteLyric = note.NoteLyric.Substring(blankPos - 1, 1) + " " + body;
                                }
                            }
                            else
                            {
                                if (note.NoteLyric.Substring(0, 1) == "?")
                                {
                                    note.NoteLyric = note.NoteLyric.Remove(0, 1);
                                }

                                try
                                {
                                    for (int j = 3; j >= 1; j--)
                                    {
                                        if (FindRomaji(note.NoteLyric.Substring(0, j)) != -1)
                                        {
                                            note.NoteLyric = note.NoteLyric.Substring(0, j);
                                            break;
                                        }
                                    }
                                }
                                catch
                                {
                                    // Ignore
                                }
                            }
                        }

                        break;
                    }
                    case LyricType.KanaTandoku:
                    {
                        foreach (Note note in noteList)
                        {
                            if (note.NoteLyric != "")
                            {
                                for (int j = 0; j < note.NoteLyric.Length; j++)
                                {
                                    string buffer = "";
                                    try
                                    {
                                        buffer = note.NoteLyric.Substring(j, 2);
                                        if (FindKana(buffer) == -1)
                                        {
                                            buffer = note.NoteLyric.Substring(j, 1);
                                        }
                                    }
                                    catch
                                    {
                                        buffer = note.NoteLyric.Substring(j, 1);
                                    }

                                    if (FindKana(buffer) == -1) continue;
                                    note.NoteLyric = buffer;
                                    break;
                                }
                            }
                            note.NoteLyric = note.NoteLyric.Replace("っ", "");
                        }

                        break;
                    }
                    case LyricType.KanaRenzoku:
                    {
                        foreach (Note note in noteList)
                        {
                            if (note.NoteLyric == "") continue;
                            if (note.NoteLyric.Contains(" "))
                            {
                                int blankPos = note.NoteLyric.IndexOf(" ", StringComparison.Ordinal);
                                string body;
                                try
                                {
                                    body = note.NoteLyric.Substring(blankPos + 1, 2);
                                    if (FindKana(body) == -1)
                                    {
                                        body = note.NoteLyric.Substring(blankPos + 1, 1);
                                    }
                                }
                                catch
                                {
                                    body = note.NoteLyric.Substring(blankPos + 1, 1);
                                }
                                if (FindKana(body) != -1 && IsEnd(note.NoteLyric.Substring(blankPos - 1, 1)))
                                {
                                    note.NoteLyric = note.NoteLyric.Substring(blankPos - 1, 1) + " " + body;
                                }
                            }
                            else
                            {
                                for (int j = 0; j < note.NoteLyric.Length; j++)
                                {
                                    string buffer = "";
                                    try
                                    {
                                        buffer = note.NoteLyric.Substring(j, 2);
                                        if (FindKana(buffer) == -1)
                                        {
                                            buffer = note.NoteLyric.Substring(j, 1);
                                        }
                                    }
                                    catch
                                    {
                                        buffer = note.NoteLyric.Substring(j, 1);
                                    }

                                    if (FindKana(buffer) == -1) continue;
                                    note.NoteLyric = buffer;
                                    break;
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }

        private void TransLyricsRomaji2Kana(TransDirection direction)
        {
            foreach (Track track in Data.TrackList)
            {
                var noteList = track.NoteList;
                switch (direction)
                {
                    case TransDirection.None:  //Do nothing
                        break;

                    case TransDirection.Sequential:  //Romaji to Kana
                        foreach (Note note in noteList)
                        {
                            if (note.NoteLyric.Contains(" ")) //is RenZokuOn
                            {
                                string head = note.NoteLyric.Substring(0, 1);
                                string body = note.NoteLyric.Remove(0, 2);
                                note.NoteLyric = head + " " + ConvertRomajiToKana(body);
                            }
                            else //is TanDokuOn
                            {
                                note.NoteLyric = ConvertRomajiToKana(note.NoteLyric);
                            }
                        }
                        break;

                    case TransDirection.Reverse:  //Kana to Romaji
                        foreach (Note note in noteList)
                        {
                            if (note.NoteLyric.Contains(" ")) //is RenZokuOn
                            {
                                string head = note.NoteLyric.Substring(0, 1);
                                string body = note.NoteLyric.Remove(0, 2);
                                note.NoteLyric = head + " " + ConvertKanaToRomaji(body);
                            }
                            else //is TanDokuOn
                            {
                                note.NoteLyric = ConvertKanaToRomaji(note.NoteLyric);
                            }
                        }
                        break;
                }
            }
        }

        private void TransLyricsTan2Ren(TransDirection direction)
        {
            foreach (Track track in Data.TrackList)
            {
                var noteList = track.NoteList;
                switch (direction)
                {
                    case TransDirection.None:  //Do nothing
                        break;

                    case TransDirection.Sequential:  //TanDoku to RenZoku
                        string tailOfLast = "-";
                        for (int i = 0; i < noteList.Count; i++)
                        {
                            string tail = tailOfLast;
                            if (i > 0 && noteList[i].NoteTimeOn > noteList[i - 1].NoteTimeOff)
                            {
                                tail = "-";
                            }
                            if (FindKana(noteList[i].NoteLyric) != -1)  //is Kana
                            {
                                tailOfLast = ConvertKanaToRomaji(noteList[i].NoteLyric).Substring(ConvertKanaToRomaji(noteList[i].NoteLyric).Length - 1, 1);
                                noteList[i].NoteLyric = tail + " " + noteList[i].NoteLyric;
                            }
                            else if (FindRomaji(noteList[i].NoteLyric) != -1) //is Romaji
                            {
                                tailOfLast = noteList[i].NoteLyric.Substring(noteList[i].NoteLyric.Length - 1, 1);
                                noteList[i].NoteLyric = tail + " " + noteList[i].NoteLyric;
                            }
                            else
                            {
                                tailOfLast = "-";
                            }
                        }
                        break;

                    case TransDirection.Reverse:  //RenZoku to TanDoku
                        foreach (Note note in noteList)
                        {
                            if (note.NoteLyric.Contains(" ") && (FindKana(note.NoteLyric.Remove(0, 2)) != -1 || FindRomaji(note.NoteLyric.Remove(0, 2)) != -1))
                            {
                                note.NoteLyric = note.NoteLyric.Remove(0, 2);
                            }
                        }
                        break;
                }
            }
        }

        private string ConvertKanaToRomaji(string kana)
        {
            for (int i = 0; i < Kanas.Length; i++)
                if (kana == Kanas[i])
                    return Romajis[i];
            return kana;
        }

        private string ConvertRomajiToKana(string romaji)
        {
            for (int i = 0; i < Kanas.Length; i++)
                if (romaji == Romajis[i])
                    return Kanas[i];
            return romaji;
        }

        private static int FindKana(string kana) => Kanas.ToList().IndexOf(kana);

        private static int FindRomaji(string romaji) => Romajis.ToList().IndexOf(romaji);

        private static bool IsEnd(string end)
        {

            if (end.Length != 1)
                return false;

            if (end == "a" || end == "i" || end == "u" || end == "e" || end == "o" || end == "n" || end == "-")
                return true;

            return false;

        }

        private static readonly string[] Kanas = { "あ", "い", "いぇ", "う", "わ", "うぁ", "うぁ", "うぃ", "うぃ", "うぇ", "え", "お", "か", "が", "き", "きぇ", "きゃ", "きゅ", "きょ", "ぎ", "ぎぇ", "ぎゃ", "ぎゅ", "ぎょ", "く", "くぁ", "くぃ", "くぇ", "くぉ", "ぐ", "ぐぁ", "ぐぃ", "ぐぇ", "ぐぉ", "け", "げ", "こ", "ご", "さ", "ざ", "し", "し", "しぇ", "しぇ", "しゃ", "しゃ", "しゅ", "しゅ", "しょ", "しょ", "じ", "じぇ", "じぇ", "じゃ", "じゃ", "じゅ", "じゅ", "じょ", "じょ", "す", "すぁ", "すぃ", "すぇ", "すぉ", "ず", "ずぁ", "ずぃ", "ずぇ", "ずぉ", "せ", "ぜ", "そ", "ぞ", "た", "だ", "ち", "ちぇ", "ちゃ", "ちゅ", "ちょ", "つ", "つ", "つぁ", "つぁ", "つぃ", "つぃ", "つぇ", "つぇ", "つぉ", "つぉ", "て", "てぃ", "てゅ", "で", "でぃ", "でゅ", "と", "とぅ", "とぅ", "ど", "どぅ", "どぅ", "な", "に", "にぇ", "にゃ", "にゅ", "にょ", "ぬ", "ぬぁ", "ぬぃ", "ぬぇ", "ぬぉ", "ね", "の", "は", "ば", "ぱ", "ひ", "ひぇ", "ひゃ", "ひゅ", "ひょ", "び", "びぇ", "びゃ", "びゅ", "びょ", "ぴ", "ぴぇ", "ぴゃ", "ぴゅ", "ぴょ", "ふ", "ふぁ", "ふぃ", "ふぇ", "ふぉ", "ぶ", "ぶぁ", "ぶぃ", "ぶぇ", "ぶぉ", "ぷ", "ぷぁ", "ぷぃ", "ぷぇ", "ぷぉ", "へ", "べ", "ぺ", "ほ", "ぼ", "ぽ", "ま", "み", "みぇ", "みゃ", "みゅ", "みょ", "む", "むぁ", "むぃ", "むぇ", "むぉ", "め", "も", "や", "ゆ", "よ", "ら", "り", "りぇ", "りゃ", "りゅ", "りょ", "る", "るぁ", "るぃ", "るぇ", "るぉ", "れ", "ろ", "わ", "を", "うぉ", "ん", "ー" };
        private static readonly string[] Romajis = { "a", "i", "ye", "u", "wa", "wa", "ua", "wi", "ui", "we", "e", "o", "ka", "ga", "ki", "kye", "kya", "kyu", "kyo", "gi", "gye", "gya", "gyu", "gyo", "ku", "kua", "kui", "kue", "kuo", "gu", "gua", "gui", "gue", "guo", "ke", "ge", "ko", "go", "sa", "za", "shi", "si", "she", "sye", "sha", "sya", "shu", "syu", "sho", "syo", "ji", "je", "jye", "ja", "jya", "ju", "jyu", "jo", "jyo", "su", "sua", "sui", "sue", "suo", "zu", "zua", "zui", "zue", "zuo", "se", "ze", "so", "zo", "ta", "da", "chi", "che", "cha", "chu", "cho", "tsu", "tu", "tsa", "tua", "tsi", "tui", "tse", "tue", "tso", "tuo", "te", "ti", "tyu", "de", "di", "dyu", "to", "tu", "twu", "do", "du", "dwu", "na", "ni", "nye", "nya", "nyu", "nyo", "nu", "nua", "nui", "nue", "nuo", "ne", "no", "ha", "ba", "pa", "hi", "hye", "hya", "hyu", "hyo", "bi", "bye", "bya", "byu", "byo", "pi", "pye", "pya", "pyu", "pyo", "fu", "fa", "fi", "fe", "fo", "bu", "bua", "bui", "bue", "buo", "pu", "pua", "pui", "pue", "puo", "he", "be", "pe", "ho", "bo", "po", "ma", "mi", "mye", "mya", "myu", "myo", "mu", "mua", "mui", "mue", "muo", "me", "mo", "ya", "yu", "yo", "ra", "ri", "rye", "rya", "ryu", "ryo", "ru", "rua", "rui", "rue", "ruo", "re", "ro", "wa", "o", "wo", "n", "-" };

    }

    public enum LyricType
    {
        None,
        RomajiTandoku,
        RomajiRenzoku,
        KanaTandoku,
        KanaRenzoku,
    }

    public enum TransDirection
    {
        None,
        Sequential,
        Reverse,
    }

}
