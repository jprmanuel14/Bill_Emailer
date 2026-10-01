-- =============================================================================
-- Test Seed Data for PostgreSQL BCAT Database
-- Run this in pgAdmin 4 Query Tool after executing BCAT_09252026_stage_postgresql.sql
-- =============================================================================

SET search_path TO dbo, public;

-- 1. Insert Test Practice Entity
INSERT INTO dbo."PHEntity" ("EntityCode", "EntityName", "ReportHierarchy")
VALUES ('PWC01', 'Tax & Legal Practice', 1);

-- 2. Insert Test Bank Remittance Instruction
INSERT INTO dbo."Banks" ("AccountCode", "AccountName", "BankName", "Branch", "SwiftCode", "AccountNumber")
VALUES ('PWC01', 'Isla Lipana & Co.', 'BDO Unibank', 'Makati Main Branch', 'BNORPHMM', '001234567890');

-- 3. Insert Test Billing Data Item
INSERT INTO dbo."tbBCATData" (
    "ARClientCode", "ARClientName", "vcPracticeName", "chBillEntityCode",
    "BillNo", "ReferenceNo", "TransDate", "Age", "Currency", "Dollar", "Peso", "StatementDate"
) VALUES (
    'CLI001', 'Acme Corporation', 'Tax & Legal Practice', '001',
    'INV-2026-001', 'REF-99881', CURRENT_TIMESTAMP - INTERVAL '30 days', 30, 'PHP', 0, 150000.00, CURRENT_DATE
);

-- 4. Insert Test Scheduled Email Queue Record
INSERT INTO dbo."tblScheduledMail" (
    "ClientCode", "ClientName", "Recepients", "CC", "AccountCode", "StatementDate", "ScheduledDateTime", "SentDate", "Status"
) VALUES (
    'CLI001', 'Acme Corporation', 'testclient@example.com', 'testcc@example.com', 'PWC01', CURRENT_DATE, CURRENT_TIMESTAMP - INTERVAL '1 hour', NULL, ''
);
