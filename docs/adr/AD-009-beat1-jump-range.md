# AD-009: Beat 1 wrong-guard jump range

**Date:** 2026-10-17 · **Closes:** OD-013 · **Decided by:** owner

## Decision
Accept that the other wave guards are within jump range of D-PLAYER. Riding a wrong guard leads to the Beat 1 soft fail, as §9 Beat 1 already says. The Player Room floor shadow covers the whole room.

## Rejected alternatives
End the room shadow 1.5 m short of the door: rejected because the player has to wait inside the room. Lower T-JMP-RANGE: rejected because it breaks the Beat 3 lever jump.

## Consequences
Design doc §9.1's sentence about the other guards being out of jump range needs correcting (owner edit).
