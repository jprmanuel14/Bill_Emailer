-- =============================================================================
-- BCAT PostgreSQL Database Schema Migration
-- Converted from SQL Server DDL (BCAT_09252026_stage.sql)
-- =============================================================================

CREATE SCHEMA IF NOT EXISTS dbo;
SET search_path TO dbo, public;

-- Enable UUID extension if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =============================================================================
-- TABLES
-- =============================================================================

-- Table: EngagementTeams
CREATE TABLE dbo."EngagementTeams" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Designation" TEXT,
    "Name" TEXT NOT NULL,
    "ClientCode" TEXT,
    "Email" TEXT NOT NULL,
    "ContactNumber" TEXT,
    "GroupId" TEXT NOT NULL
);

-- Table: Engagement_Secretary
CREATE TABLE dbo."Engagement_Secretary" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Secretary" VARCHAR(50),
    "Partner" VARCHAR(50),
    "Group" VARCHAR(255),
    "Is_Default" BOOLEAN
);

-- Table: Mailler
CREATE TABLE dbo."Mailler" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Email" VARCHAR(50) NOT NULL,
    "Description" VARCHAR(50) NOT NULL,
    "Name" VARCHAR(50) NOT NULL
);

-- Table: tblScheduledMail
CREATE TABLE dbo."tblScheduledMail" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ClientCode" VARCHAR(50),
    "ClientName" TEXT,
    "Recepients" TEXT,
    "CC" TEXT,
    "AccountCode" VARCHAR(50),
    "StatementDate" TIMESTAMP,
    "ScheduledDateTime" TIMESTAMP,
    "SentDate" TIMESTAMP,
    "Status" VARCHAR(50)
);

-- Table: __EFMigrationsHistory
CREATE TABLE dbo."__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL PRIMARY KEY,
    "ProductVersion" VARCHAR(32) NOT NULL
);

-- Table: Banks
CREATE TABLE dbo."Banks" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "AccountCode" TEXT,
    "AccountName" TEXT,
    "BankName" TEXT,
    "Branch" TEXT,
    "SwiftCode" TEXT,
    "AccountNumber" TEXT
);

-- Table: PHEntity
CREATE TABLE dbo."PHEntity" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "EntityCode" TEXT,
    "EntityName" TEXT,
    "ReportHierarchy" INT NOT NULL DEFAULT 0
);

-- Table: BankPHEntity
CREATE TABLE dbo."BankPHEntity" (
    "BankId" INT NOT NULL,
    "PHEntityId" INT NOT NULL,
    PRIMARY KEY ("BankId", "PHEntityId"),
    CONSTRAINT "FK_BankPHEntity_Banks_BankId" FOREIGN KEY ("BankId") REFERENCES dbo."Banks"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_BankPHEntity_PHEntity_PHEntityId" FOREIGN KEY ("PHEntityId") REFERENCES dbo."PHEntity"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_BankPHEntity_PHEntityId" ON dbo."BankPHEntity"("PHEntityId");

-- Table: CeilingAmt
CREATE TABLE dbo."CeilingAmt" (
    "Description" VARCHAR(50),
    "Amt" NUMERIC(19, 4) NOT NULL
);

-- Table: ClientRemarks
CREATE TABLE dbo."ClientRemarks" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ClientCode" TEXT NOT NULL,
    "StatementDate" TIMESTAMP NOT NULL,
    "Remarks" TEXT
);

-- Table: Contacts
CREATE TABLE dbo."Contacts" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ClientCode" TEXT,
    "Email" TEXT NOT NULL,
    "ContactNumber" TEXT,
    "Designation" TEXT,
    "FirstName" TEXT,
    "LastName" TEXT,
    "MiddleName" TEXT,
    "Salutation" TEXT,
    "LoS" VARCHAR(50),
    "GroupType" VARCHAR(50)
);

-- Table: ContactsLimit
CREATE TABLE dbo."ContactsLimit" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Limit" INT NOT NULL
);

-- Table: Deletion_Exception
CREATE TABLE dbo."Deletion_Exception" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ClientCode" TEXT NOT NULL,
    "Reason" TEXT,
    "Other_Reason" TEXT,
    "User_Responsible" VARCHAR(100) NOT NULL,
    "Date_Deleted" TIMESTAMP NOT NULL
);

-- Table: Deletion_Reason
CREATE TABLE dbo."Deletion_Reason" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Description" VARCHAR(255)
);

-- Table: Exceptions
CREATE TABLE dbo."Exceptions" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ClientCode" TEXT NOT NULL DEFAULT '',
    "Reason" TEXT,
    "ClientName" TEXT,
    "Other_Reason" VARCHAR(255),
    "Group_Code" VARCHAR(50),
    "Reference_No" VARCHAR(50),
    "BillNo" VARCHAR(50)
);

-- Table: Group
CREATE TABLE dbo."Group" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Code" TEXT,
    "Description" TEXT
);

-- Table: Log_Admin
CREATE TABLE dbo."Log_Admin" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Log_Date" DATE NOT NULL,
    "Log_Time" TIMESTAMP NOT NULL,
    "Action_Taker" VARCHAR(255) NOT NULL,
    "Action_Taken" TEXT NOT NULL
);

