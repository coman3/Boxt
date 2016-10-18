namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class addplayersToGameChangeToString : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUsers", "Game_Id", "dbo.Games");
            DropIndex("dbo.AspNetUsers", new[] { "Game_Id" });
            DropColumn("dbo.AspNetUsers", "Game_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Game_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.AspNetUsers", "Game_Id");
            AddForeignKey("dbo.AspNetUsers", "Game_Id", "dbo.Games", "Id");
        }
    }
}
