using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLBH_ThuySan.Migrations
{
    /// <inheritdoc />
    public partial class AddPartialPaymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "tongTien",
                table: "PhieuXuat",
                newName: "soPhaiThanhToan");

            migrationBuilder.RenameColumn(
                name: "tongTien",
                table: "PhieuTinhChietKhau",
                newName: "soPhaiThanhToan");

            migrationBuilder.RenameColumn(
                name: "tongTien",
                table: "PhieuNhap",
                newName: "soPhaiThanhToan");



            migrationBuilder.AddColumn<decimal>(
                name: "soChuaThanhToan",
                table: "PhieuXuat",
                type: "decimal(18,2)",
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "soDaThanhToan",
                table: "PhieuXuat",
                type: "decimal(18,2)",
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "soChuaThanhToan",
                table: "PhieuTinhChietKhau",
                type: "decimal(18,2)",
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "soDaThanhToan",
                table: "PhieuTinhChietKhau",
                type: "decimal(18,2)",
                nullable: true,
                defaultValue: 0m);



            migrationBuilder.AddColumn<decimal>(
                name: "soChuaThanhToan",
                table: "PhieuNhap",
                type: "decimal(18,2)",
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "soDaThanhToan",
                table: "PhieuNhap",
                type: "decimal(18,2)",
                nullable: true,
                defaultValue: 0m);




        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhieuXuat_KhoXuat",
                table: "PhieuXuat");

            migrationBuilder.DropIndex(
                name: "IX_PhieuXuat_maKhoXuat",
                table: "PhieuXuat");

            migrationBuilder.DropColumn(
                name: "maKhoXuat",
                table: "PhieuXuat");

            migrationBuilder.DropColumn(
                name: "soChuaThanhToan",
                table: "PhieuXuat");

            migrationBuilder.DropColumn(
                name: "soDaThanhToan",
                table: "PhieuXuat");

            migrationBuilder.DropColumn(
                name: "soChuaThanhToan",
                table: "PhieuTinhChietKhau");

            migrationBuilder.DropColumn(
                name: "soDaThanhToan",
                table: "PhieuTinhChietKhau");

            migrationBuilder.DropColumn(
                name: "isDisabled",
                table: "PhieuNhap");

            migrationBuilder.DropColumn(
                name: "soChuaThanhToan",
                table: "PhieuNhap");

            migrationBuilder.DropColumn(
                name: "soDaThanhToan",
                table: "PhieuNhap");

            migrationBuilder.DropColumn(
                name: "isDisabled",
                table: "NhaCungCap");

            migrationBuilder.DropColumn(
                name: "isDisabled",
                table: "Kho");

            migrationBuilder.DropColumn(
                name: "isDisabled",
                table: "KhachHang");

            migrationBuilder.DropColumn(
                name: "isDisabled",
                table: "HangHoa");

            migrationBuilder.DropColumn(
                name: "isDisabled",
                table: "DoiTuongChiPhi");

            migrationBuilder.DropColumn(
                name: "isDisabled",
                table: "DaiLy");

            migrationBuilder.RenameColumn(
                name: "soPhaiThanhToan",
                table: "PhieuXuat",
                newName: "tongTien");

            migrationBuilder.RenameColumn(
                name: "soPhaiThanhToan",
                table: "PhieuTinhChietKhau",
                newName: "tongTien");

            migrationBuilder.RenameColumn(
                name: "soPhaiThanhToan",
                table: "PhieuNhap",
                newName: "tongTien");
        }
    }
}
