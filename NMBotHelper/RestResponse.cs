using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMBotHelper
{

    public class RestResponse
    {
        [JsonProperty("answers")]
        public Answer[] Answers { get; set; }
    }

    public class Answer
    {
        public string[] questions { get; set; }
        public string answer { get; set; }
        public float score { get; set; }
        public int id { get; set; }
        public string source { get; set; }
        public Metadata[] metadata { get; set; }
    }

    public class Metadata
    {
        public string name { get; set; }
        public string value { get; set; }
    }

}