-- Table: OINV (SAP Business One Invoice Table)
CREATE TABLE dbo."OINV" (
    "DocEntry" INT NOT NULL PRIMARY KEY,
    "DocNum" INT,
    "DocType" CHAR(1) DEFAULT 'I',
    "CANCELED" CHAR(1) DEFAULT 'N',
    "Handwrtten" CHAR(1) DEFAULT 'N',
    "Printed" CHAR(1) DEFAULT 'N',
    "DocStatus" CHAR(1) DEFAULT 'O',
    "InvntSttus" CHAR(1) DEFAULT 'O',
    "Transfered" CHAR(1) DEFAULT 'N',
    "ObjType" VARCHAR(20) DEFAULT '13',
    "DocDate" TIMESTAMP,
    "DocDueDate" TIMESTAMP,
    "CardCode" VARCHAR(15),
    "CardName" VARCHAR(200),
    "Address" VARCHAR(254),
    "NumAtCard" VARCHAR(200),
    "VatPercent" NUMERIC(19, 6),
    "VatSum" NUMERIC(19, 6),
    "VatSumFC" NUMERIC(19, 6),
    "DiscPrcnt" NUMERIC(19, 6),
    "DiscSum" NUMERIC(19, 6),
    "DiscSumFC" NUMERIC(19, 6),
    "DocCur" VARCHAR(3),
    "DocRate" NUMERIC(19, 6),
    "DocTotal" NUMERIC(19, 6),
    "DocTotalFC" NUMERIC(19, 6),
    "PaidToDate" NUMERIC(19, 6),
    "PaidFC" NUMERIC(19, 6),
    "GrosProfit" NUMERIC(19, 6),
    "GrosProfFC" NUMERIC(19, 6),
    "Ref1" VARCHAR(11),
    "Ref2" VARCHAR(11),
    "Comments" VARCHAR(254),
    "JrnlMemo" VARCHAR(254),
    "TransId" INT,
    "ReceiptNum" INT,
    "GroupNum" SMALLINT,
    "DocTime" SMALLINT,
    "SlpCode" INT DEFAULT -1,
    "TrnspCode" SMALLINT DEFAULT -1,
    "PartSupply" CHAR(1) DEFAULT 'Y',
    "Confirmed" CHAR(1) DEFAULT 'Y',
    "GrossBase" SMALLINT DEFAULT 0,
    "ImportEnt" INT,
    "CreateTran" CHAR(1) DEFAULT 'N',
    "SummryType" CHAR(1) DEFAULT 'N',
    "UpdInvnt" CHAR(1) DEFAULT 'N',
    "UpdCardBal" CHAR(1) DEFAULT 'N',
    "Instance" SMALLINT DEFAULT 0,
    "Flags" INT DEFAULT 0,
    "InvntDirec" CHAR(1) DEFAULT 'X',
    "CntctCode" INT,
    "ShowSCN" CHAR(1) DEFAULT 'N',
    "FatherCard" VARCHAR(15),
    "SysRate" NUMERIC(19, 6),
    "CurSource" CHAR(1) DEFAULT 'C',
    "VatSumSy" NUMERIC(19, 6),
    "DiscSumSy" NUMERIC(19, 6),
    "DocTotalSy" NUMERIC(19, 6),
    "PaidSys" NUMERIC(19, 6),
    "FatherType" CHAR(1) DEFAULT 'P',
    "GrosProfSy" NUMERIC(19, 6),
    "UpdateDate" TIMESTAMP,
    "IsICT" CHAR(1) DEFAULT 'N',
    "CreateDate" TIMESTAMP,
    "Volume" NUMERIC(19, 6),
    "VolUnit" SMALLINT,
    "Weight" NUMERIC(19, 6),
    "WeightUnit" SMALLINT,
    "Series" INT,
    "TaxDate" TIMESTAMP,
    "Filler" VARCHAR(8),
    "DataSource" CHAR(1) DEFAULT 'N',
    "StampNum" VARCHAR(16),
    "isCrin" CHAR(1) DEFAULT 'N',
    "FinncPriod" INT,
    "UserSign" SMALLINT,
    "selfInv" CHAR(1) DEFAULT 'N',
    "VatPaid" NUMERIC(19, 6),
    "VatPaidFC" NUMERIC(19, 6),
    "VatPaidSys" NUMERIC(19, 6),
    "UserSign2" SMALLINT,
    "WddStatus" CHAR(1) DEFAULT '-',
    "draftKey" INT DEFAULT -1,
    "TotalExpns" NUMERIC(19, 6),
    "TotalExpFC" NUMERIC(19, 6),
    "TotalExpSC" NUMERIC(19, 6),
    "DunnLevel" INT,
    "Address2" VARCHAR(254),
    "LogInstanc" INT DEFAULT 0,
    "Exported" CHAR(1) DEFAULT 'N',
    "StationID" INT,
    "Indicator" VARCHAR(2),
    "NetProc" CHAR(1) DEFAULT 'N',
    "AqcsTax" NUMERIC(19, 6),
    "AqcsTaxFC" NUMERIC(19, 6),
    "AqcsTaxSC" NUMERIC(19, 6),
    "CashDiscPr" NUMERIC(19, 6),
    "CashDiscnt" NUMERIC(19, 6),
    "CashDiscFC" NUMERIC(19, 6),
    "CashDiscSC" NUMERIC(19, 6),
    "ShipToCode" VARCHAR(50),
    "LicTradNum" VARCHAR(32),
    "PaymentRef" VARCHAR(27),
    "WTSum" NUMERIC(19, 6),
    "WTSumFC" NUMERIC(19, 6),
    "WTSumSC" NUMERIC(19, 6),
    "RoundDif" NUMERIC(19, 6),
    "RoundDifFC" NUMERIC(19, 6),
    "RoundDifSy" NUMERIC(19, 6),
    "CheckDigit" CHAR(1),
    "Form1099" INT,
    "Box1099" VARCHAR(20),
    "submitted" CHAR(1) DEFAULT 'N',
    "PoPrss" CHAR(1) DEFAULT 'N',
    "Rounding" CHAR(1) DEFAULT 'N',
    "RevisionPo" CHAR(1) DEFAULT 'N',
    "Segment" SMALLINT DEFAULT 0,
    "ReqDate" TIMESTAMP,
    "CancelDate" TIMESTAMP,
    "PickStatus" CHAR(1) DEFAULT 'N',
    "Pick" CHAR(1) DEFAULT 'N',
    "BlockDunn" CHAR(1) DEFAULT 'N',
    "PeyMethod" VARCHAR(15),
    "PayBlock" CHAR(1) DEFAULT 'N',
    "PayBlckRef" INT,
    "MaxDscn" CHAR(1) DEFAULT 'N',
    "Reserve" CHAR(1) DEFAULT 'N',
    "Max1099" NUMERIC(19, 6),
    "CntrlBnk" VARCHAR(15),
    "PickRmrk" VARCHAR(254),
    "ISRCodLine" VARCHAR(53),
    "ExpAppl" NUMERIC(19, 6),
    "ExpApplFC" NUMERIC(19, 6),
    "ExpApplSC" NUMERIC(19, 6),
    "Project" VARCHAR(20),
    "DeferrTax" CHAR(1),
    "LetterNum" VARCHAR(50),
    "FromDate" TIMESTAMP,
    "ToDate" TIMESTAMP,
    "WTApplied" NUMERIC(19, 6),
    "WTAppliedF" NUMERIC(19, 6),
    "BoeReserev" CHAR(1) DEFAULT 'N',
    "AgentCode" VARCHAR(32),
    "WTAppliedS" NUMERIC(19, 6),
    "EquVatSum" NUMERIC(19, 6),
    "EquVatSumF" NUMERIC(19, 6),
    "EquVatSumS" NUMERIC(19, 6),
    "Installmnt" SMALLINT DEFAULT 1,
    "VATFirst" CHAR(1),
    "NnSbAmnt" NUMERIC(19, 6),
    "NnSbAmntSC" NUMERIC(19, 6),
    "NbSbAmntFC" NUMERIC(19, 6),
    "ExepAmnt" NUMERIC(19, 6),
    "ExepAmntSC" NUMERIC(19, 6),
    "ExepAmntFC" NUMERIC(19, 6),
    "VatDate" TIMESTAMP,
    "CorrExt" VARCHAR(25),
    "CorrInv" INT,
    "NCorrInv" INT,
    "CEECFlag" CHAR(1) DEFAULT 'N',
    "BaseAmnt" NUMERIC(19, 6),
    "BaseAmntSC" NUMERIC(19, 6),
    "BaseAmntFC" NUMERIC(19, 6),
    "CtlAccount" VARCHAR(15),
    "BPLId" INT,
    "BPLName" VARCHAR(200),
    "VATRegNum" VARCHAR(32),
    "TxInvRptNo" VARCHAR(10),
    "TxInvRptDt" TIMESTAMP,
    "KVVATCode" TEXT,
    "WTDetails" VARCHAR(100),
    "SumAbsId" INT DEFAULT -1,
    "SumRptDate" TIMESTAMP,
    "PIndicator" VARCHAR(10) DEFAULT ' ',
    "ManualNum" VARCHAR(20),
    "UseShpdGd" CHAR(1) DEFAULT 'N',
    "BaseVtAt" NUMERIC(19, 6),
    "BaseVtAtSC" NUMERIC(19, 6),
    "BaseVtAtFC" NUMERIC(19, 6),
    "NnSbVAt" NUMERIC(19, 6),
    "NnSbVAtSC" NUMERIC(19, 6),
    "NbSbVAtFC" NUMERIC(19, 6),
    "ExptVAt" NUMERIC(19, 6),
    "ExptVAtSC" NUMERIC(19, 6),
    "ExptVAtFC" NUMERIC(19, 6),
    "LYPmtAt" NUMERIC(19, 6),
    "LYPmtAtSC" NUMERIC(19, 6),
    "LYPmtAtFC" NUMERIC(19, 6),
    "ExpAnSum" NUMERIC(19, 6),
    "ExpAnSys" NUMERIC(19, 6),
    "ExpAnFrgn" NUMERIC(19, 6),
    "DocSubType" VARCHAR(2) DEFAULT '--',
    "DpmStatus" CHAR(1) DEFAULT 'O',
    "DpmAmnt" NUMERIC(19, 6),
    "DpmAmntSC" NUMERIC(19, 6),
    "DpmAmntFC" NUMERIC(19, 6),
    "DpmDrawn" CHAR(1) DEFAULT 'N',
    "DpmPrcnt" NUMERIC(19, 6),
    "PaidSum" NUMERIC(19, 6),
    "PaidSumFc" NUMERIC(19, 6),
    "PaidSumSc" NUMERIC(19, 6),
    "FolioPref" VARCHAR(4),
    "FolioNum" INT,
    "DpmAppl" NUMERIC(19, 6),
    "DpmApplFc" NUMERIC(19, 6),
    "DpmApplSc" NUMERIC(19, 6),
    "LPgFolioN" INT,
    "Header" TEXT,
    "Footer" TEXT,
    "Posted" CHAR(1) DEFAULT 'Y',
    "OwnerCode" INT,
    "BPChCode" VARCHAR(15),
    "BPChCntc" INT,
    "PayToCode" VARCHAR(50),
    "IsPaytoBnk" CHAR(1),
    "BnkCntry" VARCHAR(3),
    "BankCode" VARCHAR(30),
    "BnkAccount" VARCHAR(50),
    "BnkBranch" VARCHAR(50),
    "isIns" CHAR(1) DEFAULT 'N',
    "TrackNo" VARCHAR(30),
    "VersionNum" VARCHAR(13),
    "LangCode" INT,
    "BPNameOW" CHAR(1) DEFAULT 'N',
    "BillToOW" CHAR(1) DEFAULT 'N',
    "ShipToOW" CHAR(1) DEFAULT 'N',
    "RetInvoice" CHAR(1) DEFAULT 'N',
    "ClsDate" TIMESTAMP,
    "MInvNum" INT,
    "MInvDate" TIMESTAMP,
    "SeqCode" SMALLINT,
    "Serial" INT,
    "SeriesStr" VARCHAR(3),
    "SubStr" VARCHAR(3),
    "Model" VARCHAR(6) DEFAULT '0',
    "TaxOnExp" NUMERIC(19, 6),
    "TaxOnExpFc" NUMERIC(19, 6),
    "TaxOnExpSc" NUMERIC(19, 6),
    "TaxOnExAp" NUMERIC(19, 6),
    "TaxOnExApF" NUMERIC(19, 6),
    "TaxOnExApS" NUMERIC(19, 6),
    "LastPmnTyp" CHAR(1),
    "LndCstNum" INT,
    "UseCorrVat" CHAR(1) DEFAULT 'N',
    "BlkCredMmo" CHAR(1) DEFAULT 'N',
    "OpenForLaC" CHAR(1) DEFAULT 'Y',
    "Excised" CHAR(1) DEFAULT 'O',
    "ExcRefDate" TIMESTAMP,
    "ExcRmvTime" VARCHAR(8),
    "SrvGpPrcnt" NUMERIC(19, 6),
    "DepositNum" INT,
    "CertNum" VARCHAR(31),
    "DutyStatus" CHAR(1) DEFAULT 'Y',
    "AutoCrtFlw" CHAR(1) DEFAULT 'N',
    "FlwRefDate" TIMESTAMP,
    "FlwRefNum" VARCHAR(100),
    "VatJENum" INT DEFAULT -1,
    "DpmVat" NUMERIC(19, 6),
    "DpmVatFc" NUMERIC(19, 6),
    "DpmVatSc" NUMERIC(19, 6),
    "DpmAppVat" NUMERIC(19, 6),
    "DpmAppVatF" NUMERIC(19, 6),
    "DpmAppVatS" NUMERIC(19, 6),
    "InsurOp347" CHAR(1) DEFAULT 'N',
    "IgnRelDoc" CHAR(1) DEFAULT 'N',
    "BuildDesc" VARCHAR(50),
    "ResidenNum" CHAR(1) DEFAULT '1',
    "Checker" INT,
    "Payee" INT,
    "CopyNumber" INT DEFAULT 0,
    "SSIExmpt" CHAR(1),
    "PQTGrpSer" INT,
    "PQTGrpNum" INT,
    "PQTGrpHW" CHAR(1) DEFAULT 'N',
    "ReopOriDoc" CHAR(1),
    "ReopManCls" CHAR(1),
    "DocManClsd" CHAR(1) DEFAULT 'N',
    "ClosingOpt" SMALLINT DEFAULT 1,
    "SpecDate" TIMESTAMP,
    "Ordered" CHAR(1) DEFAULT 'N',
    "NTSApprov" CHAR(1) DEFAULT 'N',
    "NTSWebSite" SMALLINT,
    "NTSeTaxNo" VARCHAR(50),
    "NTSApprNo" VARCHAR(50),
    "PayDuMonth" CHAR(1),
    "ExtraMonth" SMALLINT,
    "ExtraDays" SMALLINT,
    "CdcOffset" SMALLINT DEFAULT 0,
    "SignMsg" TEXT,
    "SignDigest" TEXT,
    "CertifNum" VARCHAR(50),
    "KeyVersion" INT,
    "EDocGenTyp" CHAR(1) DEFAULT 'N',
    "ESeries" SMALLINT,
    "EDocNum" VARCHAR(50),
    "EDocExpFrm" INT,
    "OnlineQuo" CHAR(1) DEFAULT 'N',
    "POSEqNum" VARCHAR(20),
    "POSManufSN" VARCHAR(20),
    "POSCashN" INT,
    "EDocStatus" CHAR(1) DEFAULT 'C',
    "EDocCntnt" TEXT,
    "EDocProces" CHAR(1) DEFAULT 'C',
    "EDocErrCod" VARCHAR(50),
    "EDocErrMsg" TEXT,
    "EDocCancel" CHAR(1) DEFAULT 'N',
    "EDocTest" CHAR(1) DEFAULT 'N',
    "EDocPrefix" VARCHAR(10),
    "CUP" INT,
    "CIG" INT,
    "DpmAsDscnt" CHAR(1) DEFAULT 'N',
    "Attachment" TEXT,
    "AtcEntry" INT,
    "SupplCode" VARCHAR(254),
    "GTSRlvnt" CHAR(1) DEFAULT 'N',
    "BaseDisc" NUMERIC(19, 6),
    "BaseDiscSc" NUMERIC(19, 6),
    "BaseDiscFc" NUMERIC(19, 6),
    "BaseDiscPr" NUMERIC(19, 6),
    "CreateTS" INT,
    "UpdateTS" INT,
    "SrvTaxRule" CHAR(1) DEFAULT 'N',
    "AnnInvDecR" INT,
    "Supplier" VARCHAR(15),
    "Releaser" INT,
    "Receiver" INT,
    "ToWhsCode" VARCHAR(8),
    "AssetDate" TIMESTAMP,
    "Requester" VARCHAR(25),
    "ReqName" VARCHAR(155),
    "Branch" SMALLINT,
    "Department" SMALLINT,
    "Email" VARCHAR(100),
    "Notify" CHAR(1),
    "ReqType" INT DEFAULT 12,
    "OriginType" CHAR(1) DEFAULT 'M',
    "IsReuseNum" CHAR(1) DEFAULT 'N',
    "IsReuseNFN" CHAR(1) DEFAULT 'N',
    "DocDlvry" CHAR(1),
    "PaidDpm" NUMERIC(19, 6),
    "PaidDpmF" NUMERIC(19, 6),
    "PaidDpmS" NUMERIC(19, 6),
    "EnvTypeNFe" INT DEFAULT -1,
    "AgrNo" INT,
    "IsAlt" CHAR(1) DEFAULT 'N',
    "AltBaseTyp" INT DEFAULT -1,
    "AltBaseEnt" INT,
    "AuthCode" VARCHAR(250),
    "StDlvDate" TIMESTAMP,
    "StDlvTime" INT,
    "EndDlvDate" TIMESTAMP,
    "EndDlvTime" INT,
    "VclPlate" VARCHAR(20),
    "ElCoStatus" VARCHAR(10),
    "AtDocType" VARCHAR(10),
    "ElCoMsg" VARCHAR(254),
    "PrintSEPA" CHAR(1) DEFAULT 'N',
    "FreeChrg" NUMERIC(19, 6),
    "FreeChrgFC" NUMERIC(19, 6),
    "FreeChrgSC" NUMERIC(19, 6),
    "NfeValue" NUMERIC(19, 6),
    "FiscDocNum" VARCHAR(100),
    "RelatedTyp" INT DEFAULT -1,
    "RelatedEnt" INT,
    "CCDEntry" INT,
    "NfePrntFo" INT DEFAULT 0,
    "ZrdAbs" INT,
    "POSRcptNo" INT,
    "FoCTax" NUMERIC(19, 6),
    "FoCTaxFC" NUMERIC(19, 6),
    "FoCTaxSC" NUMERIC(19, 6),
    "TpCusPres" INT,
    "ExcDocDate" TIMESTAMP,
    "FoCFrght" NUMERIC(19, 6),
    "FoCFrghtFC" NUMERIC(19, 6),
    "FoCFrghtSC" NUMERIC(19, 6),
    "InterimTyp" SMALLINT DEFAULT 0,
    "PTICode" VARCHAR(5),
    "Letter" CHAR(1),
    "FolNumFrom" INT,
    "FolNumTo" INT,
    "FolSeries" INT,
    "SplitTax" NUMERIC(19, 6),
    "SplitTaxFC" NUMERIC(19, 6),
    "SplitTaxSC" NUMERIC(19, 6),
    "ToBinCode" VARCHAR(228),
    "PriceMode" CHAR(1),
    "PoDropPrss" CHAR(1) DEFAULT 'N',
    "PermitNo" VARCHAR(20),
    "MYFtype" VARCHAR(2),
    "DocTaxID" VARCHAR(32),
    "DateReport" TIMESTAMP,
    "RepSection" VARCHAR(3),
    "ExclTaxRep" CHAR(1) DEFAULT 'N',
    "PosCashReg" INT,
    "DmpTransID" VARCHAR(20),
    "ECommerBP" VARCHAR(15),
    "EComerGSTN" VARCHAR(15),
    "Revision" CHAR(1) DEFAULT 'N',
    "RevRefNo" VARCHAR(100),
    "RevRefDate" TIMESTAMP,
    "RevCreRefN" VARCHAR(100),
    "RevCreRefD" TIMESTAMP,
    "TaxInvNo" VARCHAR(100),
    "FrmBpDate" TIMESTAMP,
    "GSTTranTyp" VARCHAR(2),
    "BaseType" INT DEFAULT -1,
    "BaseEntry" INT,
    "ComTrade" CHAR(1) DEFAULT 'E',
    "UseBilAddr" CHAR(1),
    "IssReason" SMALLINT DEFAULT 1,
    "ComTradeRt" CHAR(1) DEFAULT 'N',
    "SplitPmnt" CHAR(1) DEFAULT 'N',
    "SOIWizId" INT,
    "SelfPosted" CHAR(1) DEFAULT 'N',
    "EnBnkAcct" TEXT,
    "EncryptIV" VARCHAR(100),
    "DPPStatus" CHAR(1) DEFAULT 'N',
    "SAPPassprt" TEXT,
    "EWBGenType" CHAR(1),
    "CtActTax" NUMERIC(19, 6),
    "CtActTaxFC" NUMERIC(19, 6),
    "CtActTaxSC" NUMERIC(19, 6),
    "EDocType" CHAR(1) DEFAULT 'F',
    "QRCodeSrc" TEXT,
    "AggregDoc" CHAR(1) DEFAULT 'N',
    "DataVers" INT DEFAULT 1,
    "ShipState" VARCHAR(3),
    "ShipPlace" VARCHAR(60),
    "CustOffice" VARCHAR(60),
    "FCI" VARCHAR(36),
    "NnSbCuAmnt" NUMERIC(19, 6),
    "NnSbCuSC" NUMERIC(19, 6),
    "NnSbCuFC" NUMERIC(19, 6),
    "ExepCuAmnt" NUMERIC(19, 6),
    "ExepCuSC" NUMERIC(19, 6),
    "ExepCuFC" NUMERIC(19, 6),
    "AddLegIn" VARCHAR(100),
    "LegTextF" INT,
    "IndFinal" CHAR(1) DEFAULT 'N',
    "DANFELgTxt" TEXT,
    "PostPmntWT" CHAR(1) DEFAULT 'N',
    "QRCodeSPGn" TEXT,
    "FCEPmnMean" CHAR(1) DEFAULT 'N',
    "ReqCode" VARCHAR(50),
    "NotRel4MI" CHAR(1) DEFAULT 'N',
    "Rel4PPTax" CHAR(1) DEFAULT 'N',
    "ConfrmedBy" INT,
    "ConfrmedOn" TIMESTAMP,
    "ReqID" INT,
    "NonDdAmt" NUMERIC(19, 6),
    "NonDdAmtSC" NUMERIC(19, 6),
    "NonDdAmtFC" NUMERIC(19, 6),
    "BookeTdsBP" CHAR(1) DEFAULT 'N',
    "AllocNum" VARCHAR(50),
    "DPayToAddr" VARCHAR(50),
    "DigPayment" CHAR(1) DEFAULT 'N',
    "OperProfit" NUMERIC(19, 6),
    "OperProfFC" NUMERIC(19, 6),
    "OperProfSy" NUMERIC(19, 6),
    "NetIncome" NUMERIC(19, 6),
    "NetIncomFC" NUMERIC(19, 6),
    "NetIncomSy" NUMERIC(19, 6),
    "U_VAN" VARCHAR(30),
    "U_ExpDate" TIMESTAMP,
    "U_Docs" VARCHAR(20),
    "U_BankAccounts" VARCHAR(20),
    "U_SubconARN" VARCHAR(15),
    "U_TransType" VARCHAR(15),
    "U_CPR" VARCHAR(20),
    "U_WithCPR" VARCHAR(10),
    "U_CARF" VARCHAR(20),
    "U_CancellationReason" VARCHAR(100),
    "U_PreparedBy" VARCHAR(100),
    "U_EMPCODE" VARCHAR(30),
    "U_EMPNAME" VARCHAR(100),
    "U_APP_Bank" VARCHAR(30),
    "U_APP_BankBranch" VARCHAR(50),
    "U_APP_BankAcct" VARCHAR(20),
    "U_APP_SwiftCode" VARCHAR(15)
);

