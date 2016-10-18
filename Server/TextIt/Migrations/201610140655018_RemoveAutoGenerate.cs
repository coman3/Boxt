namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveAutoGenerate : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GameApplications", "Flow_Id", "dbo.GameApplicationFlows");
            DropPrimaryKey("dbo.GameApplicationFlows");
            AlterColumn("dbo.GameApplicationFlows", "Id", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.GameApplicationFlows", "Id");
            AddForeignKey("dbo.GameApplications", "Flow_Id", "dbo.GameApplicationFlows", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GameApplications", "Flow_Id", "dbo.GameApplicationFlows");
            DropPrimaryKey("dbo.GameApplicationFlows");
            AlterColumn("dbo.GameApplicationFlows", "Id", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.GameApplicationFlows", "Id");
            AddForeignKey("dbo.GameApplications", "Flow_Id", "dbo.GameApplicationFlows", "Id");
        }
    }
}
