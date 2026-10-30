namespace SistemaInventario.API.Helpers
{
    public static class Hora
    {
        // Argentina es UTC-3 todo el año (no tiene horario de verano)
        public static DateTime Ahora => DateTime.UtcNow.AddHours(-3);
    }
}