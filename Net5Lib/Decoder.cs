using System;

namespace Net5Lib
{
    public class Decoder
    {
        public string GetRandomString()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
