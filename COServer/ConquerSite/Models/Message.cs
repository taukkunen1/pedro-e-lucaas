namespace ConquerSite.Models
{
    public class Message
    {
        public TypeMessage Type { get; set; }
        public string Text { get; set; }
    }

    public enum TypeMessage
    {
        Primary,
        Secundary,
        Success,
        Danger,
        Warning,
        Info,
        Light,
        Dark,
    }
}
