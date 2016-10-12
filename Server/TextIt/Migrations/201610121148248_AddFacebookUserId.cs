namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFacebookUserId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "FacebookId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "FacebookId");
        }
    }
}
