using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemplateBackend.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoFactorAuthenticationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GoogleAuthenticatorSecret",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GoogleAuthenticatorEnabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationOTP",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationOTPExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GoogleAuthenticatorSecret",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GoogleAuthenticatorEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerificationOTP",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerificationOTPExpiry",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                table: "AspNetUsers");
        }
    }
} 