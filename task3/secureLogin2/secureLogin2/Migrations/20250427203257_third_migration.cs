using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace secureLogin2.Migrations
{
    /// <inheritdoc />
    public partial class third_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "VARCHAR(90)",
                maxLength: 90,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(90)",
                oldMaxLength: 90);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "VARCHAR(90)",
                maxLength: 90,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "VARCHAR(90)",
                oldMaxLength: 90,
                oldNullable: true);
        }
    }
}
