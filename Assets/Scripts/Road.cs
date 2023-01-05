public class Road
{
    public byte Owner => _owner;
    public CityModel[] Cities => _cities;
    public byte Index => _index;

    private readonly byte _index;
    private readonly CityModel[] _cities;
    private byte _owner;

    public Road(byte index, CityModel[] cities, byte owner)
    {
        _index = index;
        _cities = cities;
        _owner = owner;
    }
}