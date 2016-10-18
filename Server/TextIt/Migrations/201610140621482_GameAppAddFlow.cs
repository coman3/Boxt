#pragma warning disable 1591
namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GameAppAddFlow : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GameApplicationFlows",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        RowSpan = c.Int(nullable: false),
                        ColSpan = c.Int(nullable: false),
                        StyleString = c.String(),
                        IconPath = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.GameApplications", "Flow_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.GameApplications", "Flow_Id");
            AddForeignKey("dbo.GameApplications", "Flow_Id", "dbo.GameApplicationFlows", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GameApplications", "Flow_Id", "dbo.GameApplicationFlows");
            DropIndex("dbo.GameApplications", new[] { "Flow_Id" });
            DropColumn("dbo.GameApplications", "Flow_Id");
            DropTable("dbo.GameApplicationFlows");
        }
    }
}
