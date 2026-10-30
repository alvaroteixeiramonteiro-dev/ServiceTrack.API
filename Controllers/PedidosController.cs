using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using ServiceTrack.API.Models;

namespace ServiceTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        // Guarda a configuração da ligação ao MySQL.
        private readonly string _connectionString;

        public PedidosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "A ligação 'DefaultConnection' não está configurada.");
        }
        // GET: api/pedidos
        [HttpGet]
        public async Task<IActionResult> ObterTodos()
        {
            // Guarda os pedidos que forem lidos da base de dados.
            var lista = new List<Pedido>();

            // Cria e abre a ligação ao MySQL.
            using var conexao = new MySqlConnection(_connectionString);
            await conexao.OpenAsync();

            // Seleciona os campos da tabela, começando pelos pedidos mais recentes.
            string query = @"SELECT id, titulo, descricao, nome_contato,
                        email_contato, categoria, prioridade, estado,
                        criado_em, atualizado_em, resolvido_em
                 FROM pedidos
                 ORDER BY criado_em DESC, id DESC";

            // Associa a consulta à ligação aberta.
            using var comando = new MySqlCommand(query, conexao);

            // Executa a consulta e permite ler as linhas devolvidas.
            using var leitor = await comando.ExecuteReaderAsync();

            // Lê uma linha de cada vez até terminar os resultados.
            while (await leitor.ReadAsync())
            {
                var pedido = new Pedido
                {
                    Id = leitor.GetInt32("id"),
                    Titulo = leitor.GetString("titulo"),
                    Descricao = leitor.GetString("descricao"),
                    NomeContato = leitor.GetString("nome_contato"),
                    EmailContato = leitor.GetString("email_contato"),
                    Categoria = leitor.GetString("categoria"),
                    Prioridade = leitor.GetString("prioridade"),
                    Estado = leitor.GetString("estado"),
                    CriadoEm = leitor.GetDateTime("criado_em"),
                    AtualizadoEm = leitor.GetDateTime("atualizado_em"),
                    ResolvidoEm = leitor.IsDBNull(leitor.GetOrdinal("resolvido_em"))
                        ? (DateTime?)null
                        : leitor.GetDateTime("resolvido_em")
                };

                lista.Add(pedido);
            }
            
            return Ok(lista);
        }
        // GET: api/pedidos/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            using var conexao = new MySqlConnection(_connectionString);
            await conexao.OpenAsync();

            string query = @"SELECT id, titulo, descricao, nome_contato,
                            email_contato, categoria, prioridade, estado,
                            criado_em, atualizado_em, resolvido_em
                     FROM pedidos
                     WHERE id = @id";

            using var comando = new MySqlCommand(query, conexao);

            // Envia o identificador como parâmetro, separado do SQL.
            comando.Parameters.AddWithValue("@id", id);

            using var leitor = await comando.ExecuteReaderAsync();

            if (await leitor.ReadAsync())
            {
                var pedido = new Pedido
                {
                    Id = leitor.GetInt32("id"),
                    Titulo = leitor.GetString("titulo"),
                    Descricao = leitor.GetString("descricao"),
                    NomeContato = leitor.GetString("nome_contato"),
                    EmailContato = leitor.GetString("email_contato"),
                    Categoria = leitor.GetString("categoria"),
                    Prioridade = leitor.GetString("prioridade"),
                    Estado = leitor.GetString("estado"),
                    CriadoEm = leitor.GetDateTime("criado_em"),
                    AtualizadoEm = leitor.GetDateTime("atualizado_em"),
                    ResolvidoEm = leitor.IsDBNull(leitor.GetOrdinal("resolvido_em"))
                        ? (DateTime?)null
                        : leitor.GetDateTime("resolvido_em")
                };
                return Ok(pedido);
            }

            // Executa apenas quando a consulta não encontra o pedido.
            return NotFound(new { mensagem = "Pedido não encontrado." });
        }
    }
}
