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
        // POST: api/pedidos
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] Pedido novoPedido)
        {
            // Impede a criação de um pedido sem título.
            if (string.IsNullOrWhiteSpace(novoPedido.Titulo))
            {
                return BadRequest(new { mensagem = "O título é obrigatório." });
            }

            if (string.IsNullOrWhiteSpace(novoPedido.Descricao))
            {
                return BadRequest(new { mensagem = "A descrição é obrigatória." });
            }

            if (string.IsNullOrWhiteSpace(novoPedido.NomeContato))
            { 
                return BadRequest(new { mensagem = "O nome do contacto é obrigatório." });
            }

            if (string.IsNullOrWhiteSpace(novoPedido.EmailContato))
            {
                return BadRequest(new { mensagem = "O email do contacto é obrigatório." });
            }

            if (string.IsNullOrWhiteSpace(novoPedido.Categoria))
            {
                return BadRequest(new { mensagem = "A categoria é obrigatória." });
            }

            // Respeita os limites definidos nas colunas VARCHAR da tabela.
            if (novoPedido.Titulo.Length > 255 ||
                novoPedido.NomeContato.Length > 255 ||
                novoPedido.EmailContato.Length > 255 ||
                novoPedido.Categoria.Length > 50)
            {
                return BadRequest(new 
                {
                    mensagem = "O título, nome e email podem ter até 255 caracteres; a categoria, até 50."
                });
                
            }

            if (novoPedido.Prioridade != "Baixa" &&
                novoPedido.Prioridade != "Normal" &&
                novoPedido.Prioridade != "Alta")
            {
                return BadRequest(new { mensagem = "A prioridade deve ser Baixa, Normal ou Alta." });
            }

            if (novoPedido.Estado != "Aberto" &&
                novoPedido.Estado != "Em curso" &&
                novoPedido.Estado != "Resolvido")
            {
                return BadRequest(new { mensagem = "O estado deve ser Aberto, Em curso ou Resolvido." });
            }

            novoPedido.CriadoEm = DateTime.UtcNow;
            novoPedido.AtualizadoEm = novoPedido.CriadoEm;

            novoPedido.ResolvidoEm = novoPedido.Estado == "Resolvido"
                ? novoPedido.CriadoEm
                : (DateTime?)null;

            using var conexao = new MySqlConnection(_connectionString);
            await conexao.OpenAsync();

            // Insere o pedido e consulta o identificador gerado pelo MySQL.
            string query = @"INSERT INTO pedidos (
                    titulo, descricao, nome_contato, email_contato,
                    categoria, prioridade, estado,
                    criado_em, atualizado_em, resolvido_em
                 )
                 VALUES (
                    @titulo, @descricao, @nome_contato, @email_contato,
                    @categoria, @prioridade, @estado,
                    @criado_em, @atualizado_em, @resolvido_em
                 );
                 SELECT LAST_INSERT_ID();";

            using var comando = new MySqlCommand(query, conexao);

            comando.Parameters.AddWithValue("@titulo", novoPedido.Titulo);
            comando.Parameters.AddWithValue("@descricao", novoPedido.Descricao);
            comando.Parameters.AddWithValue("@nome_contato", novoPedido.NomeContato);
            comando.Parameters.AddWithValue("@email_contato", novoPedido.EmailContato);
            comando.Parameters.AddWithValue("@categoria", novoPedido.Categoria);
            comando.Parameters.AddWithValue("@prioridade", novoPedido.Prioridade);
            comando.Parameters.AddWithValue("@estado", novoPedido.Estado);
            comando.Parameters.AddWithValue("@criado_em", novoPedido.CriadoEm);
            comando.Parameters.AddWithValue("@atualizado_em", novoPedido.AtualizadoEm);

            comando.Parameters.AddWithValue("@resolvido_em", (object?)novoPedido.ResolvidoEm ?? DBNull.Value);

            var idGerado = Convert.ToInt32(await comando.ExecuteScalarAsync());
            novoPedido.Id = idGerado;

            return CreatedAtAction(nameof(ObterPorId), new { id = novoPedido.Id }, novoPedido);
        }

        // PUT: api/pedidos/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Pedido pedidoAtualizado)
        {
            // Impede a atualização de um pedido com título vazio.
            if (string.IsNullOrWhiteSpace(pedidoAtualizado.Titulo))
            {
                return BadRequest(new { mensagem = "O título é obrigatório." });
            }

            if (string.IsNullOrWhiteSpace(pedidoAtualizado.Descricao))
            {
                return BadRequest(new { mensagem = "A descrição é obrigatória." });
            }

            if (string.IsNullOrWhiteSpace(pedidoAtualizado.NomeContato))
            {
                return BadRequest(new { mensagem = "O nome do contacto é obrigatório." });
            }

            if (string.IsNullOrWhiteSpace(pedidoAtualizado.EmailContato))
            {
                return BadRequest(new { mensagem = "O email do contacto é obrigatório." });
            }

            if (string.IsNullOrWhiteSpace(pedidoAtualizado.Categoria))
            {
                return BadRequest(new { mensagem = "A categoria é obrigatória." });
            }

            // Respeita os limites definidos na tabela.
            if (pedidoAtualizado.Titulo.Length > 255 ||
                pedidoAtualizado.NomeContato.Length > 255 ||
                pedidoAtualizado.EmailContato.Length > 255 ||
                pedidoAtualizado.Categoria.Length > 50)
            {
                return BadRequest(new { mensagem = "O título, nome e email podem ter até 255 caracteres; a categoria, até 50."});
            }

            if (pedidoAtualizado.Prioridade != "Baixa" &&
                pedidoAtualizado.Prioridade != "Normal" &&
                pedidoAtualizado.Prioridade != "Alta")
            {
                return BadRequest(new { mensagem = "A prioridade deve ser Baixa, Normal ou Alta."});
            }

            if (pedidoAtualizado.Estado != "Aberto" &&
                pedidoAtualizado.Estado != "Em curso" &&
                pedidoAtualizado.Estado != "Resolvido")
            {
                return BadRequest(new { mensagem = "O estado deve ser Aberto, Em curso ou Resolvido."} );
            }

            // A API define a data da atualização.
            pedidoAtualizado.AtualizadoEm = DateTime.UtcNow;

            if (pedidoAtualizado.Estado == "Resolvido")
            {
                pedidoAtualizado.ResolvidoEm = pedidoAtualizado.AtualizadoEm;
            }
            else
            {
                pedidoAtualizado.ResolvidoEm = null;
            }

            using var conexao = new MySqlConnection(_connectionString);
            await conexao.OpenAsync();

            string query = @"UPDATE pedidos
                 SET titulo = @titulo,
                     descricao = @descricao,
                     nome_contato = @nome_contato,
                     email_contato = @email_contato,
                     categoria = @categoria,
                     prioridade = @prioridade,
                     estado = @estado,
                     atualizado_em = @atualizado_em,
                     resolvido_em = @resolvido_em
                 WHERE id = @id";

            using var comando = new MySqlCommand(query, conexao);

            comando.Parameters.AddWithValue("@id", id);
            comando.Parameters.AddWithValue("@titulo", pedidoAtualizado.Titulo);
            comando.Parameters.AddWithValue("@descricao", pedidoAtualizado.Descricao);
            comando.Parameters.AddWithValue("@nome_contato", pedidoAtualizado.NomeContato);
            comando.Parameters.AddWithValue("@email_contato", pedidoAtualizado.EmailContato);
            comando.Parameters.AddWithValue("@categoria", pedidoAtualizado.Categoria);
            comando.Parameters.AddWithValue("@prioridade", pedidoAtualizado.Prioridade);
            comando.Parameters.AddWithValue("@estado", pedidoAtualizado.Estado);
            comando.Parameters.AddWithValue("@atualizado_em", pedidoAtualizado.AtualizadoEm);

            if (pedidoAtualizado.ResolvidoEm.HasValue)
            {
                comando.Parameters.AddWithValue(
                    "@resolvido_em", pedidoAtualizado.ResolvidoEm.Value);
            }
            else
            {
                comando.Parameters.AddWithValue("@resolvido_em", DBNull.Value);
            }

            int linhasAfetadas = await comando.ExecuteNonQueryAsync();

            if (linhasAfetadas == 0)
            {
                return NotFound(new
                {
                    mensagem = "Pedido não encontrado para atualizar."
                });
            }

            return NoContent();
        }
    }
}