-- Table: Secretary_Logs
CREATE TABLE dbo."Secretary_Logs" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Date" DATE DEFAULT CURRENT_DATE,
    "Time" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "Action_Taker" VARCHAR(100),
    "Action_Taken" TEXT
);

-- Table: tbBCATData
CREATE TABLE dbo."tbBCATData" (
    "AgeBracket" VARCHAR(50),
    "BillNo" VARCHAR(50),
    "ReferenceNo" VARCHAR(20),
    "TransDate" TIMESTAMP,
    "Age" INT,
    "ARClientCode" VARCHAR(10),
    "ARClientName" VARCHAR(255),
    "StaffName" VARCHAR(50),
    "chBillEntityCode" CHAR(3),
    "vcPracticeName" VARCHAR(100),
    "Dollar" NUMERIC(19, 4),
    "Peso" NUMERIC(19, 4),
    "Manager" VARCHAR(255),
    "Remarks" TEXT,
    "RunDate" TIMESTAMP,
    "Currency" VARCHAR(50),
    "StatementDate" DATE,
    "SentDate" DATE
);

-- Table: tbl_Log_Clients
CREATE TABLE dbo."tbl_Log_Clients" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Date" DATE,
    "Time" TIMESTAMP,
    "User_Name" VARCHAR(100),
    "Client_Name" TEXT,
    "inv_Number" VARCHAR(50),
    "Action" VARCHAR(50),
    "Previous_Reason" TEXT,
    "Current_Reason" TEXT,
    "Deletion_Reason" TEXT,
    "BillNo" VARCHAR(50)
);

