USE MAINDB
GO

-------------------------------------------------------------------------------

IF NOT EXISTS ( SELECT  *
                FROM    sys.schemas
                WHERE   name = N'ServiceCenter' ) 
    EXEC('CREATE SCHEMA [ServiceCenter] AUTHORIZATION [dbo]');
GO

GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON SCHEMA :: ServiceCenter TO [MAINDB-2030\ccwebapp];  

GRANT INSERT ON dbo.einlesen_Artikel TO [MAINDB-2030\ccwebapp];  
GRANT INSERT ON dbo.einlesen_Kopfdaten TO [MAINDB-2030\ccwebapp];  
GRANT INSERT ON dbo.einlesen_Lieferadresse TO [MAINDB-2030\ccwebapp];  
GRANT INSERT ON dbo.einlesen_Rechnungsadresse TO [MAINDB-2030\ccwebapp];  
GRANT INSERT, UPDATE ON dbo.Bemerkung_Kunde TO [MAINDB-2030\ccwebapp];  

GO

-------------------------------------------------------------------------------

IF(OBJECT_ID('ServiceCenter.Einlesen_Kopfdaten_PKorder_id') IS NULL)
	CREATE SEQUENCE ServiceCenter.Einlesen_Kopfdaten_PKorder_id
	AS INT 
	START WITH 505000000
	INCREMENT BY 1
	NO CACHE; 
GO
	
IF(OBJECT_ID('ServiceCenter.CustomerId') IS NULL)
	CREATE SEQUENCE ServiceCenter.CustomerId
	AS INT 
	START WITH 1000
	INCREMENT BY 1
	NO CACHE; 
GO

-------------------------------------------------------------------------------

/*
IF OBJECT_ID('ServiceCenter.InvoiceItem', 'U') IS NOT NULL 
	DROP TABLE ServiceCenter.InvoiceItem; 

IF OBJECT_ID('ServiceCenter.InvoiceDeliveryAddress', 'U') IS NOT NULL 
	DROP TABLE ServiceCenter.InvoiceDeliveryAddress; 

IF OBJECT_ID('ServiceCenter.InvoiceBillingAddress', 'U') IS NOT NULL 
	DROP TABLE ServiceCenter.InvoiceBillingAddress; 

IF OBJECT_ID('ServiceCenter.Invoice', 'U') IS NOT NULL 
	DROP TABLE ServiceCenter.Invoice; 

IF OBJECT_ID('ServiceCenter.CustomerComment', 'U') IS NOT NULL 
	DROP TABLE ServiceCenter.CustomerComment; 

IF OBJECT_ID('ServiceCenter.DeliveryPaymentButtons', 'U') IS NOT NULL 
	DROP TABLE ServiceCenter.DeliveryPaymentButtons; 

GO
*/

-------------------------------------------------------------------------------
-- TABLE ServiceCenter.Invoice

PRINT 'CREATE TABLE ServiceCenter.Invoice...'

CREATE TABLE ServiceCenter.Invoice (
	InvoicePk INT NOT NULL IDENTITY(1,1),
	AffiliateId INT NOT NULL,
	CustomerId INT NOT NULL,
	InvoiceId INT NOT NULL,
    CustomerGroupId INT NOT NULL,
	CustomerGroupDiscount MONEY NOT NULL,
	DeliveryTypeId INT NULL,
	PaymentTypeId INT NULL,
    IBAN NVARCHAR(MAX) NULL,
    BIC NVARCHAR(MAX) NULL,
	AccountOwner NVARCHAR(128) NULL,
    CreatedUTC DATETIME NOT NULL,
    ChangedUTC DATETIME NULL,
    ChangedUserName NVARCHAR(128) NOT NULL
    );

ALTER TABLE ServiceCenter.Invoice
ADD CONSTRAINT PK_Invoice PRIMARY KEY NONCLUSTERED (InvoicePK);

CREATE UNIQUE CLUSTERED INDEX IX_Invoice_AffiliateId_CustomerId ON ServiceCenter.Invoice (AffiliateId, CustomerId, InvoiceId);

ALTER TABLE [ServiceCenter].[Invoice] ADD  CONSTRAINT [DF_Invoice_CreatedUTC] DEFAULT (getutcdate()) FOR [CreatedUTC]

GO

CREATE TRIGGER TrU_Invoice on ServiceCenter.Invoice FOR UPDATE AS            
BEGIN
    SET NOCOUNT ON;

    UPDATE i
    SET ChangedUTC = GetUtcDate()
    FROM ServiceCenter.Invoice i
    INNER JOIN deleted d ON i.InvoicePk = d.InvoicePk;
END

GO

-------------------------------------------------------------------------------
-- TABLE ServiceCenter.InvoiceItem 
PRINT 'CREATE TABLE ServiceCenter.InvoiceItem...'

CREATE TABLE ServiceCenter.InvoiceItem (
	InvoiceItemPk INT NOT NULL IDENTITY(1,1),
	InvoiceFk INT NOT NULL,
	PZN INT NOT NULL,
	Quantity  INT NOT NULL,
	VAT MONEY NOT NULL,
	ItemPrice MONEY NOT NULL,
    ItemSavings MONEY NOT NULL,
    CreatedUTC DATETIME NOT NULL,
    ChangedUTC DATETIME NULL,
    ChangedUserName NVARCHAR(128) NOT NULL
);

