#pragma warning disable 1591
namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class DatabaseCreateId : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserGame", "GameRefId", "dbo.Games");
            DropForeignKey("dbo.GameInvites", "Game_Id", "dbo.Games");
            DropPrimaryKey("dbo.GameInvites");
            DropPrimaryKey("dbo.Games");
            AlterColumn("dbo.GameInvites", "Id", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Games", "Id", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.GameInvites", "Id");
            AddPrimaryKey("dbo.Games", "Id");
            AddForeignKey("dbo.UserGame", "GameRefId", "dbo.Games", "Id", cascadeDelete: true);
            AddForeignKey("dbo.GameInvites", "Game_Id", "dbo.Games", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GameInvites", "Game_Id", "dbo.Games");
            DropForeignKey("dbo.UserGame", "GameRefId", "dbo.Games");
            DropPrimaryKey("dbo.Games");
            DropPrimaryKey("dbo.GameInvites");
            AlterColumn("dbo.Games", "Id", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.GameInvites", "Id", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.Games", "Id");
            AddPrimaryKey("dbo.GameInvites", "Id");
            AddForeignKey("dbo.GameInvites", "Game_Id", "dbo.Games", "Id");
            AddForeignKey("dbo.UserGame", "GameRefId", "dbo.Games", "Id", cascadeDelete: true);
        }
    }
}
