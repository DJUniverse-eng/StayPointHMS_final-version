# StayPoint HMS — role split update

## Roles
- `ADMIN` — Admin Dashboard and administration modules.
- `STAFF_ROOM` — shared Staff Dashboard, room operations only.
- `STAFF_PARKING` — shared Staff Dashboard, parking operations only.

The UI labels are `Admin`, `Staff - Room`, and `Staff - Parking`.
The existing `STAFF` value is treated as a legacy/unassigned role, not as permission
for both departments.

## Apply to an existing database
1. Back up `staypoint_hms`.
2. Run `database-migrations/01_add_staff_roles.sql` in phpMyAdmin.
3. Sign in using an active `ADMIN` account.
4. In Staff Management, edit every `Unassigned (legacy)` account and choose its
   correct department. Do not guess; check with the account owner/manager.
5. Run `database-migrations/02_finalize_user_roles.sql`. It refuses to finalize
   while any `STAFF` accounts remain.
6. Restart the application and test each role with separate accounts.

New user creation offers only the three supported roles. The login flow rejects
legacy `STAFF` accounts until they are explicitly assigned. The staff home shows
only the department button allowed for the signed-in role; room and parking
workspace entry points also validate the role.

## Important limitation
These changes enforce access at login and operational workspace entry points.
This is not a replacement for database-level authorization for a future multi-user
service/API architecture. Continue to validate role and current database state
inside sensitive transaction workflows before committing writes.


## System Setup and inventory generation
- `SystemSetupControl.vb` is an Admin-only System Setup / capacity expansion workspace.
- `SystemSetupForm.vb` hosts the initial setup gate after Admin login until `app_settings.setup_completed=1`.
- Room and parking inventory generation uses a transaction and inserts only missing codes; existing rows are not reset.
- Initial manual-add buttons are hidden in Room Management and Parking Management. Rates remain in their existing dedicated pricing screens.
- Run `database-migrations/03_system_setup_tracking.sql` after backing up the database.
- Set room and parking rates in the pricing screens before processing chargeable operations.

Review the room type / bed code mappings against the live database before generating production inventory.
