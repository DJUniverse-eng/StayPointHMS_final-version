# StayPoint HMS implementation update

## Added in this project copy
- Admin-only System Setup / capacity expansion workspace.
- Initial Admin login is routed to setup until setup completion is recorded.
- Staff startup is blocked until setup is complete, room and parking inventory exists, and positive room and parking rates are configured.
- Setup saves hotel name, address, phone, and floor count to `app_settings`.
- Room and parking capacity grids generate missing inventory transactionally; existing records are not reset.
- Manual Add Room / Add Space controls are hidden in inventory-management screens.
- Role split remains `ADMIN`, `STAFF_ROOM`, and `STAFF_PARKING`.
- Added `database-migrations/03_system_setup_tracking.sql`.

## Deployment steps
1. Back up the database.
2. Apply role migrations `01_add_staff_roles.sql` and `02_finalize_user_roles.sql` as described in `ROLE_UPDATE_README.md`. Do not finalize until every legacy `STAFF` account has been explicitly assigned.
3. Apply `03_system_setup_tracking.sql`.
4. Build and run on Windows with .NET 8 and MySqlConnector 2.4.0.
5. Sign in as Admin, save hotel information, generate inventory, then configure room and parking prices through the existing pricing screens.
6. Test both staff roles and verify unauthorized navigation and transactional actions are rejected.

## Important validation notes
- This environment does not have the .NET SDK, so this ZIP has not been compiled or runtime-tested here.
- Review the live database column definitions before running setup. This code expects the existing `app_settings`, `room_types`, `room_type_options`, `rooms`, `parking_rate_options`, and `parking_spaces` columns used by the existing project.
- The existing numbering specification needs one explicit resolution before production generation: a one-digit type code gives only nine Room IDs per floor, each holding three rooms. The example capacity of 26 rooms across the listed configurations needs ten three-room groups. The implementation's current numbering allocation should be reviewed against a final explicit type-code allocation table before using it for production inventory.
- This update does not claim that every legacy operational query in the project is fully schema-aligned; test room assignment, parking tickets, folios, payments, and reports against a backed-up copy of the actual database before accepting guests.
