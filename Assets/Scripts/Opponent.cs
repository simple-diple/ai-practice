public static class Opponent
{
    public static byte Get(byte player)
    {
        return player == 1 ? (byte)2 : (byte)1;
    }
}