-- Table: tbl_Reasons
CREATE TABLE dbo."tbl_Reasons" (
    "Id" INT,
    "Description" VARCHAR(255)
);

-- Table: tblScheduledMail_v2
CREATE TABLE dbo."tblScheduledMail_v2" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ClientCode" VARCHAR(50),
    "ClientName" TEXT,
    "Recepients" TEXT,
    "CC" TEXT,
    "AccountCode" VARCHAR(50),
    "StatementDate" TIMESTAMP,
    "ScheduledDateTime" TIMESTAMP,
    "SentDate" TIMESTAMP,
    "Status" VARCHAR(50),
    "GroupCode" VARCHAR(50),
    "BillNo" VARCHAR(50),
    "ReferenceNo" VARCHAR(50)
);

-- Table: tblScheduledMailTemp
CREATE TABLE dbo."tblScheduledMailTemp" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ClientCode" VARCHAR(50),
    "ClientName" TEXT,
    "Recepients" TEXT,
    "CC" TEXT,
    "AccountCode" VARCHAR(50),
    "StatementDate" TIMESTAMP,
    "ScheduledDateTime" TIMESTAMP,
    "SentDate" TIMESTAMP,
    "Status" VARCHAR(50)
);

-- Table: User_Role
CREATE TABLE dbo."User_Role" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "E_Code" VARCHAR(10) NOT NULL,
    "EmployeeName" VARCHAR(100) NOT NULL,
    "User_Role" VARCHAR(20) NOT NULL,
    "Date_Assigned" TIMESTAMP NOT NULL
);

