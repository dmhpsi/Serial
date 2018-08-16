namespace Serial
{
    class ComboObject
    {
        string text;
        public object value;

        public ComboObject(string text, object value)
        {
            this.text = text;
            this.value = value;
        }

        public override string ToString()
        {
            return text;
        }
    }
    static class Constants
    {
        public const int saveInterval = 5; //seconds
        //public const long dataInterval = 600; //seconds
        //public const long historyInterval = 7200; //seconds
    }
}
