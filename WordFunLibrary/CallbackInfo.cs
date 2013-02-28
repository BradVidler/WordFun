using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace WordFunLibrary
{
        [DataContract]
        public class CallbackInfo
        {
            [DataMember]
            public int numWords { get; private set; }
            [DataMember]
            public List<char> letters { get; private set; }
            [DataMember]
            public bool reset { get; private set; }
            [DataMember]
            public string details { get; private set; }
            [DataMember]
            public bool isNewGame { get; private set; }
            [DataMember]
            public List<Player> pStats { get; private set; }
            [DataMember]
            public List<string> words { get; private set; }
            [DataMember]
            public int secondsLeft { get; private set; }


            public CallbackInfo(int w, List<char> l, bool r, string d, bool n, List<Player> p, List<string> ww, int s)
            {
                details = d;
                letters = l;
                numWords = w;
                reset = r;
                isNewGame = n;
                pStats = p;
                words = ww;
                secondsLeft = s;
            }
        }
}
