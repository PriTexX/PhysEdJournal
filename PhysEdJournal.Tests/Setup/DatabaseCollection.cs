namespace PhysEdJournal.Tests.Setup;

[CollectionDefinition("Db collection")]
public class DatabaseCollection : ICollectionFixture<PostgresContainerFixture> { }
