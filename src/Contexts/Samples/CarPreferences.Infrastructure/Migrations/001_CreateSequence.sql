-- Migration: 001 - Create s_row_id_sequence
-- Purpose: Sequence for auto-incrementing row_id
-- Date: 2026-08-01

IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 's_row_id_sequence')
BEGIN
    CREATE SEQUENCE dbo.s_row_id_sequence
        AS BIGINT
        START WITH 1
        INCREMENT BY 1
        NO CACHE
        NO CYCLE
END
GO
