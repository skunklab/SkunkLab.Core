namespace Piraeus.Grains.Notifications
{
    public static class RangeIncrementerExtension
    {
        public static int RangeIncrement(this int value, int inclusiveMinimum, int inclusiveMaximum)
        {
            value++;

            if (value < inclusiveMinimum || value > inclusiveMaximum)
            {
                value = inclusiveMinimum;
            }

            return value;
        }

    }
}
