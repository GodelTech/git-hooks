Description:
Move template loading and expansion to the new compilation pipeline.

Tasks:
1. Support local templates.
2. Support nested templates.
3. Pass parameters to templates.
4. Handle missing templates.
5. Detect circular template references.
6. Preserve the include chain.
7. Add unit tests for all scenarios.

Done when: templates are fully expanded into the new Domain AST without dependencies on legacy Workflow projects.
