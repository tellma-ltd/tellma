CREATE PROCEDURE [dal].[MarminAe__ResetOnOpen]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * In Sandbox only, forgets everything Marmin knew about documents that are being reopened.
	 *
	 * bll.Documents_Validate__Open refuses to reopen a submitted document in Production, but
	 * deliberately allows it in Sandbox so that the integration can be exercised repeatedly.
	 * Without this, a reopened and edited sandbox document would keep its vendor id and its
	 * Submitted or Delivered state: a later refresh would report the vendor's verdict on the old
	 * content, the document would still count as a credit-note original, and a re-close would
	 * never submit the new content because dal.MarminAe__GetInvoices skips those states.
	 *
	 * After this a re-close submits afresh. Note that the vendor enforces unique document numbers
	 * within an organisation, so re-sending a number it already holds is refused as a duplicate;
	 * that is the expected sandbox outcome, and the reason the README says to test each scenario
	 * on a fresh document.
	 *
	 * Called from DocumentsService inside the Open transaction, only for Marmin definitions.
	 */

	IF (SELECT TOP 1 [MarminAeEnvironment] FROM [dbo].[Settings]) <> N'Sandbox'
		RETURN;

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = NULL,
		[MarminAeDocumentId] = NULL,
		[MarminAeDocumentNumber] = NULL,
		[MarminAeResult] = NULL,
		[MarminAeLastEventId] = NULL,
		[MarminAeLastEventAt] = NULL
	WHERE [Id] IN (SELECT [Id] FROM @Ids)
	AND [MarminAeState] IS NOT NULL;
END;
GO