-- Table: Users
CREATE TABLE dbo."Users" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "GUID" TEXT,
    "Email" TEXT
);

-- =============================================================================
-- FUNCTIONS
-- =============================================================================

-- Function: DaysInYear
CREATE OR REPLACE FUNCTION dbo."DaysInYear"(p_Year INT)
RETURNS INT AS $$
DECLARE
    v_Jan1 TIMESTAMP;
    v_NextJan1 TIMESTAMP;
BEGIN
    IF p_Year < 1753 OR p_Year > 9998 THEN
        RETURN -1;
    END IF;

    v_Jan1 := MAKE_DATE(p_Year, 1, 1);
    v_NextJan1 := MAKE_DATE(p_Year + 1, 1, 1);
    
    RETURN (v_NextJan1::DATE - v_Jan1::DATE);
END;
$$ LANGUAGE plpgsql;

-- Function: fnc_FiscalYear
CREATE OR REPLACE FUNCTION dbo."fnc_FiscalYear"(p_AsOf TIMESTAMP)
RETURNS INT AS $$
BEGIN
    IF EXTRACT(MONTH FROM p_AsOf) > 6 THEN
        RETURN EXTRACT(YEAR FROM p_AsOf)::INT - 1;
    ELSE
        RETURN EXTRACT(YEAR FROM p_AsOf)::INT;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Function: IsLeapYear
