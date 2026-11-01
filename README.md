# ServiceTrack.API

REST API for managing service requests, developed as part of UC00614 — Integrar Sistemas de Informação.

This version uses a single MySQL table and follows a controller-based structure, with direct database access through parameterized SQL.

## Technologies

- C# and ASP.NET Core Web API — .NET 10
- MySQL
- MySqlConnector
- Swagger UI — Swashbuckle.AspNetCore

## Features

- List all service requests.
- Retrieve a request by ID.
- Create, update and delete requests.
- Validate required fields, field lengths, priorities and statuses.
- Track creation, update and resolution dates.

Allowed priorities: `Baixa`, `Normal`, `Alta`.

Allowed statuses: `Aberto`, `Em curso`, `Resolvido`.

## Project structure

- `Controllers/PedidosController.cs` — CRUD endpoints and database access.
- `Models/Pedido.cs` — service request model.
- `database/servicetrack.sql` — database and table creation script.
- `Program.cs` — application and Swagger configuration.
- `ServiceTrack.API.http` — sample HTTP requests.

## Requirements

- .NET 10 SDK
- Visual Studio with .NET 10 support and the ASP.NET workload
- MySQL Server
- A MySQL administration tool, such as phpMyAdmin

## Database setup

1. Start MySQL.
2. Execute `database/servicetrack.sql`.
3. The script creates the `servicetrack` database and its single table, `pedidos`.

The script creates the structure without inserting sample data.

## Connection configuration

Configure `ConnectionStrings:DefaultConnection` in `appsettings.json` for your local environment:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=servicetrack;User ID=root;"
}
```

This example assumes a local MySQL account without a password.

If your account requires a password, store the complete connection string using Visual Studio's **Manage User Secrets** option. Do not commit passwords to the repository.

## Running the API

1. Open `ServiceTrack.API.slnx` in Visual Studio.
2. Restore the NuGet packages and build the solution.
3. Ensure MySQL is running.
4. Run the API using the HTTPS launch profile.
5. Open `/swagger` at the application's HTTPS address.

The current HTTPS address is `https://localhost:7242`. Confirm the port in `Properties/launchSettings.json` if your configuration differs.

Swagger is enabled in the Development environment.

## Endpoints

| Method | Route | Successful response |
|---|---|---|
| GET | `/api/Pedidos` | 200 — list of requests |
| GET | `/api/Pedidos/{id}` | 200 — requested record |
| POST | `/api/Pedidos` | 201 — created record |
| PUT | `/api/Pedidos/{id}` | 204 — updated |
| DELETE | `/api/Pedidos/{id}` | 204 — deleted |

Invalid POST or PUT data returns HTTP 400.

Retrieving, updating or deleting a missing record returns HTTP 404.

## Example POST body

```json
{
  "titulo": "Falha na ligação à base de dados",
  "descricao": "A aplicação não consegue estabelecer ligação ao MySQL.",
  "nomeContato": "Cliente de teste",
  "emailContato": "cliente@example.com",
  "categoria": "Software",
  "prioridade": "Alta",
  "estado": "Aberto"
}
```

MySQL generates the ID. The API assigns creation and update dates using UTC.

For PUT requests, send all editable fields, as in the example above. The record is selected using the ID in the URL, and its creation date is preserved.

When the status is `Resolvido`, the API assigns a resolution date. Updating a resolved request assigns a new resolution date; changing to another status clears it.

## Manual verification

The following scenarios were tested using Swagger:

- Listing requests.
- Retrieving existing and missing requests.
- Creating a valid request and retrieving the saved record.
- Rejecting an invalid priority.
- Updating a request and checking its saved values.
- Updating a request to the resolved status.
- Updating a missing request.
- Deleting a request and confirming it no longer exists.
- Repeating deletion for a missing request.

## Scope

This is an educational portfolio version with one database table.

Email validation currently checks presence and length, not address format. Authentication and authorization rules are not implemented.

The Windows Forms client, `ServiceTrack.Client`, is developed in a separate solution and repository.
