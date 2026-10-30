namespace ServiceTrack.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Regista os controllers da API.
            builder.Services.AddControllers();

            // Regista os serviços necessários para documentar os endpoints.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Disponibiliza a documentação e a interface Swagger
            // apenas no ambiente de desenvolvimento.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Redireciona os pedidos HTTP para HTTPS.
            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Disponibiliza as rotas definidas nos controllers.
            app.MapControllers();

            app.Run();
        }
    }
}
