using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotSpace
{
    public class WebBlob : ICloneable
    {
        public string HomeTeam;
        public string AwayTeam;
        public string League;
        public string Url;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
