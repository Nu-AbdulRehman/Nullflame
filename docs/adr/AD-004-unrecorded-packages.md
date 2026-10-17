# AD-004: Packages added after the template commit

**Date:** 2026-10-17 · **Closes:** OD-005 · **Decided by:** owner

## Decision
Code uses none of Cinemachine 3.1.7, ProBuilder 6.1.3, Visual Effect Graph 17.3.0 or glTFast 6.20.0. The camera is the custom rig, the greybox comes from `FacilityBuilder`, VFX use the Particle System. glTFast may be used only if the pocket watch arrives as glTF.

## Rejected alternatives
Cinemachine for the camera: rejected to avoid a dependency for a small rig. ProBuilder for touch-ups: rejected because hand-modelled geometry can't be rebuilt from `FacilityLayout`.

## Consequences
The packages stay installed; removing them is a separate owner decision.
