using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GudSafe.Data.Migrations
{
    /// <inheritdoc />
    public partial class ShortURLs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortUrl",
                table: "Files",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortUrl",
                table: "Files");
        }
    }
}
