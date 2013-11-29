namespace VLC_WINRT.Model
{
    public class Subtitle
    {
        public int Id;
        public string Name;

        public override string ToString()
        {
            return Id + ": " + Name;
        }
    }
}