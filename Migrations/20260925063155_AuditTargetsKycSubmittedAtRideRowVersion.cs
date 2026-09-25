using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ride_Hailing_API.Migrations
{
    /// <inheritdoc />
    public partial class AuditTargetsKycSubmittedAtRideRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Rides",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Kycs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "AuditLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetEntity",
                table: "AuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetId",
                table: "AuditLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TargetEntity_TargetId",
                table: "AuditLogs",
                columns: new[] { "TargetEntity", "TargetId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TargetEntity_TargetId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Kycs");

            migrationBuilder.DropColumn(
                name: "Details",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "TargetEntity",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "AuditLogs");
        }
    }
}
