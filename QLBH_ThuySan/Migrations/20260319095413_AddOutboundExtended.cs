using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLBH_ThuySan.Migrations
{
    public partial class AddOutboundExtended : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "idNhaCungCap",
                table: "PhieuXuat",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "loaiXuat",
                table: "PhieuXuat",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValueSql: "('SALES')");

            migrationBuilder.AddColumn<string>(
                name: "lyDo",
                table: "PhieuXuat",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "maKhoNhan",
                table: "PhieuXuat",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuat_idNhaCungCap",
                table: "PhieuXuat",
                column: "idNhaCungCap");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuat_maKhoNhan",
                table: "PhieuXuat",
                column: "maKhoNhan");

            migrationBuilder.AddForeignKey(
                name: "FK_PhieuXuat_KhoNhan",
                table: "PhieuXuat",
                column: "maKhoNhan",
                principalTable: "Kho",
                principalColumn: "maKho");

            migrationBuilder.AddForeignKey(
                name: "FK_PhieuXuat_NhaCungCap",
                table: "PhieuXuat",
                column: "idNhaCungCap",
                principalTable: "NhaCungCap",
                principalColumn: "maDoiTuong");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhieuXuat_KhoNhan",
                table: "PhieuXuat");

            migrationBuilder.DropForeignKey(
                name: "FK_PhieuXuat_NhaCungCap",
                table: "PhieuXuat");

            migrationBuilder.DropIndex(
                name: "IX_PhieuXuat_idNhaCungCap",
                table: "PhieuXuat");

            migrationBuilder.DropIndex(
                name: "IX_PhieuXuat_maKhoNhan",
                table: "PhieuXuat");

            migrationBuilder.DropColumn(
                name: "idNhaCungCap",
                table: "PhieuXuat");

            migrationBuilder.DropColumn(
                name: "loaiXuat",
                table: "PhieuXuat");

            migrationBuilder.DropColumn(
                name: "lyDo",
                table: "PhieuXuat");

            migrationBuilder.DropColumn(
                name: "maKhoNhan",
                table: "PhieuXuat");
        }
    }
}
