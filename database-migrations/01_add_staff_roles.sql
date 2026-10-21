-- StayPoint HMS: introduce the two staff role values without guessing
-- which department existing STAFF accounts belong to.
-- BACK UP staypoint_hms before running this migration.
--
-- Step 1: expand the enum temporarily so existing STAFF rows remain valid.
-- The app login intentionally rejects legacy STAFF until an administrator
-- explicitly edits each legacy account and assigns Staff - Room or Staff - Parking.
-- The role editor shows legacy rows as "Unassigned (legacy)" and requires a choice.

ALTER TABLE users
    MODIFY COLUMN role ENUM('ADMIN','STAFF','STAFF_ROOM','STAFF_PARKING')
    NOT NULL;

-- Verify current accounts and decide their department with the administrator:
SELECT user_id, full_name, username, role, is_active
FROM users
ORDER BY user_id;

-- After running the application:
-- 1. Sign in with an ADMIN account.
-- 2. Open Staff Management.
-- 3. Edit every account whose role is "Unassigned (legacy)".
-- 4. Select exactly one role: Staff - Room or Staff - Parking.
-- 5. Save and repeat until no rows remain with role = 'STAFF'.
--
-- Then run 02_finalize_user_roles.sql.
