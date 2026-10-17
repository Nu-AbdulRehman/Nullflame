# AD-013: Scanlines via a Full Screen Pass renderer feature

**Date:** 2026-10-17 · **Closes:** OD-009 · **Decided by:** owner

## Decision
Draw the time-power scanlines with a URP Full Screen Pass renderer feature on `PC_Renderer`, using a scanline Shader Graph material. `ScreenFx` drives the material's intensity, and the feature is active only while a time power runs. Desaturation stays a Volume Color Adjustments override.

## Rejected alternatives
Full-screen UI RawImage with a tiled texture: rejected by the owner in favour of the better-looking pass.

## Consequences
This approves only this renderer-asset change; any other pipeline change still needs approval. Verify with `unity_docs` that the Full Screen Pass Renderer Feature exists in URP 17.3 and how to toggle it at runtime. Ask before also adding it to `Mobile_Renderer`.
