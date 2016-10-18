namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class removeKeyFromCoverPicture : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUsers", "CoverPicture_Id", "dbo.CoverPictures");
            DropIndex("dbo.AspNetUsers", new[] { "CoverPicture_Id" });
            AddColumn("dbo.AspNetUsers", "Name", c => c.String());
            AddColumn("dbo.AspNetUsers", "CoverPicture_OffsetX", c => c.Int());
            AddColumn("dbo.AspNetUsers", "CoverPicture_OffsetY", c => c.Int());
            AddColumn("dbo.AspNetUsers", "CoverPicture_Source", c => c.String());
            DropColumn("dbo.AspNetUsers", "CoverPicture_Id");
            DropTable("dbo.CoverPictures");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.CoverPictures",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        OffsetX = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AspNetUsers", "CoverPicture_Id", c => c.Long());
            DropColumn("dbo.AspNetUsers", "CoverPicture_Source");
            DropColumn("dbo.AspNetUsers", "CoverPicture_OffsetY");
            DropColumn("dbo.AspNetUsers", "CoverPicture_OffsetX");
            DropColumn("dbo.AspNetUsers", "Name");
            CreateIndex("dbo.AspNetUsers", "CoverPicture_Id");
            AddForeignKey("dbo.AspNetUsers", "CoverPicture_Id", "dbo.CoverPictures", "Id");
        }
    }
}
