// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RunningTotal.Model.SubTypes
{

    public class Distance
    {

        [JsonProperty("timestamp")]
        public double Timestamp { get; set; }

        [JsonProperty("distance")]
        public double Meters { get; set; }
    }
}
