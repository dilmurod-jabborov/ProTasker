namespace ProTasker.Menu.UserUIFolder
{
    public class UserSession
    {
        public string CurrentStep {  get; set; }
        public string Mode {  get; set; }
        public Dictionary<string, string> Data { get; set; } = new();
    }
}