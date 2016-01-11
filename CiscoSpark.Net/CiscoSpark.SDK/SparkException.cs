using System;

namespace CiscoSpark.SDK
{
    public class SparkException : Exception
    {
        public SparkException() { }
            
        public SparkException(string message) : base(message) { }
        public SparkException(Exception exception) : base(exception.Message) { }

        public SparkException(string message, Exception exception):base(message,exception){}
    }
}