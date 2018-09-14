using System.Drawing;

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
        public const string ColorPrimary = "#174276";
        public const string ColorPrimaryInvert = "#76171c";
    }
}
