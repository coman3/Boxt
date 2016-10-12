namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addprofilepictureinfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ProfilePicture_IsSilhouette", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "ProfilePicture_Source", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "ProfilePicture_Source");
            DropColumn("dbo.AspNetUsers", "ProfilePicture_IsSilhouette");
        }
    }
}
