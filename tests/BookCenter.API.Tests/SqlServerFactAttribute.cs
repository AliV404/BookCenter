namespace BookCenter.API.Tests
{
    /// <summary>
    /// A test that relies on real SQL Server behavior (rowversion, constraints, indexes).
    /// Skipped unless BOOKCENTER_TEST_SQLSERVER is set — see <see cref="ApiFactory"/>.
    /// </summary>
    public sealed class SqlServerFactAttribute : FactAttribute
    {
        public SqlServerFactAttribute()
        {
            if (!ApiFactory.UsesSqlServer)
                Skip = "Needs SQL Server: set BOOKCENTER_TEST_SQLSERVER to run it.";
        }
    }
}
