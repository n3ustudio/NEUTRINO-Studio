﻿using System;
using System.Runtime.Serialization;

namespace NeutrinoStudio.FileConverter.Core
{

    public class NeutrinoStudioFileConverterDeprecatedException : Exception
    {

        private NeutrinoStudioFileConverterDeprecatedException()
        {
        }

        protected NeutrinoStudioFileConverterDeprecatedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public NeutrinoStudioFileConverterDeprecatedException(string message) : base(message)
        {
        }

        public NeutrinoStudioFileConverterDeprecatedException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }

    public class NeutrinoStudioFileConverterOperationException : Exception
    {

        private NeutrinoStudioFileConverterOperationException()
        {
        }

        protected NeutrinoStudioFileConverterOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public NeutrinoStudioFileConverterOperationException(string message) : base(message)
        {
        }

        public NeutrinoStudioFileConverterOperationException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }

    public class NeutrinoStudioFileConverterFileException : Exception
    {

        private NeutrinoStudioFileConverterFileException()
        {
        }

        protected NeutrinoStudioFileConverterFileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public NeutrinoStudioFileConverterFileException(string message) : base(message)
        {
        }

        public NeutrinoStudioFileConverterFileException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }

}
