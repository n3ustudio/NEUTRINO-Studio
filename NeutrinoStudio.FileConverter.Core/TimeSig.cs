﻿namespace NeutrinoStudio.FileConverter.Core
{

    public class TimeSig
    {

        public TimeSig()
        {

        }

        public TimeSig(TimeSig timeSig)
        {

            PosMes = timeSig.PosMes;
            Nume = timeSig.Nume;
            Denomi = timeSig.Denomi;

        }

        public int PosMes;

        public int Nume;

        public int Denomi;

    }

}
