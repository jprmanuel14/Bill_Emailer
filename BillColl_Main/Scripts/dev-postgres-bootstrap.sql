-- =============================================================================
-- dev-postgres-bootstrap.sql
--
-- DEVELOPMENT ONLY. DO NOT RUN AGAINST STAGE OR PRODUCTION.
--
-- Why this exists
--   BCAT is the only database migrated to PostgreSQL so far. The legacy SQL
--   Server deployment also read from two other servers:
--     * PH_MLAAPP004S.[FinAppsDM]           -> tblBill
--     * PH_MLAAPP002S.[PH Report Databank]  -> tblGroupCode
--   Those are not migrated. This script recreates the small surface the app
--   actually touches, as empty stubs, so the application runs end to end in
--   development with no errors.
--
--   Nothing here is referenced by application code paths on STAGE/PRODUCTION:
--   those environments still have the real FinAppsDM / PH Report Databank
--   tables, so the same build deploys unchanged. The SQL branches in
--   BillingMailStatusSQLRepository always read dbo."tblBill" and
--   dbo."tblGroupCode" regardless of provider.
--
-- This script is idempotent and safe to re-run. It RESETS the development demo
-- dataset on every run (section 5), which is intentional.
--
-- Usage
--   psql -h localhost -p 5432 -U postgres -d BCAT -f dev-postgres-bootstrap.sql
-- =============================================================================

BEGIN;

-- =============================================================================
-- 1. Schema parity
--
--    a) EngagementTeams."GroupId" is TEXT in the imported database, but both the
--       EF model (EngagementTeam.GroupId is int) and the original SQL Server
--       schema use INT. Left as-is, EF throws when materialising the table.
--
--    b) tbBCATData."chBillEntityCode" was imported as character(3), but it
--       carries the entity code that PHEntity."EntityCode" / Banks."AccountCode"
--       hold ('PWC01', 5 chars). Widened to match its sibling key columns.
-- =============================================================================
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema='dbo' AND table_name='EngagementTeams'
          AND column_name='GroupId' AND data_type <> 'integer')
    THEN
        ALTER TABLE dbo."EngagementTeams"
            ALTER COLUMN "GroupId" TYPE integer USING NULLIF(TRIM("GroupId"), '')::integer;
    END IF;
END
$$;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema='dbo' AND table_name='tbBCATData'
          AND column_name='chBillEntityCode'
          AND (data_type IN ('character','bpchar') OR character_maximum_length < 50))
    THEN
        ALTER TABLE dbo."tbBCATData"
            ALTER COLUMN "chBillEntityCode" TYPE character varying(50);
    END IF;
END
$$;

-- =============================================================================
-- 2. Missing tables (stubs for the two unmigrated SQL Server databanks)
-- =============================================================================

-- PH_MLAAPP002S.[PH Report Databank].tblGroupCode
CREATE SEQUENCE IF NOT EXISTS dbo."tblGroupCode_Id_seq";
CREATE TABLE IF NOT EXISTS dbo."tblGroupCode"
(
    "Id"           integer          NOT NULL DEFAULT nextval('dbo."tblGroupCode_Id_seq"'),
    "chOfficeCode" character varying NULL,
    "chGroupCode"  character varying NULL,
    "vcGroupDesc"  character varying NULL
);

-- PH_MLAAPP004S.[FinAppsDM].tblBill
CREATE SEQUENCE IF NOT EXISTS dbo."tblBill_Id_seq";
CREATE TABLE IF NOT EXISTS dbo."tblBill"
(
    "Id"               integer          NOT NULL DEFAULT nextval('dbo."tblBill_Id_seq"'),
    "chBillNo"         character varying NULL,
    "chDebtorCode"     character varying NULL,
    "chBillOfficeCode" character varying NULL,
    "chBillGroupCode"  character varying NULL
);

-- =============================================================================
-- 3. Missing columns on dbo."tblScheduledMail"
--    Present on dbo."tblScheduledMail_v2" in the migrated database, but the
--    PostgreSQL code branches read them from tblScheduledMail.
-- =============================================================================
ALTER TABLE dbo."tblScheduledMail" ADD COLUMN IF NOT EXISTS "BillNo"      character varying NULL;
ALTER TABLE dbo."tblScheduledMail" ADD COLUMN IF NOT EXISTS "ReferenceNo" character varying NULL;
ALTER TABLE dbo."tblScheduledMail" ADD COLUMN IF NOT EXISTS "GroupCode"   character varying NULL;

