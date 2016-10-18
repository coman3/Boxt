namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class changeCoverAgeRequired : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "CoverPicture", c => c.String());
            AddColumn("dbo.AspNetUsers", "ProfilePicture", c => c.String());
            DropColumn("dbo.AspNetUsers", "AgeRange_Min");
            DropColumn("dbo.AspNetUsers", "AgeRange_Max");
            DropColumn("dbo.AspNetUsers", "CoverPicture_OffsetX");
            DropColumn("dbo.AspNetUsers", "CoverPicture_OffsetY");
            DropColumn("dbo.AspNetUsers", "CoverPicture_Source");
            DropColumn("dbo.AspNetUsers", "ProfilePicture_IsSilhouette");
            DropColumn("dbo.AspNetUsers", "ProfilePicture_Source");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "ProfilePicture_Source", c => c.String());
            AddColumn("dbo.AspNetUsers", "ProfilePicture_IsSilhouette", c => c.Boolean());
            AddColumn("dbo.AspNetUsers", "CoverPicture_Source", c => c.String());
            AddColumn("dbo.AspNetUsers", "CoverPicture_OffsetY", c => c.Int());
            AddColumn("dbo.AspNetUsers", "CoverPicture_OffsetX", c => c.Int());
            AddColumn("dbo.AspNetUsers", "AgeRange_Max", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "AgeRange_Min", c => c.Int(nullable: false));
            DropColumn("dbo.AspNetUsers", "ProfilePicture");
            DropColumn("dbo.AspNetUsers", "CoverPicture");
        }
    }
}
