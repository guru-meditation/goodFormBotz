using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanners
{
    public abstract class Data
    {
    }

    public class FullTimeResult : Data
    {
        public string One;
        public string X;
        public string Two;
    }

    public class HalfTimeResult : Data
    {
        public string One;
        public string X;
        public string Two;
    }

    public class DoubleChance : Data
    {
        public string OneX;
        public string XTwo;
        public string OneTwo;
    }

    public class NthGoal : Data
    {
        public string Team1;
        public string NoGoal;
        public string Team2;
    }

    public class MatchGoals : Data
    {
        public decimal NumberOfGoals;
        public string Over;
        public string Under;
    }

    public class CardsAndCorners : Data
    {
    }
}