ALTER TABLE ServiceCenter.InvoiceItem
ADD CONSTRAINT PK_InvoiceItem PRIMARY KEY NONCLUSTERED (InvoiceItemPk);

CREATE CLUSTERED INDEX IX_InvoiceItem_InvoiceFk ON ServiceCenter.InvoiceItem (InvoiceFk);

ALTER TABLE ServiceCenter.InvoiceItem
ADD CONSTRAINT FK_InvoiceItem_InvoicePk
FOREIGN KEY (InvoiceFk) REFERENCES ServiceCenter.Invoice(InvoicePk);

ALTER TABLE [ServiceCenter].[InvoiceItem] ADD CONSTRAINT [DF_InvoiceItem_CreatedUTC] DEFAULT (getutcdate()) FOR [CreatedUTC]

GO

CREATE TRIGGER TrU_InvoiceItem on ServiceCenter.InvoiceItem FOR UPDATE AS            
BEGIN
    SET NOCOUNT ON;

    UPDATE ii
    SET ChangedUTC = GetUtcDate()
    FROM ServiceCenter.InvoiceItem ii
    INNER JOIN deleted d ON ii.InvoiceItemPk = d.InvoiceItemPk;
END

GO

-------------------------------------------------------------------------------
-- TABLE ServiceCenter.InvoiceDeliveryAddress 

PRINT 'CREATE TABLE ServiceCenter.InvoiceDeliveryAddress...'

USE [MAINDB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [ServiceCenter].[InvoiceDeliveryAddress](
	[InvoiceDeliveryAddressPk] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceFk] [int] NOT NULL,
	[Title] [nvarchar](64) NULL,
	[FirstName] [nvarchar](256) NULL,
	[LastName] [nvarchar](256) NULL,
	[CompanyName] [nvarchar](256) NULL,
	[Street] [nvarchar](256) NULL,
	[City] [nvarchar](256) NULL,
	[Zip] [nvarchar](16) NULL,
	[Country] [nvarchar](2) NULL,
	[AdditionalLine] [nvarchar](256) NULL,
	[CreatedUTC] [datetime] NOT NULL,
	[ChangedUTC] [datetime] NULL,
	[ChangedUserName] [nvarchar](128) NOT NULL
	);

ALTER TABLE ServiceCenter.InvoiceDeliveryAddress
ADD CONSTRAINT PK_InvoiceDeliveryAddress PRIMARY KEY NONCLUSTERED (InvoiceDeliveryAddressPK);

CREATE UNIQUE CLUSTERED INDEX IX_InvoiceDeliveryAddress_InvoiceFk ON ServiceCenter.InvoiceDeliveryAddress (InvoiceFk);

ALTER TABLE ServiceCenter.InvoiceDeliveryAddress
ADD CONSTRAINT FK_InvoiceDeliveryAddress_InvoicePk
FOREIGN KEY (InvoiceFk) REFERENCES ServiceCenter.Invoice(InvoicePk);

ALTER TABLE [ServiceCenter].[InvoiceDeliveryAddress] ADD  CONSTRAINT [DF_InvoiceDeliveryAddress_CreatedUTC] DEFAULT (getutcdate()) FOR [CreatedUTC]

GO

CREATE TRIGGER TrU_InvoiceDeliveryAddress ON ServiceCenter.InvoiceDeliveryAddress FOR UPDATE AS            
BEGIN
    SET NOCOUNT ON;

    UPDATE i
    SET ChangedUTC = GetUtcDate()
    FROM ServiceCenter.InvoiceDeliveryAddress i
    INNER JOIN deleted d ON i.InvoiceDeliveryAddressPK = d.InvoiceDeliveryAddressPK;
END

GO

-------------------------------------------------------------------------------
-- TABLE ServiceCenter.InvoiceBillingAddress 

PRINT 'CREATE TABLE ServiceCenter.InvoiceBillingAddress...'

USE [MAINDB]
GO

CREATE TABLE [ServiceCenter].[InvoiceBillingAddress](
	[InvoiceBillingAddressPk] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceFk] [int] NOT NULL,
	--
    [CustomerNumber] [nvarchar](16) NULL,
	--
	[Title] [nvarchar](64) NULL,
	[FirstName] [nvarchar](256) NULL,
	[LastName] [nvarchar](256) NULL,
	[CompanyName] [nvarchar](256) NULL,
	[Street] [nvarchar](256) NULL,
	[City] [nvarchar](256) NULL,
	[Zip] [nvarchar](16) NULL,
	[Country] [nvarchar](2) NULL,
	[AdditionalLine] [nvarchar](256) NULL,
	--
	[EmailAddress] [nvarchar](256) NULL,
	[TelephoneNumber] [nvarchar](128) NULL,
	[MobileNumber] [nvarchar](128) NULL,
	[FaxNumber] [nvarchar](128) NULL,
	[DoB] DATE NULL,
	--
	[CreatedUTC] [datetime] NOT NULL,
	[ChangedUTC] [datetime] NULL,
	[ChangedUserName] [nvarchar](128) NOT NULL
	);

