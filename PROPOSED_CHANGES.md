# Proposed Changes: Re-add Passive Healing & Reduce Archer Damage

## High-Level Outline
1. Re-add passive healing (1 HP per turn) to PlayerController
2. Reduce Archer base damage from 2 to 1 (or configure in Inspector)

## Reasoning
- **Passive Healing:** Makes exploration viable. Without it, every hit is permanent damage. This is a core roguelike mechanic (resting/waiting).
- **Archer Nerf:** Ranged enemies are safer (can hit from distance), so lower damage balances risk/reward.

## Architecture Impact
None. Simple parameter adjustments.

- [ ] Re-add Health.Heal(1) after successful player actions
- [ ] Adjust Archer BaseAttackDamage in Archer.tscn or ArcherController.cs
