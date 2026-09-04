CREATE PROCEDURE [dal].[MarminAe__UpdateDocumentInfo]
	@Id INT,
	@MarminAeState INT,
	@MarminAeDocumentId NVARCHAR (50) = NULL,
	@MarminAeDocumentNumber NVARCHAR (50) = NULL,
	@MarminAeResult NVARCHAR (MAX) = NULL,
	@MarminAeLastEventAt DATETIMEOFFSET(7) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Records the outcome of a submission. Mirrors [dal].[Zatca__UpdateDocumentInfo].
	 *
	 * Runs in its own transaction AFTER the close has committed and after the HTTP call has
	 * returned, so a failure here leaves the document at state 0 (Submitting) rather than
	 * rolling back accounting that is already correct. The "Resubmit to Marmin" action recovers
	 * from that, and checks with the vendor first so it cannot transmit a duplicate.
	 */

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = @MarminAeState,
		[MarminAeDocumentId] = ISNULL(@MarminAeDocumentId, [MarminAeDocumentId]),
		[MarminAeDocumentNumber] = ISNULL(@MarminAeDocumentNumber, [MarminAeDocumentNumber]),
		[MarminAeResult] = @MarminAeResult,
		[MarminAeLastEventAt] = ISNULL(@MarminAeLastEventAt, [MarminAeLastEventAt])
	WHERE [Id] = @Id;
END;
GO
