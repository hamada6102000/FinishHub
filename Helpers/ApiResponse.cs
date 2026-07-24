namespace test.Helpers;

public static class ApiResponse
{
    public static object Success(object? data = null, string message = "Success")
        => new { success = true, message, data };

    public static object Fail(string message)
        => new { success = false, message, data = (object?)null };
}
