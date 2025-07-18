namespace ProTasker.Menu.AdminUIFolder
{
    public class AdminSession
    {
        public string CurrentStep { get; set; }
        public string Mode { get; set; }
        public Dictionary<string, string> Data { get; set; } = new();

    }
}
