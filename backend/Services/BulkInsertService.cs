using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Backend.Interfaces;
using Microsoft.Extensions.Configuration;
using Backend.Models.Entities;

using Backend.Interfaces;
using Backend.Context;
using Backend.Models.Entities;

namespace Backend.Services{
     public class BulkInsertService : IBulkInsertService
    {
        private readonly IConfiguration _configuration;

        public BulkInsertService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task BulkInsertSSHCommandsAsync(IEnumerable<SSHCommand> commands, CancellationToken cancellationToken)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var writer = connection.BeginBinaryImport("COPY \"SSHCommands\" (\"Id\", \"Command\", \"Output\", \"ExecutedAt\", \"LinkedSSHSessionId\") FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var command in commands)
                    {
                        writer.StartRow();
                        writer.Write(command.Id, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(command.CommandText, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(command.Output, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(command.ExecutedAt, NpgsqlTypes.NpgsqlDbType.Timestamp);
                        writer.Write(command.LinkedSSHSessionId, NpgsqlTypes.NpgsqlDbType.Varchar);
                    }

                    await writer.CompleteAsync(cancellationToken);
                }
            }
        }

        public async Task BulkInsertAIConversationsAsync(IEnumerable<AIConversation> conversations, CancellationToken cancellationToken)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var writer = connection.BeginBinaryImport("COPY \"AIConversations\" (\"Id\", \"Topic\", \"LinkedSSHSessionId\") FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var conversation in conversations)
                    {
                        writer.StartRow();
                        writer.Write(conversation.Id, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(conversation.Topic, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(conversation.LinkedSSHSessionId, NpgsqlTypes.NpgsqlDbType.Varchar);
                    }

                    await writer.CompleteAsync(cancellationToken);
                }
            }
        }

        public async Task BulkInsertAIMessagesAsync(IEnumerable<AIMessage> messages, CancellationToken cancellationToken)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var writer = connection.BeginBinaryImport("COPY \"AIMessages\" (\"Id\", \"Message\", \"SentAt\", \"AIConversationId\") FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var message in messages)
                    {
                        writer.StartRow();
                        writer.Write(message.Id, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(message.Message, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(message.SentAt, NpgsqlTypes.NpgsqlDbType.Timestamp);
                        writer.Write(message.AIConversationId, NpgsqlTypes.NpgsqlDbType.Varchar);
                    }

                    await writer.CompleteAsync(cancellationToken);
                }
            }
        }
    }
}
