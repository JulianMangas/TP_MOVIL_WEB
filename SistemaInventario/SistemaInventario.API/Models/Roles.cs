namespace SistemaInventario.API.Models
{
    public static class Roles
    {
        public const string Administrador = "Administrador";
        public const string Cajero = "Cajero";
        public const string Usuario = "Usuario";
        public const string CajeroOAdmin = Cajero + "," + Administrador;

        public static readonly string[] Todos = { Administrador, Cajero, Usuario };
    }

    public static class ConsumidorFinal
    {
        public const string Nombre = "Consumidor";
        public const string Apellido = "Final";
    }
}