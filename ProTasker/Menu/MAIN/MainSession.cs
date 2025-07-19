using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTasker.Menu.MAIN
{
    public class MainSession
    {
        public string Mode {  get; set; }
        public string CurrentStep { get; set; } = "start";
        public Dictionary<string, string> Data { get; set; } = new();
    }
}
