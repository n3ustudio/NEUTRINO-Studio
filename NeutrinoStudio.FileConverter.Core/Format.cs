using System;
using System.Globalization;

namespace NeutrinoStudio.FileConverter.Core
{

    public enum InputFormat
    {
        Undefined = 0,
        MusicXml = 1,
        Xml = 2,
        Mxl = 3,
        Vsq2 = 4,
        Vsq3 = 5,
        Vsq4 = 6,
        Vpr = 7,
        Ust = 8,
        Ccs = 9
    }

    public enum OutputFormat
    {
        Undefined = 0,
        Wav = 1,
        Mp3 = 2,
        Flac = 3
    }

}
