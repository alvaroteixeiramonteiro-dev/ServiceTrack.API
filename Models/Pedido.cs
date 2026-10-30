namespace ServiceTrack.API.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string NomeContato { get; set; } = string.Empty;
        public string EmailContato { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Prioridade { get; set; } = "Normal";
        public string Estado { get; set; } = "Aberto";

        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
        public DateTime? ResolvidoEm { get; set; }

    }
}
