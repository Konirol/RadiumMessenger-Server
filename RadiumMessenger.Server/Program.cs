
using RadiumMessenger.Server.Data;
using RadiumMessenger.Server.Services;
using RadiumMessenger.Server.Hubs;
using Resend;
using Microsoft.EntityFrameworkCore;

namespace RadiumMessenger.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<AuthService>();

            builder.Services.AddScoped<ChatService>();

            builder.Services.AddScoped<MessageService>();

            builder.Services.AddResend(options =>
            {
                options.ApiToken = builder.Configuration["Resend:ApiKey"]!;
            });

            builder.Services.AddScoped<EmailService>();

            builder.Services.AddScoped<VerificationService>();

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddSignalR();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>();

            var app = builder.Build();

            app.Urls.Add("http://127.0.0.1:7199");

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
                var connection = db.Database.GetDbConnection();

                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "PRAGMA table_info('Users');";

                    using var reader = await command.ExecuteReaderAsync();

                    bool columnExists = false;

                    while (await reader.ReadAsync())
                    {
                        if (reader.GetString(1) == "IsEmailVerified")
                        {
                            columnExists = true;
                            break;
                        }
                    }

                    reader.Close();

                    if (!columnExists)
                    {
                        await db.Database.ExecuteSqlRawAsync(
                            "ALTER TABLE Users ADD COLUMN IsEmailVerified INTEGER NOT NULL DEFAULT 0;");
                    }
                }


                using (var messageCommand = connection.CreateCommand())
                {
                    messageCommand.CommandText = "PRAGMA table_info('Messages')";

                    using var messageReader = await
                        messageCommand.ExecuteReaderAsync();

                    bool isDeliveredExists = false;
                    bool isReadExists = false;

                    while (await messageReader.ReadAsync())
                    {
                        string columnName = messageReader.GetString(1);

                        if (columnName == "IsDelivered")
                            isDeliveredExists = true;

                        if (columnName == "IsRead")
                            isReadExists = true;
                    }

                    await messageReader.CloseAsync();

                    if (!isDeliveredExists)
                    {
                        await db.Database.ExecuteSqlRawAsync(
                            "ALTER TABLE Messages ADD COLUMN IsDelivered INTEGER NOT NULL DEFAULT 0");
                    }

                    if (!isReadExists)
                    {
                        await db.Database.ExecuteSqlRawAsync(
                            "ALTER TABLE Messages ADD COLUMN IsRead INTEGER NOT NULL DEFAULT 0");
                    }
                }

                using (var chatCommand = connection.CreateCommand())
                {
                    chatCommand.CommandText = "PRAGMA table_info('Chats')";

                    using var chatReader = await chatCommand.ExecuteReaderAsync();

                    bool isArchivedForFirstUserExists = false;
                    bool isArchivedForSecondUserExists = false;

                    while (await chatReader.ReadAsync())
                    {
                        string columnName = chatReader.GetString(1);

                        if (columnName == "IsArchivedForFirstUser")
                            isArchivedForFirstUserExists = true;

                        if (columnName == "IsArchivedForSecondUser")
                            isArchivedForSecondUserExists = true;
                    }

                    await chatReader.CloseAsync();

                    if (!isArchivedForFirstUserExists)
                    {
                        await db.Database.ExecuteSqlRawAsync(
                            "ALTER TABLE Chats ADD COLUMN IsArchivedForFirstUser INTEGER NOT NULL DEFAULT 0");
                    }

                    if (!isArchivedForSecondUserExists)
                    {
                        await db.Database.ExecuteSqlRawAsync(
                            "ALTER TABLE Chats ADD COLUMN IsArchivedForSecondUser INTEGER NOT NULL DEFAULT 0");
                    }
                }

                using (var deleteCommand = connection.CreateCommand())
                {
                    deleteCommand.CommandText = "PRAGMA table_info('Messages');";

                    using var deleteReader = await deleteCommand.ExecuteReaderAsync();

                    bool firstDeletedExists = false;
                    bool secondDeletedExists = false;

                    while (await deleteReader.ReadAsync())
                    {
                        string columnName = deleteReader.GetString(1);

                        if (columnName == "IsDeletedForFirstUser")
                            firstDeletedExists = true;

                        if (columnName == "IsDeletedForSecondUser")
                            secondDeletedExists = true;
                    }

                    await deleteReader.CloseAsync();

                    if (!firstDeletedExists)
                    {
                        await db.Database.ExecuteSqlRawAsync(
                            "ALTER TABLE Messages ADD COLUMN IsDeletedForFirstUser INTEGER NOT NULL DEFAULT 0");
                    }

                    if (!secondDeletedExists)
                    {
                        await db.Database.ExecuteSqlRawAsync(
                            "ALTER TABLE Messages ADD COLUMN IsDeletedForSecondUser INTEGER NOT NULL DEFAULT 0");
                    }
                }

                await connection.CloseAsync();

                db.Database.ExecuteSqlRaw(@"
                  CREATE TABLE IF NOT EXISTS EmailVerifications (
                      Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                      Email TEXT NOT NULL,
                      CodeHash TEXT NOT NULL,
                      ExpiresAt TEXT NOT NULL,
                      IsUsed INTEGER NOT NULL
                   );
                ");
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.MapHub<MessageHub>("/messageHub");

            app.Run();
        }
    }
}
