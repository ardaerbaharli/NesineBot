using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesineBot
{
    class Bet
    {
        private string matchCode;
        private string matchName;
        private int mbs;
        private string betType;
        private float rate;
        private int playedCount;
        private DateTime date;

        public string MatchCode { get => matchCode; set => matchCode = value; }
        public string MatchName { get => matchName; set => matchName = value; }
        public int Mbs { get => mbs; set => mbs = value; }
        public string BetType { get => betType; set => betType = value; }
        public float Rate { get => rate; set => rate = value; }
        public int PlayedCount { get => playedCount; set => playedCount = value; }
        public DateTime Date { get => date; set => date = value; }
    }
}
