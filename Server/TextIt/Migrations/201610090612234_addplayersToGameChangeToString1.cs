namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addplayersToGameChangeToString1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Game_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.AspNetUsers", "Game_Id");
            AddForeignKey("dbo.AspNetUsers", "Game_Id", "dbo.Games", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "Game_Id", "dbo.Games");
            DropIndex("dbo.AspNetUsers", new[] { "Game_Id" });
            DropColumn("dbo.AspNetUsers", "Game_Id");
        }
    }
}
