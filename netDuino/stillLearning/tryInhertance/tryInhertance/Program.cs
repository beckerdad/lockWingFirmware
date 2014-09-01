public class Dog
{
    public string Name;

    public Dog(string name)
    {
//        Console.WriteLine("In Dog constructor");
        Name = name;
    }
}

public class Terrier : Dog
{
    public string Temperament;

    public Terrier(string name, string temperament)
        : base(name)
    {
//        Console.WriteLine("In Terrier constructor");
        Temperament = temperament;
    }
}
