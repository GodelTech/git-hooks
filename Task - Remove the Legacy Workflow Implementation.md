Description:
Remove the old Workflow pipeline only after RunHandler uses the new compiler.

Tasks:

Remove legacy DI registrations.
Remove obsolete using directives.
Remove old project references.
Remove or rewrite legacy Workflow unit tests.
Delete:

Remove the projects from git-hooks.slnx.
Verify that no production code references Workflow.*.
Done when: the old Workflow pipeline is completely removed and the solution builds without it.