CREATE OR REPLACE FUNCTION dbo."IsLeapYear"(p_Year INT)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN (p_Year % 4 = 0 AND (p_Year % 100 <> 0 OR p_Year % 400 = 0));
END;
$$ LANGUAGE plpgsql;

-- Function: GetFiscalDates
CREATE OR REPLACE FUNCTION dbo."GetFiscalDates"(
    p_TheDate DATE,
    p_FiscalStartMonthNo INT,
    p_Period VARCHAR(2) DEFAULT 'FY'
)
RETURNS TABLE (
    "StartDate" DATE,
    "EndDate" DATE,
    "Period" VARCHAR(20)
) AS $$
DECLARE
    v_StartDate DATE;
BEGIN
    v_StartDate := DATE_TRUNC('month', p_TheDate)::DATE 
                   + (INTERVAL '1 month' * (p_FiscalStartMonthNo - EXTRACT(MONTH FROM p_TheDate)
                   - CASE WHEN p_FiscalStartMonthNo > EXTRACT(MONTH FROM p_TheDate) THEN 12 ELSE 0 END));

    IF p_Period = 'FY' THEN
        RETURN QUERY
        SELECT v_StartDate AS "StartDate",
               (v_StartDate + INTERVAL '1 year' - INTERVAL '1 day')::DATE AS "EndDate",
               ('FY' || TO_CHAR(v_StartDate + INTERVAL '1 year' - INTERVAL '1 day', 'YYYY'))::VARCHAR(20) AS "Period";
    ELSIF p_Period = 'Q' THEN
        RETURN QUERY
        SELECT (v_StartDate + (INTERVAL '3 months' * (n - 1)))::DATE AS "StartDate",
               (v_StartDate + (INTERVAL '3 months' * n) - INTERVAL '1 day')::DATE AS "EndDate",
               ('FY' || TO_CHAR(v_StartDate + INTERVAL '1 year' - INTERVAL '1 day', 'YYYY') || ' Q' || n::TEXT)::VARCHAR(20) AS "Period"
        FROM GENERATE_SERIES(1, 4) AS n;
    ELSIF p_Period = 'M' THEN
        RETURN QUERY
        SELECT (v_StartDate + (INTERVAL '1 month' * (n - 1)))::DATE AS "StartDate",
               (v_StartDate + (INTERVAL '1 month' * n) - INTERVAL '1 day')::DATE AS "EndDate",
               ('FY' || TO_CHAR(v_StartDate + INTERVAL '1 year' - INTERVAL '1 day', 'YYYY') || ' M' || n::TEXT)::VARCHAR(20) AS "Period"
        FROM GENERATE_SERIES(1, 12) AS n;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Function: GetFiscalYearPeriod
