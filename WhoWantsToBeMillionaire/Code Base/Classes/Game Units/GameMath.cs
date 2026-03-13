namespace WhoWantsToBeMillionaire
{
    static class GameMath
    {
        public static int Clamp(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
}