ALTER TABLE ServiceCenter.InvoiceBillingAddress
ADD CONSTRAINT PK_InvoiceBillingAddress PRIMARY KEY NONCLUSTERED (InvoiceBillingAddressPK);

CREATE UNIQUE CLUSTERED INDEX IX_InvoiceBillingAddress_InvoiceFk ON ServiceCenter.InvoiceBillingAddress (InvoiceFk);

ALTER TABLE ServiceCenter.InvoiceBillingAddress
ADD CONSTRAINT FK_InvoiceBillingAddress_InvoicePk
FOREIGN KEY (InvoiceFk) REFERENCES ServiceCenter.Invoice(InvoicePk);

ALTER TABLE [ServiceCenter].[InvoiceBillingAddress] ADD  CONSTRAINT [DF_InvoiceBillingAddress_CreatedUTC] DEFAULT (getutcdate()) FOR [CreatedUTC]

GO

CREATE TRIGGER TrU_InvoiceBillingAddress ON ServiceCenter.InvoiceBillingAddress FOR UPDATE AS            
BEGIN
    SET NOCOUNT ON;

    UPDATE i
    SET ChangedUTC = GetUtcDate()
    FROM ServiceCenter.InvoiceBillingAddress i
    INNER JOIN deleted d ON i.InvoiceBillingAddressPK = d.InvoiceBillingAddressPK;
END

GO

-------------------------------------------------------------------------------
-- TABLE ServiceCenter.CustomerComment

PRINT 'CREATE TABLE ServiceCenter.CustomerComment...'

CREATE TABLE ServiceCenter.CustomerComment (
	CustomerCommentPk INT NOT NULL IDENTITY(1,1),
	CustomerId INT NOT NULL,
	CommentText NVARCHAR(MAX),
    CommentSent BIT NOT NULL,
    CreatedUTC DATETIME NOT NULL,
    ChangedUTC DATETIME NULL,
    ChangedUserName NVARCHAR(128) NOT NULL
);

ALTER TABLE ServiceCenter.CustomerComment
ADD CONSTRAINT PK_CustomerComment PRIMARY KEY CLUSTERED (CustomerCommentPk);

CREATE INDEX IX_CustomerComment_CustomerId ON ServiceCenter.CustomerComment (CustomerId);
CREATE INDEX IX_CustomerComment_CustomerId_CommentSent ON ServiceCenter.CustomerComment (CustomerId, CommentSent);

ALTER TABLE [ServiceCenter].[CustomerComment] ADD CONSTRAINT [DF_CustomerComment_CreatedUTC] DEFAULT (getutcdate()) FOR [CreatedUTC]
ALTER TABLE [ServiceCenter].[CustomerComment] ADD CONSTRAINT [DF_CustomerComment_CommentSent] DEFAULT (0) FOR [CommentSent]

GO

CREATE TRIGGER TrU_CustomerComment on ServiceCenter.CustomerComment FOR UPDATE AS            
BEGIN
    SET NOCOUNT ON;

    UPDATE ic
    SET ChangedUTC = GetUtcDate()
    FROM ServiceCenter.CustomerComment ic
    INNER JOIN deleted d ON ic.CustomerCommentPk = d.CustomerCommentPk;
END

GO

-------------------------------------------------------------------------------
-- TABLE ServiceCenter.DeliveryPaymentButton

PRINT 'CREATE TABLE ServiceCenter.DeliveryPaymentButton...'

CREATE TABLE ServiceCenter.DeliveryPaymentButton (
	DeliveryPaymentButtonPk INT NOT NULL IDENTITY(1, 1),
	AffiliateId INT NOT NULL,
	PZN INT NOT NULL,
	DisplayText NVARCHAR(255) NOT NULL,
	DisplayFromTotal MONEY NOT NULL,
	ItemPrice MONEY NOT NULL,
    FromDate DATE NOT NULL,
	UntilDate DATE NOT NULL
)

ALTER TABLE ServiceCenter.DeliveryPaymentButton
ADD CONSTRAINT PK_DeliveryPaymentButton PRIMARY KEY CLUSTERED (DeliveryPaymentButtonPk);

CREATE INDEX IX_DeliveryPaymentButton_AffiliateId ON ServiceCenter.DeliveryPaymentButton (AffiliateId);

CREATE INDEX IX_DeliveryPaymentButton_AffiliateId_DeliveryPaymentButtonPk ON ServiceCenter.DeliveryPaymentButton (AffiliateId, DeliveryPaymentButtonPk);

INSERT INTO ServiceCenter.DeliveryPaymentButton
SELECT Id, 9500500, 'Rabatt', 5.00, -5.00, '20000101', '20990101'
FROM dbo.Affiliate

INSERT INTO ServiceCenter.DeliveryPaymentButton
SELECT Id, 9500500, 'Rabatt', 10.00, -10.00, '20000101', '20990101'
FROM dbo.Affiliate

