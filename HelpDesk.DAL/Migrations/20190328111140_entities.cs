using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HelpDesk.DAL.Migrations
{
    public partial class entities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Surveys",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    Satisfaction = table.Column<double>(nullable: false),
                    TechPoint = table.Column<double>(nullable: false),
                    Speed = table.Column<double>(nullable: false),
                    Pricing = table.Column<double>(nullable: false),
                    Solving = table.Column<double>(nullable: false),
                    Suggestions = table.Column<string>(maxLength: 200, nullable: true),
                    IsDone = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Surveys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    CustomerId = table.Column<string>(nullable: false),
                    OperatorId = table.Column<string>(nullable: true),
                    TechnicianId = table.Column<string>(nullable: true),
                    SurveyId = table.Column<string>(nullable: true),
                    Description = table.Column<string>(maxLength: 250, nullable: true),
                    ProductType = table.Column<int>(nullable: false),
                    IssueState = table.Column<int>(nullable: false),
                    PurchasedDate = table.Column<DateTime>(nullable: false),
                    WarrantyState = table.Column<bool>(nullable: false),
                    ServiceCharge = table.Column<decimal>(nullable: false),
                    OptReport = table.Column<string>(maxLength: 250, nullable: true),
                    TechReport = table.Column<string>(maxLength: 250, nullable: true),
                    ClosedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Issues_AspNetUsers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Issues_AspNetUsers_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_Surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_AspNetUsers_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IssueLogs",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    IssueId = table.Column<string>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    FromWhom = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueLogs_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Photographs",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    Path = table.Column<string>(nullable: false),
                    IssueId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photographs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photographs_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueLogs_IssueId",
                table: "IssueLogs",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_CustomerId",
                table: "Issues",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_OperatorId",
                table: "Issues",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_SurveyId",
                table: "Issues",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_TechnicianId",
                table: "Issues",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Photographs_IssueId",
                table: "Photographs",
                column: "IssueId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueLogs");

            migrationBuilder.DropTable(
                name: "Photographs");

            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropTable(
                name: "Surveys");
        }
    }
}
