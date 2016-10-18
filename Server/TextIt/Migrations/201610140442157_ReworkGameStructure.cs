namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class ReworkGameStructure : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GameApplications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        MinPlayers = c.Int(nullable: false),
                        MaxPlayers = c.Int(nullable: false),
                        GameStateType = c.String(),
                        CategoriesList = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Games", "GameApplication_Id", c => c.Int());
            CreateIndex("dbo.Games", "GameApplication_Id");
            AddForeignKey("dbo.Games", "GameApplication_Id", "dbo.GameApplications", "Id");
            DropColumn("dbo.Games", "GameType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Games", "GameType", c => c.Int(nullable: false));
            DropForeignKey("dbo.Games", "GameApplication_Id", "dbo.GameApplications");
            DropIndex("dbo.Games", new[] { "GameApplication_Id" });
            DropColumn("dbo.Games", "GameApplication_Id");
            DropTable("dbo.GameApplications");
        }
    }
}
