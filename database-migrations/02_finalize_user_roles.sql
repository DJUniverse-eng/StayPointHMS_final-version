-- StayPoint HMS: finalize the users.role enum to exactly three roles.
-- Run only after all legacy STAFF accounts have been reassigned.
-- This procedure aborts instead of silently converting/unassigning accounts.

DROP PROCEDURE IF EXISTS staypoint_finalize_user_roles;

DELIMITER //

CREATE PROCEDURE staypoint_finalize_user_roles()
BEGIN
    DECLARE legacy_count INT DEFAULT 0;

    SELECT COUNT(*) INTO legacy_count
    FROM users
    WHERE role = 'STAFF';

    IF legacy_count > 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Finalize cancelled: reassign every legacy STAFF account to STAFF_ROOM or STAFF_PARKING first.';
    ELSE
        ALTER TABLE users
            MODIFY COLUMN role ENUM('ADMIN','STAFF_ROOM','STAFF_PARKING')
            NOT NULL;
    END IF;
END//

DELIMITER ;

CALL staypoint_finalize_user_roles();
DROP PROCEDURE staypoint_finalize_user_roles;

-- Confirm the final role values and account assignments:
SHOW COLUMNS FROM users LIKE 'role';
SELECT role, COUNT(*) AS account_count
FROM users
GROUP BY role
ORDER BY role;
