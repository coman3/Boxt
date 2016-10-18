namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class addplayersToGame : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Games", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.AspNetUsers", "Game_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.AspNetUsers", "Game_Id");
            AddForeignKey("dbo.AspNetUsers", "Game_Id", "dbo.Games", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "Game_Id", "dbo.Games");
            DropIndex("dbo.AspNetUsers", new[] { "Game_Id" });
            DropColumn("dbo.AspNetUsers", "Game_Id");
            DropColumn("dbo.Games", "CreatedDate");
        }
    }
}
