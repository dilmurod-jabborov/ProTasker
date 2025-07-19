namespace ProTasker.Menu
{
    public class WorkerSession
    {
        public string CurrentStep { get; set; }
        public string Mode {  get; set; }
        public Dictionary<string, string> Data { get; set; } = new();
    }
}