CREATE OR REPLACE FUNCTION dbo."GetFiscalYearPeriod"(p_current_date DATE)
RETURNS DATE AS $$
DECLARE
    v_date_result DATE;
    v_currentDay INT;
BEGIN
    v_currentDay := EXTRACT(DAY FROM p_current_date)::INT;

    IF v_currentDay > 15 THEN
        SELECT (DATE_TRUNC('month', p_current_date) + INTERVAL '1 month' - INTERVAL '1 day')::DATE INTO v_date_result;
    ELSE
        SELECT (DATE_TRUNC('month', p_current_date - INTERVAL '1 month') + INTERVAL '14 days')::DATE INTO v_date_result;
    END IF;

    RETURN v_date_result;
END;
$$ LANGUAGE plpgsql;

-- =============================================================================
-- VIEWS
-- =============================================================================

-- View: vwScheduledSendDates
CREATE OR REPLACE VIEW dbo."vwScheduledSendDates" AS
SELECT DISTINCT "ScheduledDateTime", "StatementDate"
FROM dbo."tblScheduledMail";

-- View: vwClients
-- Note: Replaced external T-SQL table reference [FinAppsDM].[dbo].[tblDebtor] with local stub or FDW schema
CREATE OR REPLACE VIEW dbo."vwClients" AS
SELECT DISTINCT "ARClientCode" AS "ClientCode", "ARClientName" AS "ClientName"
FROM dbo."tbBCATData";

