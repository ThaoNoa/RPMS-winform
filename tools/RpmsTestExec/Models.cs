namespace RPMS.TestExec;

public sealed class TestCaseRow
{
    public string Id { get; set; } = "";
    public string Module { get; set; } = "";
    public string Feature { get; set; } = "";
    public string Requirement { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Precondition { get; set; } = "";
    public string TestData { get; set; } = "";
    public string Steps { get; set; } = "";
    public string Expected { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Type { get; set; } = "";
    public string Role { get; set; } = "";
}

public sealed class ExecResult
{
    public string TestCaseId { get; set; } = "";
    public string Expected { get; set; } = "";
    public string Actual { get; set; } = "";
    public string Status { get; set; } = "BLOCKED"; // PASS / FAIL / BLOCKED
    public double ExecutionMs { get; set; }
    public string Tester { get; set; } = "AutoQA-RpmsTestExec";
    public string Environment { get; set; } = @".\SQLEXPRESS / RPMS / WinForms net8";
    public string? BugId { get; set; }
    public string? StackTrace { get; set; }
    public string? DbState { get; set; }
    public string Module { get; set; } = "";
    public string Feature { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Role { get; set; } = "";
}

public sealed class BugItem
{
    public string BugId { get; set; } = "";
    public string Module { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Environment { get; set; } = @".\SQLEXPRESS; RPMS WinForms";
    public string Build { get; set; } = "Debug net8.0-windows";
    public string Steps { get; set; } = "";
    public string Expected { get; set; } = "";
    public string Actual { get; set; } = "";
    public string RootCause { get; set; } = "";
    public string Screenshot { get; set; } = "N/A (headless form/service exec)";
    public string StackTrace { get; set; } = "";
    public string DatabaseState { get; set; } = "";
    public string FixSuggestion { get; set; } = "";
    public string TestCaseId { get; set; } = "";
}
