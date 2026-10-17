# AD-006: Input reading

**Date:** 2026-10-17 · **Closes:** OD-006 · **Decided by:** owner

## Decision
One `InputReader` component reads the Input System devices directly (`Mouse.current`, `Keyboard.current`). Gameplay reads only `InputReader`.

## Rejected alternatives
A Nullflame map in the template's `InputSystem_Actions`: rejected as more setup and an edit to a template file. A new action asset: rejected as the same setup cost; rebinding isn't needed for the jam.

## Consequences
IMPL §7.4 is no longer a proposal.
