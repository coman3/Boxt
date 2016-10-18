namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class addplayerstogames : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserGame",
                c => new
                    {
                        GameRefId = c.String(nullable: false, maxLength: 128),
                        UserRefId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GameRefId, t.UserRefId })
                .ForeignKey("dbo.Games", t => t.GameRefId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserRefId, cascadeDelete: true)
                .Index(t => t.GameRefId)
                .Index(t => t.UserRefId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserGame", "UserRefId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserGame", "GameRefId", "dbo.Games");
            DropIndex("dbo.UserGame", new[] { "UserRefId" });
            DropIndex("dbo.UserGame", new[] { "GameRefId" });
            DropTable("dbo.UserGame");
        }
    }
}
