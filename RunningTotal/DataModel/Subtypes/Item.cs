// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace RunningTotal.Model.SubTypes
{

    public class Item
    {

        [JsonProperty("duration")]
        public double Duration { get; set; }

        [JsonProperty("total_distance")]
        public double TotalDistanceInMeters { get; set; }

        [JsonProperty("start_time")]
        public string StartTime { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        #region CustomCode


        public DateTime StartTimeAsDateTime
        {
            get { return DateTime.Parse(this.StartTime); }
        }
        

        public double TotalDistanceInMiles
        {
            get
            {
                return (TotalDistanceInMeters / 1609.344);
            }
        }

        public string DistanceMilesRounded
        {
            get
            {
                return string.Format("{0:0.0}", this.TotalDistanceInMiles);
            }
        }

        public string DistanceShortSummaryMiles
        {
            get
            {
                return string.Format("{0:0.0} mi", this.TotalDistanceInMiles);
            }
        }

        public string FormattedRunDate
        {
            get
            {
                return DateTime.Parse(this.StartTime).ToString("dddd MMM d");
            }
        }

        public string FormattedDuration
        {
            get
            {
                var t = TimeSpan.FromSeconds(this.Duration);

                var s = new StringBuilder();

                if (t.Hours > 0)
                    s.AppendFormat("{0} hours, ", t.Hours);

                s.AppendFormat("{0} minutes, {1} seconds", t.Minutes, t.Seconds);

                return s.ToString();
            }
        }

        public string ShortFormattedDuration
        {
            get
            {
                var t = TimeSpan.FromSeconds(this.Duration);

                var s = new StringBuilder();

                if (t.Hours > 0)
                    s.AppendFormat("{0:0}:", t.Hours);

                s.AppendFormat("{0:0}:{1:00}", t.Minutes, t.Seconds);

                return s.ToString();
            }
        }

        #endregion
    }
}
