-- StayPoint HMS System Setup tracking
-- This is additive and preserves all existing operational records.
-- Setup metadata is also mirrored in app_settings for compatibility with the application.
CREATE TABLE IF NOT EXISTS system_setup (
    setup_id TINYINT UNSIGNED NOT NULL PRIMARY KEY,
    setup_completed TINYINT(1) NOT NULL DEFAULT 0,
    completed_by BIGINT UNSIGNED NULL,
    completed_at DATETIME NULL,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT chk_system_setup_singleton CHECK (setup_id = 1)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO system_setup (setup_id, setup_completed)
VALUES (1, 0)
ON DUPLICATE KEY UPDATE setup_id = VALUES(setup_id);
