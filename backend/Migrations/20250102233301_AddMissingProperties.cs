using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "AIMessages",
                newName: "SentAt");

            migrationBuilder.RenameColumn(
                name: "MessageText",
                table: "AIMessages",
                newName: "Message");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "UserPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreferenceToken",
                table: "UserPreferences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SSHDefaultConfig",
                table: "SSHHostConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "AIConversations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "PreferenceToken",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "SSHDefaultConfig",
                table: "SSHHostConfigs");

            migrationBuilder.DropColumn(
                name: "Topic",
                table: "AIConversations");

            migrationBuilder.RenameColumn(
                name: "SentAt",
                table: "AIMessages",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "AIMessages",
                newName: "MessageText");
        }
    }
}
