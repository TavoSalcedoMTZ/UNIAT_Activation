public enum CareerType
{
    IPV, //Ingenieria en Programacion de Videojuegos
    LAV, //Licenciatura en Arte para Videojuegos
    CAD, //Cine y Animacion Digital
    CS   //Ciberseguridad
}

public static class CareerTypeExtensions
{
    public static string GetDisplayName(this CareerType _career)
    {
        return _career switch
        {
            CareerType.IPV => "Ingeniería en Programación de Videojuegos",
            CareerType.LAV => "Licenciatura en Arte para Videojuegos",
            CareerType.CAD => "Cine y Animación Digital",
            CareerType.CS => "Ciberseguridad",
            _ => _career.ToString()
        };
    }
}
