namespace TextIt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class AddGameState : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Games", "GameState", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Games", "GameState");
        }
    }
}
