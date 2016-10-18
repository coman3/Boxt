namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGameAppOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GameApplications", "Order", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GameApplications", "Order");
        }
    }
}