-- =============================================================================
-- 4. Seed rows declared by EF HasData but never applied
--    __EFMigrationsHistory is empty, so the OnModelCreating seeds were never
--    inserted. ContactsLimit is the one the dashboard calls directly.
-- =============================================================================
INSERT INTO dbo."ContactsLimit" ("Id", "Limit")
OVERRIDING SYSTEM VALUE
SELECT 1, 5
WHERE NOT EXISTS (SELECT 1 FROM dbo."ContactsLimit" WHERE "Id" = 1);

-- =============================================================================
-- 5. Development demo dataset
--
--    BillNo linkage the SQL depends on
--      * list queries  : RIGHT(sm."BillNo", LENGTH(sm."BillNo") - 3) = bct."BillNo"
--        -> tblScheduledMail.BillNo is a 3-char prefix + tbBCATData.BillNo
--           e.g. 'INV000123' -> RIGHT('INV000123', 6) = '000123'
--      * detail queries: bct."BillNo" = sm."BillNo"
--        -> mutually exclusive with the above (see NOTES at bottom)
--
--    GroupType linkage
--      tblScheduledMail.GroupCode = CONCAT(tblGroupCode.chOfficeCode, chGroupCode)
--                               = Contacts.GroupType
--                               = CONCAT(tblBill.chBillOfficeCode, chBillGroupCode)
-- =============================================================================

DELETE FROM dbo."ClientRemarks";
DELETE FROM dbo."Exceptions";
DELETE FROM dbo."EngagementTeams";
DELETE FROM dbo."Contacts";
DELETE FROM dbo."Group";
DELETE FROM dbo."tblScheduledMail";
DELETE FROM dbo."tbBCATData";
DELETE FROM dbo."tblBill";
DELETE FROM dbo."tblGroupCode";

-- Operating units (stub for PH Report Databank)
INSERT INTO dbo."tblGroupCode" ("chOfficeCode", "chGroupCode", "vcGroupDesc") VALUES
    ('01', '001', 'Tax & Legal Practice'),
    ('01', '002', 'Audit & Assurance'),
    ('02', '001', 'Advisory Services');

-- Engagement groups (EF table, used by GetGroupList and the engagement join)
INSERT INTO dbo."Group" ("Id", "Code", "Description")
OVERRIDING SYSTEM VALUE VALUES
    (1, '01001', 'Tax & Legal Practice'),
    (2, '01002', 'Audit & Assurance'),
    (3, '02001', 'Advisory Services');

-- AR data. BillNo/ReferenceNo mirror what the real aging report feeds in.
-- 'vcPracticeName' + 'chBillEntityCode' must resolve via
--   tbBCATData.vcPracticeName  = PHEntity.EntityName   ('Tax & Legal Practice')
--   PHEntity.EntityCode        = Banks.AccountCode     ('PWC01')
INSERT INTO dbo."tbBCATData"
    ("AgeBracket", "BillNo", "ReferenceNo", "TransDate", "Age",
     "ARClientCode", "ARClientName", "StaffName", "chBillEntityCode", "vcPracticeName",
     "Dollar", "Peso", "Manager", "Remarks", "RunDate", "Currency", "StatementDate", "SentDate")
VALUES
    ('Current',    '000123', 'REF99881', '2026-09-15',  0, 'CLI001', 'Acme Corporation',    'A. Reyes',    'PWC01', 'Tax & Legal Practice',    0,  125000.00, 'M. Santos', NULL, '2026-09-30', 'PHP', '2026-09-30', NULL),
    ('30 days',    '000123', 'REF99881', '2026-08-31', 30, 'CLI001', 'Acme Corporation',    'A. Reyes',    'PWC01', 'Tax & Legal Practice',    0,   75000.00, 'M. Santos', NULL, '2026-09-30', 'PHP', '2026-09-30', NULL),
    ('60 days',    '000124', 'REF99882', '2026-07-31', 60, 'CLI002', 'Globex Industries',   'B. Lim',      'PWC01', 'Tax & Legal Practice',    0,  180000.00, 'M. Santos', NULL, '2026-09-30', 'PHP', '2026-09-30', NULL),
    ('90 days',    '000125', 'REF99883', '2026-06-30', 90, 'CLI003', 'Initech Ltd',         'C. Tan',      'PWC01', 'Tax & Legal Practice', 2500,   95000.00, 'M. Santos', NULL, '2026-09-30', 'USD', '2026-09-30', NULL),
    ('30 days',    '000126', 'REF99884', '2026-07-31', 30, 'CLI004', 'Umbrella Holdings',   'D. Garcia',   'PWC01', 'Tax & Legal Practice',    0,  320000.00, 'M. Santos', NULL, '2026-08-31', 'PHP', '2026-08-31', NULL),
    ('Current',    '000127', 'REF99885', '2026-09-15',  0, 'CLI005', 'Stark Industries',    'E. Aquino',   'PWC01', 'Tax & Legal Practice',    0,  410000.00, 'M. Santos', NULL, '2026-09-30', 'PHP', '2026-09-30', NULL);

