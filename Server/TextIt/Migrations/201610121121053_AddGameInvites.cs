namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class AddGameInvites : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GameInvites",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CreateDate = c.DateTime(nullable: false),
                        Expiry = c.DateTime(nullable: false),
                        Game_Id = c.String(maxLength: 128),
                        Invitee_Id = c.String(maxLength: 128),
                        Inviter_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Games", t => t.Game_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Invitee_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Inviter_Id)
                .Index(t => t.Game_Id)
                .Index(t => t.Invitee_Id)
                .Index(t => t.Inviter_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GameInvites", "Inviter_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.GameInvites", "Invitee_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.GameInvites", "Game_Id", "dbo.Games");
            DropIndex("dbo.GameInvites", new[] { "Inviter_Id" });
            DropIndex("dbo.GameInvites", new[] { "Invitee_Id" });
            DropIndex("dbo.GameInvites", new[] { "Game_Id" });
            DropTable("dbo.GameInvites");
        }
    }
}