INSERT INTO ServiceCenter.DeliveryPaymentButton
SELECT Id, 9500500, 'Rabatt', 15.00, -15.00, '20000101', '20990101'
FROM dbo.Affiliate

INSERT INTO ServiceCenter.DeliveryPaymentButton
SELECT Id, 9, 'Rabatt %', 50.00, 10, '20000101', '20990101'
FROM dbo.Affiliate

INSERT INTO ServiceCenter.DeliveryPaymentButton
SELECT Id, 10, 'PriorityProcessing', 0.00, 0.99, '20000101', '20990101'
FROM dbo.Affiliate

INSERT INTO ServiceCenter.DeliveryPaymentButton
VALUES 
(1, 8030390, 'Schrittzähler', 50.0, 0.00, '20000101', '20990101'),
(2, 8030489, 'Schrittzähler', 65.0, 0.00, '20000101', '20990101'),
(4, 8030449, 'Schrittzähler', 70.0, 0.00, '20000101', '20990101'),
(5, 8030488, 'Schrittzähler', 60.0, 0.00, '20000101', '20990101')

INSERT INTO ServiceCenter.DeliveryPaymentButton
VALUES 
(1, 08030414, 'Schlüsselanhänger', 50.0, 0.00, '20000101', '20990101'),
(2, 08030425, 'Schlüsselanhänger', 50.0, 0.00, '20000101', '20990101')

GO

-------------------------------------------------------------------------------
-- TABLE ServiceCenter.CustomerMapping
PRINT 'TABLE ServiceCenter.CustomerMapping'

CREATE TABLE [ServiceCenter].[CustomerMapping] (
	CustomerId INT NOT NULL,
	KundenNr INT NOT NULL)

ALTER TABLE ServiceCenter.CustomerMapping
ADD CONSTRAINT PK_CustomerMapping PRIMARY KEY CLUSTERED (CustomerId);

CREATE UNIQUE NONCLUSTERED INDEX IX_CustomerMapping_KundenNr ON ServiceCenter.CustomerMapping (KundenNr);

ALTER TABLE ServiceCenter.CustomerComment
ADD CONSTRAINT FK_CustomerComment_CustomerId
FOREIGN KEY (CustomerId) REFERENCES ServiceCenter.CustomerMapping(CustomerId);

-------------------------------------------------------------------------------
-- TABLE ServiceCenter.PznAvailabilityTypeOverride
PRINT 'ServiceCenter.PznAvailabilityTypeOverride'

