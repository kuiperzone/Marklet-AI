// Disable concurrent for Control tests
[assembly: CollectionBehavior(DisableTestParallelization = true)]

// Short-hand to run these tests:
// dotnet test --filter Project=PixieChrome
[assembly: AssemblyTrait("Project", "PixieChrome")]