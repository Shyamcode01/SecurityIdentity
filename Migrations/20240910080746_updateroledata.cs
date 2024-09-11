using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApiIdentity_security.Migrations
{
    /// <inheritdoc />
    public partial class updateroledata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "bcb76309-7300-4908-b4fe-152b7b802e97", "2", "User", "User" },
                    { "cae47f64-69bf-4927-869a-4fdcb23a1400", "3", "HR", "HR" },
                    { "cd3ed16b-c844-4049-99f9-a4d55eaf513f", "1", "Admin", "Admin" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bcb76309-7300-4908-b4fe-152b7b802e97");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cae47f64-69bf-4927-869a-4fdcb23a1400");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cd3ed16b-c844-4049-99f9-a4d55eaf513f");
        }
    }
}
