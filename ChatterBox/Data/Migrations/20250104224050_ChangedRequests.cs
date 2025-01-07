using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatterBox.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangedRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Requests",
                newName: "RequestType");

            migrationBuilder.RenameColumn(
                name: "Typoe",
                table: "BindRequestChannelUserEntries",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestType",
                table: "Requests",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "BindRequestChannelUserEntries",
                newName: "Typoe");
        }
    }
}
