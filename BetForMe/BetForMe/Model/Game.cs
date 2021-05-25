﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetForMe.Model {
    public class Game {
        public string Div { get; set; }
        public string Season { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public Nullable<long> FTHG { get; set; }
        public Nullable<long> FTAG { get; set; }
        public string FTR { get; set; }
        public Nullable<long> HTHG { get; set; }
        public Nullable<long> HTAG { get; set; }
        public string HTR { get; set; }
        public string Referee { get; set; }
        public Nullable<long> HS { get; set; }
        public Nullable<long> AS_ { get; set; }
        public Nullable<long> HST { get; set; }
        public Nullable<long> AST { get; set; }
        public Nullable<long> HF { get; set; }
        public Nullable<long> AF { get; set; }
        public Nullable<long> HC { get; set; }
        public Nullable<long> AC { get; set; }
        public Nullable<long> HY { get; set; }
        public Nullable<long> AY { get; set; }
        public Nullable<long> HR { get; set; }
        public Nullable<long> AR { get; set; }
        public Nullable<double> B365H { get; set; }
        public Nullable<double> B365D { get; set; }
        public Nullable<double> B365A { get; set; }
        public Nullable<double> BWH { get; set; }
        public Nullable<double> BWD { get; set; }
        public Nullable<double> BWA { get; set; }
        public Nullable<double> IWH { get; set; }
        public Nullable<double> IWD { get; set; }
        public Nullable<double> IWA { get; set; }
        public Nullable<double> LBH { get; set; }
        public Nullable<double> LBD { get; set; }
        public Nullable<double> LBA { get; set; }
        public Nullable<double> PSH { get; set; }
        public Nullable<double> PSD { get; set; }
        public Nullable<double> PSA { get; set; }
        public Nullable<double> WHH { get; set; }
        public Nullable<double> WHD { get; set; }
        public Nullable<double> WHA { get; set; }
        public Nullable<double> SJH { get; set; }
        public Nullable<double> SJD { get; set; }
        public Nullable<double> SJA { get; set; }
        public Nullable<double> VCH { get; set; }
        public Nullable<double> VCD { get; set; }
        public Nullable<double> VCA { get; set; }
        public Nullable<double> Bb1X2 { get; set; }
        public Nullable<double> BbMxH { get; set; }
        public Nullable<double> BbAvH { get; set; }
        public Nullable<double> BbMxD { get; set; }
        public Nullable<double> BbAvD { get; set; }
        public Nullable<double> BbMxA { get; set; }
        public Nullable<double> BbAvA { get; set; }
        public Nullable<double> BbOU { get; set; }
        public Nullable<double> BbMxGreaterThan2_5 { get; set; }
        public Nullable<double> BbAvGreaterThan2_5 { get; set; }
        public Nullable<double> BbMxLowerThan2_5 { get; set; }
        public Nullable<double> BbAvLowerThan2_5 { get; set; }
        public Nullable<double> BbAH { get; set; }
        public Nullable<double> BbAHh { get; set; }
        public Nullable<double> BbMxAHH { get; set; }
        public Nullable<double> BbAvAHH { get; set; }
        public Nullable<double> BbMxAHA { get; set; }
        public Nullable<double> BbAvAHA { get; set; }
        public Nullable<double> PSCH { get; set; }
        public Nullable<double> PSCD { get; set; }
        public Nullable<double> PSCA { get; set; }
        public long ID { get; set; }
    }

    public partial class England : Game {
    }

    public partial class Spain : Game {
    }

    public partial class Germany : Game {
    }
}
