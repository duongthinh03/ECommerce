using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCreatedAtStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt_Status",
                table: "Orders",
                columns: new[] { "CreatedAt", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedAt_Status",
                table: "Orders");
        }
    }
}
