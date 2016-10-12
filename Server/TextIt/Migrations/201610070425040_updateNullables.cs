namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
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