-- =============================================================================
-- STORED PROCEDURES (PL/pgSQL)
-- =============================================================================

-- Procedure: sp_UPDATE_Sent_Date_NEW
CREATE OR REPLACE PROCEDURE dbo."sp_UPDATE_Sent_Date_NEW"(
    p_Statementdate DATE,
    p_client_code VARCHAR(50),
    p_ref_no VARCHAR(50)
) AS $$
BEGIN
    UPDATE dbo."tblScheduledMail" 
    SET "SentDate" = CURRENT_TIMESTAMP 
    WHERE "StatementDate" = p_Statementdate AND "ClientCode" = p_client_code;

    UPDATE dbo."tblScheduledMail_v2" SCHE
    SET "SentDate" = CURRENT_TIMESTAMP
    FROM dbo."Contacts" C
    WHERE SCHE."ClientCode" = C."ClientCode" 
      AND SCHE."GroupCode" = C."GroupType" 
      AND C."Email" IS NOT NULL
      AND SCHE."StatementDate" = p_Statementdate 
      AND SCHE."ClientCode" = p_client_code;

    UPDATE dbo."tbBCATData" BCTD
    SET "SentDate" = CURRENT_TIMESTAMP
    FROM dbo."tblScheduledMail_v2" SCHE
    JOIN dbo."Contacts" C ON SCHE."ClientCode" = C."ClientCode" AND SCHE."GroupCode" = C."GroupType" AND C."Email" IS NOT NULL
    WHERE BCTD."ARClientCode" = SCHE."ClientCode" 
      AND RIGHT(SCHE."BillNo", LENGTH(SCHE."BillNo") - 3) = BCTD."BillNo" 
      AND BCTD."StatementDate" = SCHE."StatementDate"::DATE
      AND SCHE."StatementDate" = p_Statementdate 
      AND BCTD."ARClientCode" = p_client_code;
END;
$$ LANGUAGE plpgsql;

-- Procedure: UpdateGroups
CREATE OR REPLACE PROCEDURE dbo."UpdateGroups"()
AS $$
BEGIN
    -- Using INSERT ... ON CONFLICT for UPSERT in PostgreSQL
    INSERT INTO dbo."Group" ("Code", "Description")
    SELECT "Code", "Description" FROM dbo."Group"
    ON CONFLICT ("Id") 
    DO UPDATE SET 
        "Code" = EXCLUDED."Code",
        "Description" = EXCLUDED."Description";
END;
$$ LANGUAGE plpgsql;

-- Procedure: sp_run_after_agentfinancedb
CREATE OR REPLACE PROCEDURE dbo."sp_run_after_agentfinancedb"()
AS $$
DECLARE
    v_currentStatementDate DATE;
BEGIN
    SELECT "StatementDate" INTO v_currentStatementDate
    FROM dbo."tbBCATData"
    ORDER BY "StatementDate" DESC
    LIMIT 1;

    INSERT INTO dbo."Exceptions" ("ClientCode", "ClientName", "Reason")
    SELECT "ClientCode", "ClientName", 'HOLD for sending'
    FROM dbo."tblScheduledMail"
    WHERE "Recepients" IS NULL
      AND "ClientCode" NOT IN (SELECT "ClientCode" FROM dbo."Exceptions")
      AND "StatementDate"::DATE = v_currentStatementDate;

    UPDATE dbo."tblScheduledMail"
    SET "Status" = 'HOLD'
    WHERE "StatementDate"::DATE = v_currentStatementDate
      AND "ClientCode" IN (SELECT "ClientCode" FROM dbo."Exceptions");
END;
$$ LANGUAGE plpgsql;