CREATE TABLE [ServiceCenter].[PznAvailabilityTypeOverride](
	[PznAvailabilityTypeOverridePk] [int] IDENTITY(1,1) NOT NULL,
	[PZN] [int] NOT NULL,
	[AvailabilityType]  [int] NOT NULL,
 CONSTRAINT [PK_PznAvailabilityTypeOverride] PRIMARY KEY CLUSTERED 
(
	[PznAvailabilityTypeOverridePk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


CREATE UNIQUE NONCLUSTERED INDEX [IX_PznAvailabilityTypeOverride_PZN] ON [ServiceCenter].[PznAvailabilityTypeOverride]
(
	[PZN]
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

INSERT INTO [ServiceCenter].[PznAvailabilityTypeOverride]
VALUES 
	(8019453, 2),
	(8030372, 2),
	(8030373, 2)

-------------------------------------------------------------------------------
-- TABLE ServiceCenter.CustomerWarning
PRINT 'TABLE ServiceCenter.CustomerWarning'
GO

CREATE TABLE [ServiceCenter].[CustomerWarning](
	[CustomerWarningPk] [int] IDENTITY(1,1) NOT NULL,
	[KundenNr] [int] NOT NULL,
	[WarningType] [nvarchar](16) NOT NULL
 CONSTRAINT [PK_CustomerWarning] PRIMARY KEY NONCLUSTERED 
(
	[CustomerWarningPk] ASC
) ON [PRIMARY]
)
GO

CREATE UNIQUE CLUSTERED INDEX [IX_CustomerWarning_KundenNr] ON [ServiceCenter].[CustomerWarning]
(
	[KundenNr] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


-------------------------------------------------------------------------------
-- TABLE ServiceCenter.InvoiceStatus
PRINT 'ServiceCenter.InvoiceStatus'

CREATE TABLE [ServiceCenter].[InvoiceStatus](
	[InvoiceStatusPk] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceFk] [int] NOT NULL,
	[ConfirmationEmailSent] DATETIME,
 CONSTRAINT [PK_InvoiceStatus] PRIMARY KEY NONCLUSTERED 
(
	[InvoiceStatusPk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [ServiceCenter].[InvoiceStatus]  WITH CHECK ADD  CONSTRAINT [FK_InvoiceStatus_InvoicePk] FOREIGN KEY([InvoiceFk])
REFERENCES [ServiceCenter].[Invoice] ([InvoicePk])
GO

ALTER TABLE [ServiceCenter].[InvoiceStatus] CHECK CONSTRAINT [FK_InvoiceStatus_InvoicePk]
GO


-------------------------------------------------------------------------------
-- PROC ServiceCenter.InvoiceInsertIntoEinlesen
PRINT 'PROC ServiceCenter.usp_InvoiceInsertIntoEinlesen'
GO

ALTER PROC [ServiceCenter].[usp_InvoiceInsertIntoEinlesen](@invoicePk INT, @changedUserName NVARCHAR(128))
AS
BEGIN
	SET NOCOUNT ON;

	IF(@invoicePk IS NULL
	OR @changedUserName IS NULL)
	BEGIN
		PRINT 'Parameters cannot be NULL';
		RETURN 1000;
	END

	-------------------------------------------------------------------------------

	DECLARE @orderId INT = NEXT VALUE FOR [ServiceCenter].[Einlesen_Kopfdaten_PKorder_id];
	
	-------------------------------------------------------------------------------
	--[dbo].[einlesen_Kopfdaten]

	SELECT InvoicePk bestellnr, AffiliateId affiliate, GETDATE() bestelldatum, DeliveryTypeId lieferart, Zw.Zahlungsweise zahlungsweise, IBAN, BIC, AccountOwner inh,  CASE Zw.Zahlungsweise WHEN 'Lastschrift' THEN RIGHT('00' + CAST(I.AffiliateId AS NVARCHAR(MAX)), 2) + '-' + RIGHT('00' + CAST(I.CustomerId AS NVARCHAR(MAX)), 10) + '-' + RIGHT('00' + CAST(I.InvoicePk AS NVARCHAR(MAX)), 5) + '-' + CAST(@orderId AS NVARCHAR(16)) END Mandatreferenz, SUBSTRING(IBAN, 5, 8) blz, SUBSTRING(IBAN, 13, LEN(IBAN) - 12) kontonummer, 'aufgenommen von ' + @changedUserName order_comment
	INTO #einlesen_Kopfdaten
	FROM [ServiceCenter].[Invoice] I
	JOIN [dbo].[Zahlungsweise] Zw ON I.PaymentTypeId = Zw.PKdvt_id
	WHERE InvoicePk = @invoicePk;

	IF(@@ROWCOUNT = 0)
	BEGIN
		DROP TABLE #einlesen_Kopfdaten;
		PRINT 'Invoice not found';
		RETURN 1001;
	END;

	-------------------------------------------------------------------------------

	UPDATE b
	SET Country = d.Country
	FROM [ServiceCenter].[InvoiceBillingAddress] b
	JOIN [ServiceCenter].[InvoiceDeliveryAddress] d ON d.InvoiceFk = d.InvoiceFk
	WHERE b.Invoicefk = @invoicePk;

	-------------------------------------------------------------------------------
	--[dbo].[einlesen_Lieferadresse]

	SELECT InvoiceDeliveryAddressPk Lieferadress_ID, Title anrede, FirstName vorname, LastName nachname, CompanyName firma, Street strasse, ZIP plz, City ort, Country land, 0 geprüft, AdditionalLine Zusatz
	INTO #einlesen_Lieferadresse
	FROM [ServiceCenter].[InvoiceDeliveryAddress] DA 
	WHERE InvoiceFk = @invoicePk;


	-------------------------------------------------------------------------------
	--[dbo].[einlesen_Rechnungsadresse]

	SELECT ISNULL(µ.Kundennr, -1) kunden_id, BA.Title anrede, BA.FirstName vorname, BA.LastName nachname, BA.CompanyName firma, BA.Street strasse, BA.Zip plz, BA.City ort, BA.Country land, BA.TelephoneNumber telefon, BA.MobileNumber mobil, BA.FaxNumber fax, BA.EmailAddress email, BA.DoB geburtsdatum, 0 geprüft, I.CustomerGroupId Kunden_Group, BA.AdditionalLine Zusatz
	INTO #einlesen_Rechnungsadresse
	FROM [ServiceCenter].[Invoice] I
	JOIN [ServiceCenter].[InvoiceBillingAddress] BA ON I.InvoicePk = BA.InvoiceFk
	LEFT JOIN [ServiceCenter].[CustomerMapping] µ ON I.CustomerId = µ.CustomerId
	WHERE I.InvoicePk = @invoicePk;

	-------------------------------------------------------------------------------
	--[dbo].[einlesen_Artikel]

	SELECT PZN pzn, Quantity menge, VAT mwst, ItemPrice einzelpreis
	INTO #einlesen_Artikel
	FROM [ServiceCenter].[InvoiceItem]
	WHERE InvoiceFk = @invoicePk;

	-------------------------------------------------------------------------------

	SELECT µ.KundenNr [Kunden_ID], 3 [User_ID], @changedUserName + ' hat eine Bestellung über die Server Center App abgegeben (' + CAST(@orderId AS NVARCHAR(MAX)) + ')' [Message]
	INTO #messages
	FROM [ServiceCenter].[Invoice] I
	JOIN [ServiceCenter].[CustomerMapping] µ ON I.CustomerId = µ.CustomerId
	WHERE InvoicePk = @invoicePk;

	-------------------------------------------------------------------------------

	BEGIN TRY  
		BEGIN TRANSACTION;

		INSERT INTO einlesen_Kopfdaten (PKorder_id, affiliate, bestellnr, bestelldatum, lieferart, zahlungsweise, IBAN, BIC, inh, Mandatreferenz, blz, kontonummer, order_comment)
		SELECT @orderId, affiliate, bestellnr, bestelldatum, lieferart, zahlungsweise, IBAN, BIC, inh, Mandatreferenz, blz, kontonummer, order_comment
		FROM #einlesen_Kopfdaten;

		INSERT INTO einlesen_Lieferadresse (PKorder_id, Lieferadress_ID, anrede, vorname, nachname, firma, strasse, plz, ort, land, geprüft, Zusatz)
		SELECT @orderId, Lieferadress_ID, anrede, vorname, nachname, firma, strasse, plz, ort, land, geprüft, Zusatz
		FROM #einlesen_Lieferadresse;

		INSERT INTO einlesen_Rechnungsadresse (PKorder_id, kunden_id, anrede, vorname, nachname, firma, strasse, plz, ort, land, telefon, mobil, fax, email, geburtsdatum, geprüft, Kunden_Group, Zusatz)
		SELECT @orderId, kunden_id, anrede, vorname, nachname, firma, strasse, plz, ort, land, telefon, mobil, fax, email, geburtsdatum, geprüft, Kunden_Group, Zusatz
		FROM #einlesen_Rechnungsadresse;

		INSERT INTO einlesen_Artikel(PKorder_id, pzn, menge, mwst, einzelpreis)
		SELECT @orderId, pzn, menge, mwst, einzelpreis
		FROM #einlesen_Artikel;

		INSERT INTO dbo.[messages]([Kunden_ID], [User_ID], [Message])
		SELECT [Kunden_ID], [User_ID], [Message]
		FROM #messages;

		UPDATE [ServiceCenter].[Invoice]
		SET InvoiceId = @orderId,
			ChangedUserName = @changedUserName
		WHERE InvoicePk = @invoicePk;

		COMMIT;
	END TRY  
	BEGIN CATCH  
		PRINT ERROR_MESSAGE();

		IF @@TRANCOUNT > 0  
			ROLLBACK TRANSACTION;  

		RETURN 1000;
	END CATCH 

	------------------------------------------------------------------------------

	DROP TABLE #einlesen_Kopfdaten;
	DROP TABLE #einlesen_Lieferadresse;
	DROP TABLE #einlesen_Rechnungsadresse;;
	DROP TABLE #einlesen_Artikel;

	------------------------------------------------------------------------------

	RETURN @orderId;
END

-------------------------------------------------------------------------------
-- PROC ServiceCenter.usp_InsertCommentInCustomer
PRINT 'PROC ServiceCenter.usp_InsertCommentInCustomer'
GO

USE [MAINDB]
GO

ALTER PROC [ServiceCenter].[usp_InsertCommentInCustomer](@customerId INT, @changedUserName NVARCHAR(128))
AS
BEGIN
	SET NOCOUNT ON;

	IF(@customerId IS NULL
	OR @changedUserName IS NULL)
	BEGIN
		PRINT 'Parameters cannot be NULL';
		RETURN ;
	END

	------------------------------------------------------------------------------

	DECLARE @returnValue INT = 0;

	DECLARE @kundenNr INT;
	DECLARE @messageText NVARCHAR(MAX) = NULL;
	
	SELECT @kundenNr = µ.KundenNr, @messageText = @changedUserName + ' sagte: ' + C.CommentText
	FROM [ServiceCenter].[CustomerComment] C
	JOIN [ServiceCenter].[CustomerMapping] µ ON C.CustomerId = µ.CustomerId	WHERE @customerId = @customerId
	  AND CommentSent = 0
	  AND CommentText IS NOT NULL

	IF( @kundenNr IS NOT NULL
	AND @messageText IS NOT NULL)
	BEGIN

		-----------------------
	
		BEGIN TRANSACTION;

		INSERT INTO [dbo].[messages]([Kunden_ID], [User_ID], [Message])
		SELECT @kundenNr, 3, @messageText;

		SET @returnValue = @@ROWCOUNT;

		UPDATE [ServiceCenter].[CustomerComment]
		SET CommentSent = @returnValue,
			ChangedUserName = @changedUserName
		WHERE @customerId = @customerId
		  AND CommentSent = 0
		  AND CommentText IS NOT NULL

		SET @returnValue = 1;

		COMMIT;

		-----------------------
	
	END	

	------------------------------------------------------------------------------

	RETURN @returnValue;
END
GO

-------------------------------------------------------------------------------
-- PROC ServiceCenter.usp_SendConfirmationEmail
PRINT 'PROC ServiceCenter.usp_SendConfirmationEmail'
GO

USE [MAINDB]
GO
/****** Object:  StoredProcedure [ServiceCenter].[usp_SendConfirmationEmail]    Script Date: 31.01.2019 15:17:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROC [ServiceCenter].[usp_SendConfirmationEmail]
AS
BEGIN 

	SET NOCOUNT ON;

	DECLARE @returnValue INT = 0;

	/** DEBUG **/
	SELECT TOP 1 I.InvoicePk, I.AffiliateId, B.FKord_cus_id KundenPk, B.PKbeleg_id BelegNr, BA.EmailAddress, LTRIM(RTRIM(ISNULL(BA.FirstName, '') + ' ' + ISNULL(BA.LastName, ''))) FullName
	/***********/
	INTO #temp
	FROM [ServiceCenter].[Invoice] I
	JOIN [ServiceCenter].[InvoiceBillingAddress] BA ON I.InvoicePk = BA.InvoiceFk
	JOIN [dbo].[einlesen_Kopfdaten] K ON I.InvoiceId = K.PKorder_id
	JOIN [dbo].[belege] B ON K.bestellnr = B.ord_orderID
	LEFT JOIN [ServiceCenter].[InvoiceStatus] S ON I.InvoicePk = S.InvoiceFk
	WHERE I.ChangedUTC > DATEADD(HOUR, -12, GETUTCDATE())
	  AND S.ConfirmationEmailSent IS NULL
	  AND BA.EmailAddress IS NOT NULL;

	DECLARE @invoicePk INT = 0;

	WHILE(1 = 1)
	BEGIN 

		SELECT @invoicePk = MIN(InvoicePk)
		FROM #temp
		WHERE InvoicePk > @invoicePk;

		IF(@invoicePk IS NULL)
			BREAK;

		DECLARE @affiliateId INT = 0;
		DECLARE @kundenId INT = 0;
		DECLARE @belegId INT = 0;

		DECLARE @recipientAddress NVARCHAR(MAX) = NULL;
		DECLARE @recipientName NVARCHAR(MAX) = NULL;

		SELECT @affiliateId = AffiliateId, @kundenId = KundenPk, @belegId = BelegNr, @recipientAddress = EmailAddress, @recipientName = FullName
		FROM #temp
		WHERE InvoicePk = @invoicePk;

		--  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- 	

		DECLARE @affiliateIdAlt INT;
		DECLARE @senderAddress NVARCHAR(MAX) = NULL;
		DECLARE @senderName NVARCHAR(MAX) = NULL;

		SELECT @affiliateIdAlt = affiliateALT, @senderName = a.Name, @senderAddress = 'service@' + RIGHT(Url, LEN(Url) - CHARINDEX('www.', Url) - 3)
		FROM dbo.SchrammOSAffiliateMapping µ
		JOIN dbo.Affiliate A ON µ.AffiliateNeu = A.ID
		WHERE µ.id = @affiliateId;

		DECLARE @subject NVARCHAR(MAX) = dbo.Vorlage_Header_ersetzt_final(@belegId, 'Bestellbestätigung Allgemein', @affiliateIdAlt);
		DECLARE @body NVARCHAR(MAX) = dbo.Vorlage_Body_ersetzt_final(@belegId, 'Bestellbestätigung Allgemein', @affiliateIdAlt);

		/** DEBUG **/
		SET @kundenId = 2721240;
		SET @recipientAddress = 'P.Spellman@xxx.de';
		/***********/

		--  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- 	
		INSERT INTO [MAINDB].[dbo].[Mail]
			([User_ID],
			[Email_Header],
			[Email_Body],
			[Kunden_ID],
			[Beleg_ID],
			[Absender_Adresse],
			[Empfänger_Adresse],
			[Absender_Name],
			[Empfänger_Name],
			[Organisation],
			[Zeitstempel]
			)
		 VALUES
			(3,
			@subject,
			@body,
			@kundenId,
			@belegId,
			@senderAddress,
			@recipientAddress,
			@senderName,
			@recipientName,
			@senderName,
			GETDATE()
			);

		--  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- --  -- 	

		UPDATE [ServiceCenter].[InvoiceStatus]
		SET ConfirmationEmailSent = GETDATE()
		WHERE InvoiceFk = @invoicePk;

		IF(@@ROWCOUNT = 0)
		BEGIN
			INSERT INTO [ServiceCenter].[InvoiceStatus](InvoiceFk, ConfirmationEmailSent)
			VALUES (@invoicePk, GETDATE());
		END

		SET @returnValue += 1;

	END;

	DROP TABLE #temp;

	RETURN(@returnValue);

END

-------------------------------------------------------------------------------
-- JOB Remove Expired Invoice Item
PRINT 'JOB Remove Expired Invoice Item'
GO

USE [msdb]
GO

/****** Object:  Job [Service Center App Cleanup]    Script Date: 28.01.2019 12:30:36 ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [MAINDB Service Center]    Script Date: 28.01.2019 12:30:36 ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'MAINDB Service Center' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'MAINDB Service Center'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'Service Center App Cleanup', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=2, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Es ist keine Beschreibung verfügbar.', 
		@category_name=N'MAINDB Service Center', 
		@owner_login_name=N'MAINDB-2030\pspellman-admin', 
		@notify_email_operator_name=N'Rundmail', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Delete Expired Invoice Items]    Script Date: 28.01.2019 12:30:36 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Delete Expired Invoice Items', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=3, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'DELETE ii
FROM [ServiceCenter].[Invoice] i
JOIN [ServiceCenter].[InvoiceItem] ii ON i.InvoicePk = ii.InvoiceFk
WHERE i.InvoiceId = 0
  AND DATEDIFF(DAY, ISNULL(ii.ChangedUTC, ii.CreatedUTC), GETDATE()) >= 1', 
		@database_name=N'MAINDB', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Delete Abandoned New Customers]    Script Date: 28.01.2019 12:30:36 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Delete Abandoned New Customers', 
		@step_id=2, 
		@cmdexec_success_code=0, 
		@on_success_action=3, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'BEGIN TRANSACTION

DELETE B
FROM [ServiceCenter].[Invoice] I
JOIN [ServiceCenter].[InvoiceBillingAddress] B ON I.InvoicePk = B.InvoiceFk
WHERE I.AffiliateId = 0

DELETE D
FROM [ServiceCenter].[Invoice] I
JOIN [ServiceCenter].[InvoiceDeliveryAddress] D ON I.InvoicePk = D.InvoiceFk
WHERE I.AffiliateId = 0

DELETE II
FROM [ServiceCenter].[Invoice] I
JOIN [ServiceCenter].[InvoiceItem] II ON I.InvoicePk = II.InvoiceFk
WHERE I.AffiliateId = 0

DELETE I
FROM [ServiceCenter].[Invoice] I
WHERE I.AffiliateId = 0

COMMIT', 
		@database_name=N'MAINDB', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Delete Abandoned Customers]    Script Date: 28.01.2019 12:30:36 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Delete Abandoned Customers', 
		@step_id=3, 
		@cmdexec_success_code=0, 
		@on_success_action=3, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'BEGIN TRANSACTION

DELETE B
FROM [ServiceCenter].[Invoice] I
LEFT JOIN [ServiceCenter].[InvoiceItem] II ON I.InvoicePk = II.InvoiceFk
JOIN [ServiceCenter].[InvoiceBillingAddress] B ON I.InvoicePk = B.InvoiceFk
WHERE I.InvoiceId = 0
  AND DATEDIFF(DAY, ISNULL(I.ChangedUTC, I.CreatedUTC), GETDATE()) >= 1
  AND II.InvoiceFk IS NULL

DELETE D
FROM [ServiceCenter].[Invoice] I
LEFT JOIN [ServiceCenter].[InvoiceItem] II ON I.InvoicePk = II.InvoiceFk
JOIN [ServiceCenter].[InvoiceDeliveryAddress] D ON I.InvoicePk = D.InvoiceFk
WHERE I.InvoiceId = 0
  AND DATEDIFF(DAY, ISNULL(I.ChangedUTC, I.CreatedUTC), GETDATE()) >= 1
  AND II.InvoiceFk IS NULL

DELETE I
FROM [ServiceCenter].[Invoice] I
LEFT JOIN [ServiceCenter].[InvoiceItem] II ON I.InvoicePk = II.InvoiceFk
WHERE I.InvoiceId = 0
  AND DATEDIFF(DAY, ISNULL(I.ChangedUTC, I.CreatedUTC), GETDATE()) >= 1
  AND II.InvoiceFk IS NULL

COMMIT

', 
		@database_name=N'MAINDB', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Delete Expired Comments]    Script Date: 28.01.2019 12:30:36 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Delete Expired Comments', 
		@step_id=4, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'DELETE CC
FROM [ServiceCenter].[CustomerComment] CC
WHERE DATEDIFF(DAY, ISNULL(CC.ChangedUTC, CC.CreatedUTC), GETDATE()) >= 7
  AND CommentSent = 0
', 
		@database_name=N'MAINDB', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Nightly 05:03', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20190109, 
		@active_end_date=99991231, 
		@active_start_time=50300, 
		@active_end_time=235959, 
		@schedule_uid=N'996c741c-8dea-42fe-bc3a-660647cad5de'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO

-------------------------------------------------------------------------------
-- JOB Server Center App Send Confirmation Emails
PRINT 'JOB Server Center App Send Confirmation Emails'
GO


USE [msdb]
GO

/****** Object:  Job [Server Center App Send Confirmation Emails]    Script Date: 31.01.2019 15:46:56 ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [MAINDB Service Center]    Script Date: 31.01.2019 15:46:56 ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'MAINDB Service Center' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'MAINDB Service Center'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'Server Center App Send Confirmation Emails', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=2, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Es ist keine Beschreibung verfügbar.', 
		@category_name=N'MAINDB Service Center', 
		@owner_login_name=N'MAINDB-2030\pspellman-admin', 
		@notify_email_operator_name=N'Rundmail', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [[ServiceCenter].[usp_SendConfirmationEmail]]    Script Date: 31.01.2019 15:46:56 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'[ServiceCenter].[usp_SendConfirmationEmail]', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXEC [ServiceCenter].[usp_SendConfirmationEmail]', 
		@database_name=N'MAINDB', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Every 10 Minutes', 
		@enabled=0, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=4, 
		@freq_subday_interval=10, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20190131, 
		@active_end_date=99991231, 
		@active_start_time=60600, 
		@active_end_time=220600, 
		@schedule_uid=N'27e22a98-6696-4c12-97a3-0e35c7a313b4'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO

-------------------------------------------------------------------------------
