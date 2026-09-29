Description:
Keep the existing RunHandler, but replace its dependency on the legacy resolver.

Tasks:

Remove the dependency on:
Inject the new compiler facade.
Read the YAML file.
Create SourceDocument and compilation context.
Pass parameter overrides to the compiler.
Process CompilationResult.
Preserve existing Git checks and exit-code behavior.
Keep the existing Error Handling approach during this migration.


Done when: RunHandler works through the new compiler and has no dependency on Workflow.*.