-- Bill headers (stub for FinAppsDM). chBillNo matches tbBCATData.BillNo exactly;
-- chBillOfficeCode + chBillGroupCode must equal Contacts.GroupType.
INSERT INTO dbo."tblBill" ("chBillNo", "chDebtorCode", "chBillOfficeCode", "chBillGroupCode") VALUES
    ('000123', 'CLI001', '01', '001'),
    ('000124', 'CLI002', '01', '002'),
    ('000125', 'CLI003', '02', '001'),
    ('000126', 'CLI004', '01', '001'),
    ('000127', 'CLI005', '01', '001');

-- Contacts. GroupType is the concatenated operating-unit code.
INSERT INTO dbo."Contacts"
    ("ClientCode", "Email", "ContactNumber", "Designation", "FirstName", "LastName", "MiddleName", "Salutation", "LoS", "GroupType")
VALUES
    ('CLI001', 'juan.dc@acme.example',    '+63 917 000 0001', 'Engagement Partner',  'Juan',    'Dela Cruz', 'R', 'Mr',  'Audit & Assurance', '01001'),
    ('CLI001', 'maria.s@acme.example',    '+63 917 000 0002', 'Manager',            'Maria',   'Santos',    'L', 'Ms',  'Tax & Legal',       '01001'),
    ('CLI002', 'ricardo.l@globex.example', '+63 917 000 0003', 'Engagement Partner',  'Ricardo', 'Lim',       'C', 'Mr',  'Audit & Assurance', '01002'),
    ('CLI002', 'ana.c@globex.example',     '+63 917 000 0004', 'Manager',            'Ana',     'Cruz',      'D', 'Ms',  'Tax & Legal',       '01002'),
    ('CLI003', 'peter.g@initech.example',  '+63 917 000 0005', 'Engagement Partner',  'Peter',   'Goh',       'W', 'Mr',  'Advisory',          '02001'),
    ('CLI004', 'linda.u@umbrella.example', '+63 917 000 0006', 'Engagement Partner',  'Linda',   'Uy',        'T', 'Ms',  'Audit & Assurance', '01001'),
    ('CLI005', 'tony.s@stark.example',     '+63 917 000 0007', 'Engagement Partner',  'Tony',    'Stark',     'I', 'Mr',  'Audit & Assurance', '01001');

-- Engagement teams (EF table, GetEContactsLists)
INSERT INTO dbo."EngagementTeams"
    ("Designation", "Name", "ClientCode", "Email", "ContactNumber", "GroupId")
VALUES
    ('Engagement Partner', 'Juan Dela Cruz',  'CLI001', 'juan.dc@acme.example',    '+63 917 000 0001', 1),
    ('Manager',            'Maria Santos',    'CLI001', 'maria.s@acme.example',    '+63 917 000 0002', 1),
    ('Engagement Partner', 'Ricardo Lim',     'CLI002', 'ricardo.l@globex.example', '+63 917 000 0003', 2),
    ('Engagement Partner', 'Peter Goh',       'CLI003', 'peter.g@initech.example',  '+63 917 000 0005', 3),
    ('Engagement Partner', 'Linda Uy',        'CLI004', 'linda.u@umbrella.example', '+63 917 000 0006', 1),
    ('Engagement Partner', 'Tony Stark',      'CLI005', 'tony.s@stark.example',     '+63 917 000 0007', 1);

