namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class updateNullables : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUsers", "ProfilePicture_IsSilhouette", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "ProfilePicture_IsSilhouette", c => c.Boolean(nullable: false));
        }
    }
}
