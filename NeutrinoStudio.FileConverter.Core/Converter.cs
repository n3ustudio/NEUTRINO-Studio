using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NeutrinoStudio.FileConverter.Core
{
    public class Converter
    {
        public Converter()
        {
        }

        public Converter(Converter converter)
        {
            ProjectName = converter.ProjectName;
            Files = converter.Files;
            TrackList = converter.TrackList.Select(it => new Track(it)).ToList();
            TimeSigList = converter.TimeSigList.Select(it => new TimeSig(it)).ToList();
            TempoList = converter.TempoList.Select(it => new Tempo(it)).ToList();
            PreMeasure = converter.PreMeasure;
            Lyric = new Lyric(this, false);
        }

        public string ProjectName;
        public List<string> Files;
        public List<Track> TrackList;
        public List<TimeSig> TimeSigList;
        public List<Tempo> TempoList;
        public int PreMeasure = 0;
        public Lyric Lyric;

        public void Import(List<string> fileNames)
        {
            throw new NotImplementedException("Do not use this import method. Check file extensions or use switch cases instead.");
            Files = fileNames.ToList();
            InputFormat format = InputFormat.Undefined;
            string extension = Path.GetExtension(fileNames.First());
            //Determine the format of the project
            if (extension == ".vsqx")
            {
                string content = File.ReadAllText(Files[0]);
                if (content.Contains("vsq3 xmlns=\"http://www.yamaha.co.jp/vocaloid/schema/vsq3/\""))
                {
                    format = InputFormat.Vsq3;
                }
                else if (content.Contains("vsq4 xmlns=\"http://www.yamaha.co.jp/vocaloid/schema/vsq4/\""))
                {
                    format = InputFormat.Vsq4;
                }
            }
            else if (extension == ".ust")
            {
                format = InputFormat.Ust;
            }
            else if (extension == ".ccs")
            {
                format = InputFormat.Ccs;
            }
            else if (extension == ".vpr")
            {
                format = InputFormat.Vpr;
            }
            else
            {
                throw new NeutrinoStudioFileConverterDeprecatedException("The format is not supported.");
            }

            switch (format)
            {
                case InputFormat.Vsq2:
                    throw new NeutrinoStudioFileConverterDeprecatedException("The format is not supported.");

                case InputFormat.Vsq3:
                    ImportVsq3(Files);
                    return;

                case InputFormat.Vsq4:
                    ImportVsq4(Files);
                    return;

                case InputFormat.Ust:
                    ImportUst(Files);
                    return;

                case InputFormat.Ccs:
                    ImportCcs(Files);
                    return;

                case InputFormat.Vpr:
                    ImportVpr(Files, "");
                    return;
            }
        }

        public void ImportVsq3(List<string> fileNames)
        {
            if (fileNames.Count != 1)
                throw new NeutrinoStudioFileConverterOperationException("Cannot Import more than one vsqx files.");
            XmlDocument vsq3 = new XmlDocument();
            vsq3.Load(fileNames.First());

            //Setup TrackList
            XmlNode root = vsq3.FirstChild.NextSibling;
            if (root is null)
                throw new NeutrinoStudioFileConverterFileException("Vsq3 file error at FirstChild.NextSibling");
            ProjectName = Path.GetFileNameWithoutExtension(fileNames[0]);
            TrackList = new List<Track>();
            TimeSigList = new List<TimeSig>();
            TempoList = new List<Tempo>();
            int TrackNum = 0;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                if (root.ChildNodes[i].Name != "masterTrack") continue;
                XmlNode masterTrack = root.ChildNodes[i];
                for (int j = 0; j < masterTrack.ChildNodes.Count; j++)
                {
                    if (masterTrack.ChildNodes[j].Name == "preMeasure")
                    {
                        PreMeasure = Convert.ToInt32(masterTrack.ChildNodes[j].FirstChild.Value);
                    }

                    if (masterTrack.ChildNodes[j].Name == "timeSig")
                    {
                        TimeSig newTimeSig = new TimeSig();
                        XmlNode inTimeSig = masterTrack.ChildNodes[j].FirstChild;
                        newTimeSig.PosMes = Convert.ToInt32(inTimeSig.FirstChild.Value);
                        inTimeSig = inTimeSig.NextSibling;
                        if (inTimeSig is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq3 file error at inTimeSig.NextSibling");
                        newTimeSig.Nume = Convert.ToInt32(inTimeSig.FirstChild.Value);
                        inTimeSig = inTimeSig.NextSibling;
                        if (inTimeSig is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq3 file error at inTimeSig.NextSibling");
                        newTimeSig.Denomi = Convert.ToInt32(inTimeSig.FirstChild.Value);
                        TimeSigList.Add(newTimeSig);
                    }

                    if (masterTrack.ChildNodes[j].Name == "tempo")
                    {
                        Tempo newTempo = new Tempo();
                        XmlNode inTimeSig = masterTrack.ChildNodes[j].FirstChild;
                        newTempo.PosTick = Convert.ToInt32(inTimeSig.FirstChild.Value);
                        inTimeSig = inTimeSig.NextSibling;
                        if (inTimeSig is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq3 file error at inTimeSig.NextSibling");
                        newTempo.BpmTimes100 = Convert.ToInt32(inTimeSig.FirstChild.Value);
                        TempoList.Add(newTempo);
                    }
                }
            }

            if (!root.HasChildNodes) return;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                if (root.ChildNodes[i].Name != "vsTrack") continue;
                int noteNum = 0;
                XmlNode thisTrack = root.ChildNodes[i];
                Track newTrack = new Track {TrackNum = TrackNum, NoteList = new List<Note>()};
                //Setup NoteList for every track
                for (int j = 0; j < thisTrack.ChildNodes.Count; j++)
                {
                    if (thisTrack.ChildNodes[j].Name == "trackName")
                    {
                        newTrack.TrackName = thisTrack.ChildNodes[j].FirstChild.Value;
                    }

                    if (thisTrack.ChildNodes[j].Name != "musicalPart") continue;
                    XmlNode thisPart = thisTrack.ChildNodes[j];
                    int partStartTime = Convert.ToInt32(thisPart.FirstChild.FirstChild.Value);
                    for (int k = 0; k < thisPart.ChildNodes.Count; k++)
                    {
                        if (thisPart.ChildNodes[k].Name != "note") continue;
                        Note newNote = new Note {NoteNum = noteNum};
                        XmlNode thisNote = thisPart.ChildNodes[k];
                        XmlNode inThisNote = thisNote.FirstChild;
                        newNote.NoteTimeOn =
                            Convert.ToInt32(inThisNote.FirstChild.Value) + partStartTime;
                        inThisNote = inThisNote.NextSibling;
                        if (inThisNote is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq3 file error at inThisNote.NextSibling");
                        newNote.NoteTimeOff =
                            newNote.NoteTimeOn + Convert.ToInt32(inThisNote.FirstChild.Value);
                        inThisNote = inThisNote.NextSibling;
                        if (inThisNote is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq3 file error at inThisNote.NextSibling");
                        newNote.NoteKey = Convert.ToInt32(inThisNote.FirstChild.Value);
                        inThisNote = inThisNote.NextSibling;
                        if (inThisNote is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq3 file error at inThisNote.NextSibling");
                        inThisNote = inThisNote.NextSibling;
                        if (inThisNote is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq3 file error at inThisNote.NextSibling");
                        newNote.NoteLyric = inThisNote.FirstChild.Value;
                        noteNum++;
                        newTrack.NoteList.Add(newNote);
                    }
                }

                if (newTrack.NoteList.Count > 0)
                {
                    TrackList.Add(newTrack);
                }
            }

            if (TrackList.Count <= 0)
                throw new NeutrinoStudioFileConverterFileException("The vsqx is invalid or empty.");
            Lyric = new Lyric(this, true);
        }

        public void ImportVsq4(List<string> fileNames)
        {
            if (fileNames.Count != 1)
                throw new NeutrinoStudioFileConverterOperationException("Cannot Import more than one vsqx files.");
            XmlDocument vsq4 = new XmlDocument();
            vsq4.Load(fileNames[0]);
            //Setup TrackList
            XmlNode root = vsq4.FirstChild.NextSibling;
            ProjectName = Path.GetFileNameWithoutExtension(fileNames[0]);
            TrackList = new List<Track>();
            TimeSigList = new List<TimeSig>();
            TempoList = new List<Tempo>();
            int trackNum = 0;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                if (root.ChildNodes[i].Name != "masterTrack") continue;
                XmlNode masterTrack = root.ChildNodes[i];
                for (int j = 0; j < masterTrack.ChildNodes.Count; j++)
                {
                    if (masterTrack.ChildNodes[j].Name == "preMeasure")
                    {
                        PreMeasure = Convert.ToInt32(masterTrack.ChildNodes[j].FirstChild.Value);
                    }

                    if (masterTrack.ChildNodes[j].Name == "timeSig")
                    {
                        TimeSig newTimeSig = new TimeSig();
                        XmlNode inTimeSig = masterTrack.ChildNodes[j].FirstChild;
                        newTimeSig.PosMes = Convert.ToInt32(inTimeSig.FirstChild.Value);
                        inTimeSig = inTimeSig.NextSibling;
                        if (inTimeSig is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at inTimeSig.NextSibling");
                        newTimeSig.Nume = Convert.ToInt32(inTimeSig.FirstChild.Value);
                        inTimeSig = inTimeSig.NextSibling;
                        if (inTimeSig is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at inTimeSig.NextSibling");
                        newTimeSig.Denomi = Convert.ToInt32(inTimeSig.FirstChild.Value);
                        TimeSigList.Add(newTimeSig);
                    }

                    if (masterTrack.ChildNodes[j].Name != "tempo") continue;
                    {
                        Tempo newTempo = new Tempo();
                        XmlNode inTimeSig = masterTrack.ChildNodes[j].FirstChild;
                        newTempo.PosTick = Convert.ToInt32(inTimeSig.FirstChild.Value);
                        inTimeSig = inTimeSig.NextSibling;
                        if (inTimeSig is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at inTimeSig.NextSibling");
                        newTempo.BpmTimes100 = Convert.ToInt32(inTimeSig.FirstChild.Value);
                        TempoList.Add(newTempo);
                    }
                }
            }

            if (!root.HasChildNodes) return;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                if (root.ChildNodes[i].Name != "vsTrack") continue;
                int noteNum = 0;
                XmlNode thisTrack = root.ChildNodes[i];
                Track newTrack = new Track {TrackNum = trackNum, NoteList = new List<Note>()};
                //Setup NoteList for every track
                for (int j = 0; j < thisTrack.ChildNodes.Count; j++)
                {
                    if (thisTrack.ChildNodes[j].Name == "name")
                    {
                        newTrack.TrackName = thisTrack.ChildNodes[j].FirstChild.Value;
                    }

                    if (thisTrack.ChildNodes[j].Name != "vsPart") continue;
                    XmlNode thisPart = thisTrack.ChildNodes[j];
                    int partStartTime = Convert.ToInt32(thisPart.FirstChild.FirstChild.Value);
                    for (int k = 0; k < thisPart.ChildNodes.Count; k++)
                    {
                        if (thisPart.ChildNodes[k].Name != "note") continue;
                        Note newNote = new Note {NoteNum = noteNum};
                        XmlNode thisNote = thisPart.ChildNodes[k];
                        XmlNode inThisNote = thisNote.FirstChild;
                        newNote.NoteTimeOn = Convert.ToInt32(inThisNote.FirstChild.Value) + partStartTime;
                        inThisNote = inThisNote.NextSibling;
                        if (inThisNote is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at inThisNote.NextSibling");
                        newNote.NoteTimeOff = newNote.NoteTimeOn + Convert.ToInt32(inThisNote.FirstChild.Value);
                        inThisNote = inThisNote.NextSibling;
                        if (inThisNote is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at inThisNote.NextSibling");
                        newNote.NoteKey = Convert.ToInt32(inThisNote.FirstChild.Value);
                        inThisNote = inThisNote.NextSibling;
                        if (inThisNote is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at inThisNote.NextSibling");
                        inThisNote = inThisNote.NextSibling;
                        if (inThisNote is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at inThisNote.NextSibling");
                        newNote.NoteLyric = inThisNote.FirstChild.Value;
                        noteNum++;
                        newTrack.NoteList.Add(newNote);
                    }
                }

                if (newTrack.NoteList.Count > 0)
                {
                    TrackList.Add(newTrack);
                }
            }

            if (TrackList.Count > 0)
            {
                Lyric = new Lyric(this, true);
                return;
            }

            throw new NeutrinoStudioFileConverterFileException("The vsqx is invalid or empty.");
        }

        public void ImportVpr(List<string> fileNames, string tempDir)
        {
            if (fileNames.Count != 1)
                throw new NeutrinoStudioFileConverterOperationException("Cannot Import more than one vpr files.");
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            Directory.CreateDirectory(tempDir);

            string copiedVprTempFileName = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(fileNames.First()) + ".zip");
            File.Copy(fileNames.First(), copiedVprTempFileName);
            string unzippedDirectory = ZipUtil.Unzip(copiedVprTempFileName);
            string jsonFileName = Path.Combine(unzippedDirectory, "Project", "sequence.json");
            Vpr vpr = JsonConvert.DeserializeObject<Vpr>(File.ReadAllText(jsonFileName));

            ProjectName = vpr.Title;
            PreMeasure = 1;
            TimeSigList = vpr.MasterTrack.TimeSig.Events.Select(it => new TimeSig
            {
                PosMes = it.Bar + 1,
                Nume = it.Numer,
                Denomi = it.Denom
            }).ToList();
            TempoList = vpr.MasterTrack.Tempo.Events.Select(it => new Tempo
            {
                PosTick = Constant.TickNumberForOneBar + it.Pos,
                BpmTimes100 = it.Value
            }).ToList();
            TrackList = new List<Track>();

            for (int i = 0; i < vpr.Tracks.Count; i++)
            {
                Vpr.VprTrack vprTrack = vpr.Tracks[i];
                Track track = new Track {TrackName = vprTrack.Name, TrackNum = i, NoteList = new List<Note>()};
                if (vprTrack.Parts != null)
                {
                    foreach (Vpr.VprPart vprPart in vprTrack.Parts)
                    {
                        int partStartTime = vprPart.Pos;
                        if (vprPart.Notes == null) continue;
                        foreach (Vpr.VprNote vprNote in vprPart.Notes)
                        {
                            track.NoteList.Add(new Note
                            {
                                NoteTimeOn = Constant.TickNumberForOneBar + partStartTime + vprNote.Pos,
                                NoteTimeOff = Constant.TickNumberForOneBar + partStartTime + vprNote.Pos +
                                              vprNote.Duration,
                                NoteKey = vprNote.Number,
                                NoteLyric = vprNote.Lyric
                            });
                        }
                    }
                }

                if (track.NoteList.Count == 0)
                {
                    continue;
                }

                var clearedNoteList = new List<Note>();
                for (int j = 0; j < track.NoteList.Count - 1; j++)
                {
                    Note note = track.NoteList[j];
                    Note nextNote = track.NoteList[j + 1];
                    if (note.NoteTimeOff > nextNote.NoteTimeOn)
                    {
                        note.NoteTimeOff = nextNote.NoteTimeOn;
                    }

                    if (note.NoteTimeOff > note.NoteTimeOn)
                    {
                        clearedNoteList.Add(note);
                    }
                }

                for (int j = 0; j < clearedNoteList.Count; j++)
                {
                    clearedNoteList[j].NoteNum = j;
                }

                track.NoteList = clearedNoteList;
                if (track.NoteList.Count > 0)
                {
                    TrackList.Add(track);
                }
            }

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }

            if (TrackList.Count <= 0)
                throw new NeutrinoStudioFileConverterFileException("The vpr is invalid or empty.");
            Lyric = new Lyric(this, true);
        }

        public void ImportUst(List<string> fileNames)
        {
            int trackCount = 0;
            TrackList = new List<Track>();
            foreach (string fileName in fileNames)
            {
                bool isValid = false;
                Track newTrack;
                using (StreamReader reader = new StreamReader(fileName, Encoding.GetEncoding("Shift-JIS")))
                {
                    int noteNum = 0;
                    int time = 0;
                    PreMeasure = 1;
                    //Setup TrackList
                    newTrack = new Track {TrackNum = 0, TrackName = Path.GetFileNameWithoutExtension(fileName)};
                    if (trackCount == 0)
                    {
                        TimeSig firstTimeSig = new TimeSig();
                        TimeSigList = new List<TimeSig>();
                        firstTimeSig.Denomi = 4;
                        firstTimeSig.Nume = 4;
                        firstTimeSig.PosMes = 0;
                        TimeSigList.Add(firstTimeSig);
                        TempoList = new List<Tempo>();
                    }

                    //Setup NoteList for every track
                    for (string buffer = "Starting";
                        buffer != "[#TRACKEND]" && buffer != null;
                        buffer = reader.ReadLine())
                    {
                        if (trackCount == 0)
                        {
                            if (buffer.Contains("ProjectName="))
                            {
                                ProjectName = buffer.Remove(0, "ProjectName=".Length);
                            }

                            if (buffer.Contains("Tempo="))
                            {
                                Tempo firstTempo = new Tempo {PosTick = 0};
                                if (double.TryParse(buffer.Remove(0, "Tempo=".Length), out double bpm))
                                {
                                    firstTempo.BpmTimes100 = (int) (bpm * 100);
                                }

                                TempoList.Add(firstTempo);
                            }
                        }

                        if (!buffer.Contains("[#0000]")) continue;
                        isValid = false;
                        newTrack.NoteList = new List<Note>();
                        Note newNote = new Note {NoteNum = noteNum};
                        bool isNoteValid = false;
                        bool tempoTempFlag = false;
                        for (buffer = reader.ReadLine();
                            buffer != "[#TRACKEND]" && buffer != null;
                            buffer = reader.ReadLine())
                        {
                            if (buffer.Contains("[#"))
                            {
                                if (isNoteValid)
                                {
                                    newTrack.NoteList.Add(newNote);
                                    noteNum++;
                                }

                                newNote = new Note {NoteNum = noteNum};
                                isNoteValid = false;
                            }

                            if (buffer.Contains("Length="))
                            {
                                newNote.NoteTimeOn = time;
                                time += Convert.ToInt32(buffer.Substring(7, buffer.Length - 7));
                                newNote.NoteTimeOff = time;
                                if (tempoTempFlag)
                                {
                                    TempoList[TempoList.Count - 1].PosTick = newNote.NoteTimeOn;
                                }
                            }

                            if (buffer.Contains("Lyric=") &&
                                buffer.Substring(6, buffer.Length - 6) != "R" &&
                                buffer.Substring(6, buffer.Length - 6) != "r" &&
                                newNote.NoteLength != 0)
                            {
                                isNoteValid = true;
                                isValid = true;
                                newNote.NoteLyric = buffer.Substring(6, buffer.Length - 6);
                            }

                            if (trackCount == 0 && buffer.Contains("Tempo="))
                            {
                                Tempo newTempo = new Tempo();
                                try
                                {
                                    newTempo.PosTick = newNote.NoteTimeOn;
                                }
                                catch
                                {
                                    tempoTempFlag = true;
                                }

                                newTempo.BpmTimes100 =
                                    (int) (100 * double.Parse(buffer.Substring(6, buffer.Length - 6)));
                                TempoList.Add(newTempo);
                            }

                            if (buffer.Contains("NoteNum="))
                            {
                                newNote.NoteKey = Convert.ToInt32(buffer.Substring(8, buffer.Length - 8));
                            }
                        }
                    }
                }

                if (!isValid)
                    throw new NeutrinoStudioFileConverterFileException("The ust is invalid or empty.");
                foreach (Note note in newTrack.NoteList)
                {
                    note.NoteTimeOn += Constant.TickNumberForOneBar;
                    note.NoteTimeOff += Constant.TickNumberForOneBar;
                }

                if (trackCount == 0)
                {
                    foreach (Tempo tempo in TempoList)
                    {
                        if (TempoList.IndexOf(tempo) != 0)
                        {
                            tempo.PosTick += Constant.TickNumberForOneBar;
                        }
                    }
                }

                TrackList.Add(newTrack);
                trackCount++;
            }

            Lyric = new Lyric(this, true);
        }

        public void ImportCcs(List<string> fileNames)
        {
            if (fileNames.Count != 1)
                throw new NeutrinoStudioFileConverterOperationException("Cannot Import more than one ccs files.");
            bool isValid = false;
            XmlDocument ccs = new XmlDocument();
            ccs.Load(fileNames[0]);
            PreMeasure = 1;
            //Setup TrackList
            XmlElement scenario = (XmlElement) ccs.FirstChild.NextSibling;
            if (scenario is null)
                throw new NeutrinoStudioFileConverterFileException("Ccs file error at ccs.FirstChild.NextSibling");
            XmlElement scene =
                (XmlElement) ((XmlElement) scenario.GetElementsByTagName("Sequence")[0]).GetElementsByTagName("Scene")
                [0];
            XmlElement units = (XmlElement) scene.GetElementsByTagName("Units")[0];
            XmlElement groups = (XmlElement) scene.GetElementsByTagName("Groups")[0];
            ProjectName = Path.GetFileNameWithoutExtension(fileNames[0]);
            TrackList = new List<Track>();
            int trackNum = 0;
            foreach (XmlElement unit in units.GetElementsByTagName("Unit"))
            {
                if (unit.GetAttribute("Category") != "SingerSong") continue;
                TempoList = new List<Tempo>();
                XmlElement tempo =
                    (XmlElement) ((XmlElement) unit.GetElementsByTagName("Song")[0]).GetElementsByTagName(
                        "Tempo")[0];
                foreach (XmlElement sound in tempo.GetElementsByTagName("Sound"))
                {
                    Tempo newTempo = new Tempo
                    {
                        PosTick = int.Parse(sound.GetAttribute("Clock")) / 2,
                        BpmTimes100 = (int) (double.Parse(sound.GetAttribute("Tempo")) * 100)
                    };
                    TempoList.Add(newTempo);
                }

                TimeSigList = new List<TimeSig>();
                XmlElement beat =
                    (XmlElement) ((XmlElement) unit.GetElementsByTagName("Song")[0]).GetElementsByTagName(
                        "Beat")[0];
                int time = 0;
                int mes = 0;
                int beats = 4;
                int beatType = 4;
                foreach (XmlElement timeElement in beat.GetElementsByTagName("Time"))
                {
                    TimeSig newTimeSig = new TimeSig
                    {
                        PosMes = mes + (int.Parse(timeElement.GetAttribute("Clock")) - time) /
                            (beats * Constant.TickNumberForOneBeat * 8 / beatType)
                    };
                    mes = newTimeSig.PosMes;
                    time = int.Parse(timeElement.GetAttribute("Clock"));
                    newTimeSig.Nume = int.Parse(timeElement.GetAttribute("Beats"));
                    beats = newTimeSig.Nume;
                    newTimeSig.Denomi = int.Parse(timeElement.GetAttribute("BeatType"));
                    beatType = newTimeSig.Denomi;
                    TimeSigList.Add(newTimeSig);
                }

                Track newTrack = new Track {TrackNum = trackNum};
                string groupId = unit.GetAttribute("Group");
                foreach (XmlElement group in groups.GetElementsByTagName("Group"))
                {
                    if (group.GetAttribute("Id") == groupId)
                    {
                        newTrack.TrackName = group.GetAttribute("Name");
                    }
                }

                newTrack.NoteList = new List<Note>();
                int noteNum = 0;
                foreach (XmlElement note in ((XmlElement) ((XmlElement) unit.GetElementsByTagName("Song")[0])
                    .GetElementsByTagName("Score")[0]).GetElementsByTagName("Note"))
                {
                    Note newNote = new Note
                    {
                        NoteNum = noteNum, NoteTimeOn = int.Parse(note.GetAttribute("Clock")) / 2
                    };
                    newNote.NoteTimeOff = newNote.NoteTimeOn + int.Parse(note.GetAttribute("Duration")) / 2;
                    newNote.NoteKey = int.Parse(note.GetAttribute("PitchStep")) +
                                      (int.Parse(note.GetAttribute("PitchOctave")) + 1) * Constant.KeyForOneOctave;
                    newNote.NoteLyric = note.GetAttribute("Lyric");
                    noteNum++;
                    newTrack.NoteList.Add(newNote);
                }

                if (newTrack.NoteList.Count <= 0) continue;
                TrackList.Add(newTrack);
                trackNum++;
                isValid = true;
            }

            if (!isValid)
                throw new NeutrinoStudioFileConverterFileException("The ccs is invalid or empty.");
            Lyric = new Lyric(this, true);
        }

        public void ExportUst(string fileName)
        {
            for (int trackNum = 0; trackNum < TrackList.Count; trackNum++)
            {
                StringBuilder ustContents = new StringBuilder();
                ustContents.AppendLine("[#VERSION]");
                ustContents.AppendLine("UST Version1.2");
                ustContents.AppendLine("[#SETTING]");
                foreach (Tempo tempo in TempoList)
                    if (tempo.PosTick == 0)
                        ustContents.AppendLine("Tempo=" + tempo.Bpm.ToString("F2"));
                int pos = 0;
                int restCount = 0;
                ustContents.AppendLine("Tracks=1");
                ustContents.AppendLine("ProjectName=");
                ustContents.AppendLine("Mode2=True");
                for (int noteNum = 0; noteNum < TrackList[trackNum].NoteList.Count; noteNum++)
                {
                    Note thisNote = TrackList[trackNum].NoteList[noteNum];
                    if (pos < thisNote.NoteTimeOn)
                    {
                        ustContents.AppendLine("[#" + (noteNum + restCount).ToString("D4") + "]");
                        ustContents.AppendLine("Length=" + (thisNote.NoteTimeOn - pos));
                        ustContents.AppendLine("Lyric=R");
                        ustContents.AppendLine("NoteNum=60");
                        ustContents.AppendLine("PreUtterance=");
                        restCount++;
                    }

                    ustContents.AppendLine("[#" + (noteNum + restCount).ToString("D4") + "]");
                    ustContents.AppendLine("Length=" + thisNote.NoteLength);
                    ustContents.AppendLine("Lyric=" + thisNote.NoteLyric);
                    ustContents.AppendLine("NoteNum=" + thisNote.NoteKey);
                    ustContents.AppendLine("PreUtterance=");
                    pos = thisNote.NoteTimeOff;
                }

                ustContents.AppendLine("[#TRACKEND]");
                File.WriteAllText(
                    fileName + "\\" + ProjectName + "_" + trackNum + "_" + TrackList[trackNum].TrackName
                        .Replace("\\", "").Replace("/", "").Replace(".", "") + ".ust", ustContents.ToString(),
                    Encoding.GetEncoding("Shift-JIS"));
            }

            // MessageBox.Show(omitText + "Ust is successfully exported.", "Export Ust");
        }

        public void ExportVsq4(string fileName)
        {
            XmlDocument vsq4 = new XmlDocument();
            const string template =
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\r\n<vsq4 xmlns=\"http://www.yamaha.co.jp/vocaloid/schema/vsq4/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.yamaha.co.jp/vocaloid/schema/vsq4/ vsq4.xsd\">\r\n  <vender><![CDATA[Yamaha corporation]]></vender>\r\n  <version><![CDATA[4.0.0.3]]></version>\r\n  <vVoiceTable>\r\n    <vVoice>\r\n      <bs>0</bs>\r\n      <pc>0</pc>\r\n      <id><![CDATA[BCXDC6CZLSZHZCB4]]></id>\r\n      <name><![CDATA[VY2V3]]></name>\r\n      <vPrm>\r\n        <bre>0</bre>\r\n        <bri>0</bri>\r\n        <cle>0</cle>\r\n        <gen>0</gen>\r\n        <ope>0</ope>\r\n      </vPrm>\r\n    </vVoice>\r\n  </vVoiceTable>\r\n  <mixer>\r\n    <masterUnit>\r\n      <oDev>0</oDev>\r\n      <rLvl>0</rLvl>\r\n      <vol>0</vol>\r\n    </masterUnit>\r\n    <vsUnit>\r\n      <tNo>0</tNo>\r\n      <iGin>0</iGin>\r\n      <sLvl>-898</sLvl>\r\n      <sEnable>0</sEnable>\r\n      <m>0</m>\r\n      <s>0</s>\r\n      <pan>64</pan>\r\n      <vol>0</vol>\r\n    </vsUnit>\r\n    <monoUnit>\r\n      <iGin>0</iGin>\r\n      <sLvl>-898</sLvl>\r\n      <sEnable>0</sEnable>\r\n      <m>0</m>\r\n      <s>0</s>\r\n      <pan>64</pan>\r\n      <vol>0</vol>\r\n    </monoUnit>\r\n    <stUnit>\r\n      <iGin>0</iGin>\r\n      <m>0</m>\r\n      <s>0</s>\r\n      <vol>-129</vol>\r\n    </stUnit>\r\n  </mixer>\r\n  <masterTrack>\r\n    <seqName><![CDATA[Untitled0]]></seqName>\r\n    <comment><![CDATA[New VSQ File]]></comment>\r\n    <resolution>480</resolution>\r\n    <preMeasure>4</preMeasure>\r\n    <timeSig>\r\n      <m>0</m>\r\n      <nu>4</nu>\r\n      <de>4</de>\r\n    </timeSig>\r\n    <tempo>\r\n      <t>0</t>\r\n      <v>12000</v>\r\n    </tempo>\r\n  </masterTrack>\r\n  <vsTrack>\r\n    <tNo>0</tNo>\r\n    <name><![CDATA[Track]]></name>\r\n    <comment><![CDATA[Track]]></comment>\r\n    <vsPart>\r\n      <t>7680</t>\r\n      <playTime>61440</playTime>\r\n      <name><![CDATA[NewPart]]></name>\r\n      <comment><![CDATA[New Musical Part]]></comment>\r\n      <sPlug>\r\n        <id><![CDATA[ACA9C502-A04B-42b5-B2EB-5CEA36D16FCE]]></id>\r\n        <name><![CDATA[VOCALOID2 Compatible Style]]></name>\r\n        <version><![CDATA[3.0.0.1]]></version>\r\n      </sPlug>\r\n      <pStyle>\r\n        <v id=\"accent\">50</v>\r\n        <v id=\"bendDep\">8</v>\r\n        <v id=\"bendLen\">0</v>\r\n        <v id=\"decay\">50</v>\r\n        <v id=\"fallPort\">0</v>\r\n        <v id=\"opening\">127</v>\r\n        <v id=\"risePort\">0</v>\r\n      </pStyle>\r\n      <singer>\r\n        <t>0</t>\r\n        <bs>0</bs>\r\n        <pc>0</pc>\r\n      </singer>\r\n      <plane>0</plane>\r\n    </vsPart>\r\n  </vsTrack>\r\n  <monoTrack>\r\n  </monoTrack>\r\n  <stTrack>\r\n  </stTrack>\r\n  <aux>\r\n    <id><![CDATA[AUX_VST_HOST_CHUNK_INFO]]></id>\r\n    <content><![CDATA[VlNDSwAAAAADAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=]]></content>\r\n  </aux>\r\n</vsq4>";
            vsq4.LoadXml(template);
            XmlElement root = (XmlElement) vsq4.FirstChild.NextSibling;
            if (root is null)
                throw new NeutrinoStudioFileConverterFileException("Vsq4 file error at vsq4.FirstChild.NextSibling");
            XmlElement mixer = (XmlElement) root.GetElementsByTagName("mixer")[0];
            XmlElement masterTrack = (XmlElement) root.GetElementsByTagName("masterTrack")[0];
            XmlElement emptyTrack = (XmlElement) root.GetElementsByTagName("vsTrack")[0];
            XmlElement emptyUnit = (XmlElement) mixer.GetElementsByTagName("vsUnit")[0];
            XmlElement preMeasure = (XmlElement) masterTrack.GetElementsByTagName("preMeasure")[0];
            preMeasure.InnerText = PreMeasure.ToString();
            XmlElement firstTempo = (XmlElement) masterTrack.GetElementsByTagName("tempo")[0];
            firstTempo.GetElementsByTagName("v")[0].FirstChild.Value = TempoList[0].BpmTimes100.ToString();
            XmlElement firstTimeSig = (XmlElement) masterTrack.GetElementsByTagName("timeSig")[0];
            firstTimeSig.GetElementsByTagName("nu")[0].FirstChild.Value = TimeSigList[0].Nume.ToString();
            firstTimeSig.GetElementsByTagName("de")[0].FirstChild.Value = TimeSigList[0].Denomi.ToString();
            if (TempoList.Count > 1)
            {
                for (int i = 1; i < TempoList.Count; i++)
                {
                    XmlElement newTempo = (XmlElement) firstTempo.Clone();
                    newTempo.GetElementsByTagName("t")[0].FirstChild.Value = TempoList[i].PosTick.ToString();
                    newTempo.GetElementsByTagName("v")[0].FirstChild.Value = TempoList[i].BpmTimes100.ToString();
                    masterTrack.InsertAfter(newTempo, firstTempo);
                    firstTempo = newTempo;
                }
            }

            if (TimeSigList.Count > 1)
            {
                for (int i = 1; i < TimeSigList.Count; i++)
                {
                    XmlElement newTimeSig = (XmlElement) firstTimeSig.Clone();
                    newTimeSig.GetElementsByTagName("m")[0].FirstChild.Value = TimeSigList[i].PosMes.ToString();
                    newTimeSig.GetElementsByTagName("nu")[0].FirstChild.Value = TimeSigList[i].Nume.ToString();
                    newTimeSig.GetElementsByTagName("de")[0].FirstChild.Value = TimeSigList[i].Denomi.ToString();
                    masterTrack.InsertAfter(newTimeSig, firstTimeSig);
                    firstTimeSig = newTimeSig;
                }
            }

            if (TrackList.Count > 0)
            {
                for (int trackNum = 0; trackNum < TrackList.Count; trackNum++)
                {
                    XmlElement newTrack = (XmlElement) emptyTrack.Clone();
                    newTrack.GetElementsByTagName("tNo")[0].FirstChild.Value = (trackNum + 1).ToString();
                    emptyTrack.GetElementsByTagName("name")[0].FirstChild.Value = TrackList[trackNum].TrackName;
                    XmlElement part = (XmlElement) emptyTrack.GetElementsByTagName("vsPart")[0];
                    int pos = 0;
                    int mes = 0;
                    int nume = 4;
                    int denomi = 4;
                    foreach (TimeSig timeSig in TimeSigList)
                    {
                        if (timeSig.PosMes > PreMeasure)
                            break;
                        pos += (timeSig.PosMes - mes) * nume * Constant.TickNumberForOneBar / denomi;
                        mes = timeSig.PosMes;
                        nume = timeSig.Nume;
                        denomi = timeSig.Denomi;
                    }

                    pos += (PreMeasure - mes) * nume * Constant.TickNumberForOneBar / denomi;
                    part.GetElementsByTagName("t")[0].InnerText = pos.ToString();
                    int partStartTime = pos;
                    int time = 0;
                    Track thisTrack = TrackList[trackNum];
                    XmlElement lastNote = (XmlElement) part.GetElementsByTagName("singer")[0];
                    foreach (Note thisNote in thisTrack.NoteList)
                    {
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement note = vsq4.CreateElement("note", vsq4.DocumentElement.NamespaceURI);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement t = vsq4.CreateElement("t", vsq4.DocumentElement.NamespaceURI);
                        t.InnerText = (thisNote.NoteTimeOn - partStartTime).ToString();
                        note.AppendChild(t);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement dur = vsq4.CreateElement("dur", vsq4.DocumentElement.NamespaceURI);
                        dur.InnerText = thisNote.NoteLength.ToString();
                        note.AppendChild(dur);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement n = vsq4.CreateElement("n", vsq4.DocumentElement.NamespaceURI);
                        n.InnerText = thisNote.NoteKey.ToString();
                        note.AppendChild(n);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement v = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v.InnerText = "64";
                        note.AppendChild(v);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement y = vsq4.CreateElement("y", vsq4.DocumentElement.NamespaceURI);
                        XmlCDataSection y_cdata = vsq4.CreateCDataSection(thisNote.NoteLyric);
                        y.AppendChild(y_cdata);
                        note.AppendChild(y);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement p = vsq4.CreateElement("p", vsq4.DocumentElement.NamespaceURI);
                        XmlCDataSection p_cdata = vsq4.CreateCDataSection("a");
                        p.AppendChild(p_cdata);
                        note.AppendChild(p);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement nStyle = vsq4.CreateElement("nStyle", vsq4.DocumentElement.NamespaceURI);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement v1 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v1.SetAttribute("id", "accent");
                        v1.InnerText = "50";
                        nStyle.AppendChild(v1);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement v2 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v2.SetAttribute("id", "bendDep");
                        v2.InnerText = "0";
                        nStyle.AppendChild(v2);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement v3 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v3.SetAttribute("id", "bendLen");
                        v3.InnerText = "0";
                        nStyle.AppendChild(v3);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement v4 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v4.SetAttribute("id", "decay");
                        v4.InnerText = "50";
                        nStyle.AppendChild(v4);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement v5 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v5.SetAttribute("id", "fallPort");
                        v5.InnerText = "0";
                        nStyle.AppendChild(v5);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement v6 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v6.SetAttribute("id", "opening");
                        v6.InnerText = "127";
                        nStyle.AppendChild(v6);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement v7 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v7.SetAttribute("id", "risePort");
                        v7.InnerText = "0";
                        nStyle.AppendChild(v7);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement v8 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v8.SetAttribute("id", "vibLen");
                        v8.InnerText = "0";
                        nStyle.AppendChild(v8);
                        if (vsq4.DocumentElement is null)
                            throw new NeutrinoStudioFileConverterFileException(
                                "Vsq4 file error at vsq4.DocumentElement");
                        XmlElement v9 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v9.SetAttribute("id", "vibType");
                        v9.InnerText = "0";
                        nStyle.AppendChild(v9);
                        note.AppendChild(nStyle);
                        part.InsertAfter(note, lastNote);
                        lastNote = note;
                        time = thisNote.NoteTimeOff;
                    }

                    part.GetElementsByTagName("playTime")[0].InnerText = time.ToString();
                    root.InsertAfter(newTrack, emptyTrack);
                    emptyTrack = newTrack;
                    XmlElement newUnit = (XmlElement) emptyUnit.Clone();
                    newUnit.GetElementsByTagName("tNo")[0].FirstChild.Value = (trackNum + 1).ToString();
                    mixer.InsertAfter(newUnit, emptyUnit);
                    emptyUnit = newUnit;
                }

                root.RemoveChild(emptyTrack);
                mixer.RemoveChild(emptyUnit);
            }

            vsq4.Save(fileName);
            // MessageBox.Show("Vsqx is successfully exported." + Environment.NewLine + "If it only pronounces \"a\", please select all notes and run \"Lyrics\"→\"Convert Phonemes\". ", "Export Vsqx");
        }

        public void ExportVpr(string fileName)
        {
            const string template =
                "{\"version\":{\"major\":5,\"minor\":0,\"revision\":0},\"vender\":\"Yamaha Corporation\",\"title\":\"title\",\"masterTrack\":{\"samplingRate\":44100,\"loop\":{\"isEnabled\":true,\"begin\":0,\"end\":7680},\"tempo\":{\"isFolded\":false,\"height\":0.0,\"global\":{\"isEnabled\":false,\"value\":12000},\"events\":[{\"pos\":0,\"value\":12000}]},\"timeSig\":{\"isFolded\":false,\"events\":[{\"bar\":0,\"numer\":4,\"denom\":4}]},\"volume\":{\"isFolded\":false,\"height\":0.0,\"events\":[{\"pos\":0,\"value\":0}]}},\"voices\":[{\"compID\":\"BCXDC6CZLSZHZCB4\",\"name\":\"VY2V3\"}],\"tracks\":[{\"type\":0,\"name\":\"1 VOCALOID\",\"color\":0,\"busNo\":0,\"isFolded\":false,\"height\":0.0,\"volume\":{\"isFolded\":true,\"height\":40.0,\"events\":[{\"pos\":0,\"value\":0}]},\"panpot\":{\"isFolded\":true,\"height\":40.0,\"events\":[{\"pos\":0,\"value\":0}]},\"isMuted\":false,\"isSoloMode\":false,\"parts\":[{\"pos\":0,\"duration\":1920,\"styleName\":\"No Effect\",\"voice\":{\"compID\":\"BCXDC6CZLSZHZCB4\",\"langID\":0},\"midiEffects\":[{\"id\":\"SingingSkill\",\"isBypassed\":true,\"isFolded\":false,\"parameters\":[{\"name\":\"Amount\",\"value\":5},{\"name\":\"Name\",\"value\":\"75F04D2B-D8E4-44b8-939B-41CD101E08FD\"},{\"name\":\"Skill\",\"value\":5}]},{\"id\":\"VoiceColor\",\"isBypassed\":true,\"isFolded\":false,\"parameters\":[{\"name\":\"Air\",\"value\":0},{\"name\":\"Breathiness\",\"value\":0},{\"name\":\"Character\",\"value\":0},{\"name\":\"Exciter\",\"value\":0},{\"name\":\"Growl\",\"value\":0},{\"name\":\"Mouth\",\"value\":0}]},{\"id\":\"RobotVoice\",\"isBypassed\":true,\"isFolded\":false,\"parameters\":[{\"name\":\"Mode\",\"value\":1}]},{\"id\":\"DefaultLyric\",\"isBypassed\":true,\"isFolded\":false,\"parameters\":[{\"name\":\"CHS\",\"value\":\"a\"},{\"name\":\"ENG\",\"value\":\"Ooh\"},{\"name\":\"ESP\",\"value\":\"a\"},{\"name\":\"JPN\",\"value\":\"あ\"},{\"name\":\"KOR\",\"value\":\"아\"}]},{\"id\":\"Breath\",\"isBypassed\":true,\"isFolded\":false,\"parameters\":[{\"name\":\"Exhalation\",\"value\":5},{\"name\":\"Mode\",\"value\":1},{\"name\":\"Type\",\"value\":0}]}],\"notes\":[{\"lyric\":\"あ\",\"phoneme\":\"a\",\"isProtected\":false,\"pos\":480,\"duration\":480,\"number\":54,\"velocity\":64,\"exp\":{\"opening\":127},\"singingSkill\":{\"duration\":158,\"weight\":{\"pre\":64,\"post\":64}},\"vibrato\":{\"type\":0,\"duration\":0}}]}]}]}";
            Vpr vpr = JsonConvert.DeserializeObject<Vpr>(template);
            int endPos = 0;
            vpr.Title = ProjectName;
            vpr.MasterTrack.TimeSig.Events = TimeSigList.Select(it => new Vpr.VprTimeSigEvent
            {
                Bar = it.PosMes,
                Numer = it.Nume,
                Denom = it.Denomi
            }).ToList();
            endPos = Math.Max(endPos,
                vpr.MasterTrack.TimeSig.Events.Select(it => it.Bar * Constant.TickNumberForOneBar).Max());
            vpr.MasterTrack.Tempo.Events = TempoList.Select(it => new Vpr.VprTempoEvent
            {
                Pos = it.PosTick,
                Value = it.BpmTimes100
            }).ToList();
            endPos = Math.Max(endPos, vpr.MasterTrack.Tempo.Events.Select(it => it.Pos).Max());

            Vpr.VprTrack trackTemplate = vpr.Tracks.First();
            vpr.Tracks.Clear();
            foreach (Track track in TrackList)
            {
                Vpr.VprTrack newTrack = trackTemplate.CloneBlank();
                newTrack.Parts.First().Notes = track.NoteList.Select(it =>
                {
                    Vpr.VprNote newNote = trackTemplate.Parts.First().Notes.First().CloneBlank();
                    newNote.Pos = it.NoteTimeOn;
                    newNote.Duration = it.NoteTimeOff - it.NoteTimeOn;
                    newNote.Number = it.NoteKey;
                    newNote.Lyric = it.NoteLyric;
                    return newNote;
                }).ToList();
                int trackEndPos = track.NoteList.Last().NoteTimeOff;
                newTrack.Parts.First().Duration = trackEndPos;
                vpr.Tracks.Add(newTrack);
            }

            endPos = Math.Max(endPos, vpr.Tracks.Select(it => it.Parts.First().Duration).Max());
            vpr.MasterTrack.Loop.End = endPos;

            string json = JsonConvert.SerializeObject(vpr, new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });

            string tempDirectory = "temp/";
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }

            DirectoryInfo directory = Directory.CreateDirectory(tempDirectory);
            string jsonFileName = tempDirectory + "sequence.json";
            File.WriteAllText(jsonFileName, json);
            ZipUtil.ZipVpr(directory, fileName, jsonFileName);
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        public void ExportCcs(string fileName)
        {
            XmlDocument ccs = new XmlDocument();
            const string template =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Scenario Code=\"7251BC4B6168E7B2992FA620BD3E1E77\">\r\n  <Generation>\r\n    <Author Version=\"3.2.21.2\" />\r\n    <TTS Version=\"3.1.0\">\r\n      <Dictionary Version=\"1.4.0\" />\r\n      <SoundSources />\r\n    </TTS>\r\n    <SVSS Version=\"3.0.5\">\r\n      <Dictionary Version=\"1.0.0\" />\r\n      <SoundSources>\r\n        <SoundSource Version=\"1.0.0\" Id=\"XSV-JPF-W\" Name=\"緑咲 香澄\" />\r\n        <SoundSource Version=\"1.0.0\" Id=\"XSV-JPM-P\" Name=\"赤咲 湊\" />\r\n      </SoundSources>\r\n    </SVSS>\r\n  </Generation>\r\n  <Sequence Id=\"\">\r\n    <Scene Id=\"\">\r\n      <Units>\r\n        <Unit Version=\"1.0\" Id=\"\" Category=\"SingerSong\" Group=\"7de5f694-4b60-493d-b6b0-16f6b56deb1f\" StartTime=\"00:00:00\" Duration=\"00:00:02\" CastId=\"XSV-JPM-P\" Language=\"Japanese\">\r\n          <Song Version=\"1.02\">\r\n            <Tempo>\r\n              <Sound Clock=\"0\" Tempo=\"120\" />\r\n            </Tempo>\r\n            <Beat>\r\n              <Time Clock=\"0\" Beats=\"4\" BeatType=\"4\" />\r\n            </Beat>\r\n            <Score>\r\n              <Key Clock=\"0\" Fifths=\"0\" Mode=\"0\" />\r\n            </Score>\r\n          </Song>\r\n        </Unit>\r\n      </Units>\r\n      <Groups>\r\n        <Group Version=\"1.0\" Id=\"7de5f694-4b60-493d-b6b0-16f6b56deb1f\" Category=\"SingerSong\" Name=\"ソング 1\" Color=\"#FFAF1F14\" Volume=\"0\" Pan=\"0\" IsSolo=\"false\" IsMuted=\"false\" CastId=\"XSV-JPM-P\" Language=\"Japanese\" />\r\n      </Groups>\r\n      <SoundSetting Rhythm=\"4/4\" Tempo=\"78\" />\r\n    </Scene>\r\n  </Sequence>\r\n</Scenario>";
            ccs.LoadXml(template);
            XmlElement scenario = (XmlElement) ccs.FirstChild.NextSibling;
            XmlElement scene =
                (XmlElement) ((XmlElement) scenario.GetElementsByTagName("Sequence")[0]).GetElementsByTagName("Scene")
                [0];
            XmlElement units = (XmlElement) scene.GetElementsByTagName("Units")[0];
            XmlElement emptyUnit = (XmlElement) units.GetElementsByTagName("Unit")[0];
            XmlElement groups = (XmlElement) scene.GetElementsByTagName("Groups")[0];
            XmlElement emptyGroup = (XmlElement) groups.GetElementsByTagName("Group")[0];
            XmlElement allTempo =
                (XmlElement) ((XmlElement) emptyUnit.GetElementsByTagName("Song")[0]).GetElementsByTagName("Tempo")[0];
            XmlElement firstTempo = (XmlElement) allTempo.GetElementsByTagName("Sound")[0];
            firstTempo.SetAttribute("Tempo", TempoList[0].Bpm.ToString("F2"));
            for (int i = 1; i < TempoList.Count; i++)
            {
                XmlElement newTempo = (XmlElement) firstTempo.Clone();
                newTempo.SetAttribute("Tempo", TempoList[i].Bpm.ToString("F2"));
                newTempo.SetAttribute("Clock", (TempoList[i].PosTick * 2).ToString());
                allTempo.InsertAfter(newTempo, firstTempo);
                firstTempo = newTempo;
            }

            XmlElement allBeat =
                (XmlElement)
                ((XmlElement) ((XmlElement) units.GetElementsByTagName("Unit")[0]).GetElementsByTagName("Song")[0])
                .GetElementsByTagName("Beat")[0];
            XmlElement firstBeat = (XmlElement) allBeat.GetElementsByTagName("Time")[0];
            firstBeat.SetAttribute("Beats", TimeSigList[0].Nume.ToString());
            firstBeat.SetAttribute("BeatType", TimeSigList[0].Denomi.ToString());
            int pos = 0;
            for (int i = 1; i < TimeSigList.Count; i++)
            {
                XmlElement newBeat = (XmlElement) firstBeat.Clone();
                pos += (TimeSigList[i].PosMes - TimeSigList[i - 1].PosMes) * Constant.TickNumberForOneBeat * 8 *
                    TimeSigList[i - 1].Nume / TimeSigList[i - 1].Denomi;
                newBeat.SetAttribute("Clock", pos.ToString());
                newBeat.SetAttribute("Beats", TimeSigList[i].Nume.ToString());
                newBeat.SetAttribute("BeatType", TimeSigList[i].Denomi.ToString());
                allBeat.InsertAfter(newBeat, firstBeat);
                firstBeat = newBeat;
            }

            var idList = new List<string>();
            Random idRandom = new Random();
            for (int trackNum = 0; trackNum < TrackList.Count; trackNum++)
            {
                Track thisTrack = TrackList[trackNum];
                XmlElement newUnit = (XmlElement) emptyUnit.Clone();
                XmlElement newGroup = (XmlElement) emptyGroup.Clone();
                while (idList.Count <= trackNum)
                {
                    string id = emptyUnit.GetAttribute("Group");
                    id = id.Remove(30, 6);
                    id += idRandom.Next(999999).ToString("D6");
                    if (!idList.Contains(id))
                    {
                        idList.Add(id);
                    }
                }

                emptyUnit.SetAttribute("Group", idList[trackNum]);
                emptyGroup.SetAttribute("Id", idList[trackNum]);
                emptyGroup.SetAttribute("Name", thisTrack.TrackName);
                XmlElement song = (XmlElement) emptyUnit.GetElementsByTagName("Song")[0];
                XmlElement tempo = (XmlElement) song.GetElementsByTagName("Tempo")[0];
                XmlElement beat = (XmlElement) song.GetElementsByTagName("Beat")[0];
                XmlElement score = (XmlElement) song.GetElementsByTagName("Score")[0];
                song.ReplaceChild(allTempo.Clone(), tempo);
                song.ReplaceChild(allBeat.Clone(), beat);
                foreach (Note thisNote in thisTrack.NoteList)
                {
                    if (ccs.DocumentElement is null)
                        throw new NeutrinoStudioFileConverterFileException("Ccs file error at ccs.DocumentElement");
                    XmlElement note = ccs.CreateElement("Note", ccs.DocumentElement.NamespaceURI);
                    note.SetAttribute("Clock", (thisNote.NoteTimeOn * 2).ToString());
                    note.SetAttribute("PitchStep", (thisNote.NoteKey % Constant.KeyForOneOctave).ToString());
                    note.SetAttribute("PitchOctave", (thisNote.NoteKey / Constant.KeyForOneOctave - 1).ToString());
                    note.SetAttribute("Duration", (thisNote.NoteLength * 2).ToString());
                    note.SetAttribute("Lyric", thisNote.NoteLyric);
                    score.AppendChild(note);
                }

                int posTick = 0;
                int posSecond = 0;
                int lastTick = thisTrack.NoteList[thisTrack.NoteList.Count - 1].NoteTimeOff;
                for (int j = 1; j < TempoList.Count; j++)
                {
                    posTick = TempoList[j].PosTick;
                    posSecond +=
                        (int) ((TempoList[j].PosTick - TempoList[j - 1].PosTick) / 8 / TempoList[j - 1].Bpm + 1);
                }

                if (posTick < lastTick)
                {
                    posSecond += (int) ((lastTick - posTick) / 8 / TempoList[TempoList.Count - 1].Bpm + 1);
                }

                TimeSpan timeSpan = new TimeSpan(0, 0, posSecond);
                emptyUnit.SetAttribute("Duration", timeSpan.ToString("c"));
                units.AppendChild(newUnit);
                groups.AppendChild(newGroup);
                emptyUnit = newUnit;
                emptyGroup = newGroup;
            }

            units.RemoveChild(emptyUnit);
            groups.RemoveChild(emptyGroup);
            ccs.Save(fileName);
            // MessageBox.Show("Ccs is successfully exported.", "ExportCcs");
        }

        public void ExportMusicXml(string filename)
        {
            string scorePrefix = $"<?xml version=\"1.0\" encoding=\"UTF-8\" ?><!DOCTYPE score-partwise PUBLIC \"-//Recordare//DTD MusicXML 3.1 Partwise//EN\" \"http://www.musicxml.org/dtds/partwise.dtd\"><score-partwise version=\"3.1\"><identification><encoding><software>NEUTRINO Studio - NeutrinoStudio.FileConverter.Core {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}</software><encoding-date>{DateTime.Now:yyyy-MM-dd}</encoding-date></encoding></identification><part>";
            string measureSuffix = "</measure>";
            string scoreSuffix = "</part></score-partwise>";

            string tempoResult = "";
            foreach (Tempo tempo in TempoList)
                if (tempo.PosTick == 0)
                    tempoResult = tempo.Bpm.ToString("F2");

            string measurePrefix = $"<measure><direction><sound tempo=\"{tempoResult}\"/></direction><attributes><divisions>480</divisions><time><beats>{TimeSigList[0].Nume}</beats><beat-type>{TimeSigList[0].Denomi}</beat-type></time></attributes>";

            StringBuilder musicXml = new StringBuilder();
            musicXml.Append(scorePrefix);
            foreach (Track track in TrackList)
            {
                musicXml.Append(measurePrefix);
                int pos = 0;
                int measure = 0;
                foreach (Note thisNote in track.NoteList)
                {

                    if (measure >= 1920)
                    {
                        musicXml.Append(measureSuffix);
                        musicXml.Append(measurePrefix);
                        measure = 0;
                    }

                    if (pos < thisNote.NoteTimeOn)
                    {
                        int duration = thisNote.NoteTimeOn - pos;
                        while (duration > 1920)
                        {
                            musicXml.Append(
                                "<note><rest/><duration>1920</duration><type>whole</type><voice>1</voice></note>");
                            musicXml.Append(measureSuffix);
                            musicXml.Append(measurePrefix);
                            duration -= 1920;
                        }
                        musicXml.Append(
                            $"<note><rest/><duration>{duration}</duration><type>whole</type><voice>1</voice></note>");
                        measure += duration;
                    }

                    if (measure >= 1920)
                    {
                        musicXml.Append(measureSuffix);
                        musicXml.Append(measurePrefix);
                        measure = 0;
                    }

                    string step = Constant.KeyList[thisNote.NoteKey % Constant.KeyForOneOctave];
                    int octave = thisNote.NoteKey / Constant.KeyForOneOctave - 1;
                    int alter = Constant.AlterList[thisNote.NoteKey % Constant.KeyForOneOctave];
                    musicXml.Append(
                        $"<note><pitch><step>{step}</step><octave>{octave}</octave><alter>{alter}</alter></pitch><duration>{thisNote.NoteLength}</duration><type>whole</type><voice>1</voice><staff>1</staff><lyric default-y=\"-77\"><text>{thisNote.NoteLyric}</text></lyric></note>");
                    measure += thisNote.NoteLength;
                    pos = thisNote.NoteTimeOff;

                }

                musicXml.Append(measureSuffix);
                musicXml.Append(measurePrefix);
                musicXml.Append(
                    "<note><rest/><duration>1920</duration><voice>1</voice></note>");
                musicXml.Append(measureSuffix);

            }

            musicXml.Append(scoreSuffix);
            File.WriteAllText(filename, musicXml.ToString(), new UTF8Encoding(false));
        }
    }
}