-- Scheduled mail queue. BillNo = 3-char prefix + tbBCATData.BillNo.
--   CLI001 / CLI004  -> SentDate set        => Delivered
--   CLI002 / CLI003  -> SentDate NULL       => Undelivered
--   CLI005           -> on the exception list => excluded from Undelivered
INSERT INTO dbo."tblScheduledMail"
    ("ClientCode", "ClientName", "BillNo", "ReferenceNo", "GroupCode",
     "StatementDate", "ScheduledDateTime", "SentDate", "Status",
     "Recepients", "CC", "AccountCode")
VALUES
    ('CLI001', 'Acme Corporation',  'INV000123', 'REF99881', '01001', '2026-09-30', '2026-09-30 09:00:00', '2026-09-30 10:15:00', NULL, 'juan.dc@acme.example',    'maria.s@acme.example',    'PWC01'),
    ('CLI002', 'Globex Industries', 'INV000124', 'REF99882', '01002', '2026-09-30', '2026-09-30 09:00:00', NULL,                     NULL, 'ricardo.l@globex.example', 'ana.c@globex.example',     'PWC01'),
    ('CLI003', 'Initech Ltd',       'INV000125', 'REF99883', '02001', '2026-09-30', '2026-09-30 09:00:00', NULL,                     NULL, 'peter.g@initech.example',  NULL,                     'PWC01'),
    ('CLI004', 'Umbrella Holdings', 'INV000126', 'REF99884', '01001', '2026-08-31', '2026-08-31 09:00:00', '2026-08-31 10:20:00', NULL, 'linda.u@umbrella.example', NULL,                     'PWC01'),
    ('CLI005', 'Stark Industries',  'INV000127', 'REF99885', '01001', '2026-09-30', '2026-09-30 09:00:00', NULL,                     'HOLD', 'tony.s@stark.example', NULL,                      'PWC01');

-- Exception list. Reference_No is deliberately NOT NULL: GetUndelivered_New uses
--   ClientCode NOT IN (SELECT ClientCode ... WHERE Reference_No = sm.ReferenceNo)
-- and a single NULL there makes NOT IN return no rows at all.
INSERT INTO dbo."Exceptions"
    ("ClientCode", "Reason", "ClientName", "Other_Reason", "Group_Code", "Reference_No", "BillNo")
VALUES
    ('CLI005', 'Incorrect recipient address', 'Stark Industries', NULL, '01001', 'REF99885', '000127');

-- Remarks captured during follow-up
INSERT INTO dbo."ClientRemarks" ("ClientCode", "StatementDate", "Remarks") VALUES
    ('CLI001', '2026-09-30', 'Confirmed receipt with Juan Dela Cruz.'),
    ('CLI002', '2026-09-30', 'Client requested to be contacted next quarter.');

COMMIT;

-- =============================================================================
-- Verification
-- =============================================================================
SELECT 'dbo.tblGroupCode'    AS object_name, count(*) AS rows FROM dbo."tblGroupCode"
UNION ALL SELECT 'dbo.tblBill',            count(*) FROM dbo."tblBill"
UNION ALL SELECT 'dbo.tbBCATData',         count(*) FROM dbo."tbBCATData"
UNION ALL SELECT 'dbo.tblScheduledMail',   count(*) FROM dbo."tblScheduledMail"
UNION ALL SELECT 'dbo.Contacts',           count(*) FROM dbo."Contacts"
UNION ALL SELECT 'dbo.EngagementTeams',    count(*) FROM dbo."EngagementTeams"
UNION ALL SELECT 'dbo.Group',              count(*) FROM dbo."Group"
UNION ALL SELECT 'dbo.Exceptions',         count(*) FROM dbo."Exceptions"
UNION ALL SELECT 'dbo.ClientRemarks',      count(*) FROM dbo."ClientRemarks"
UNION ALL SELECT 'dbo.ContactsLimit',      count(*) FROM dbo."ContactsLimit"
ORDER BY 1;

-- =============================================================================
-- NOTES - pre-existing legacy inconsistency, NOT introduced by this fixture
--
--   GetDelivered_New / GetUndelivered_New join on
--       RIGHT(sm."BillNo", LENGTH(sm."BillNo") - 3) = bct."BillNo"
--   GetDeliveredInsideDetails / GetUndeliveredInsideDetails join on
--       bct."BillNo" = sm."BillNo"
--
--   These cannot both match the same rows. The fixture is built for the list
--   queries (they drive the dashboard), so the per-client "Operating Unit"
--   detail lists return empty. Confirm the intended BillNo format against the
--   legacy SQL Server data before changing either side.
-- =============================================================================
