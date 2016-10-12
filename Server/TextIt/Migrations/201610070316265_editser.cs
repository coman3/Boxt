namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editser : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CoverPictures",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        OffsetX = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AspNetUsers", "AgeRange_Min", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "AgeRange_Max", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "Gender", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "Verified", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "CoverPicture_Id", c => c.Long());
            CreateIndex("dbo.AspNetUsers", "CoverPicture_Id");
            AddForeignKey("dbo.AspNetUsers", "CoverPicture_Id", "dbo.CoverPictures", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "CoverPicture_Id", "dbo.CoverPictures");
            DropIndex("dbo.AspNetUsers", new[] { "CoverPicture_Id" });
            DropColumn("dbo.AspNetUsers", "CoverPicture_Id");
            DropColumn("dbo.AspNetUsers", "Verified");
            DropColumn("dbo.AspNetUsers", "Gender");
            DropColumn("dbo.AspNetUsers", "AgeRange_Max");
            DropColumn("dbo.AspNetUsers", "AgeRange_Min");
            DropTable("dbo.CoverPictures");
        }
    }
}
