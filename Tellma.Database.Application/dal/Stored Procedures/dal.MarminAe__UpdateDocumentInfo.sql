CREATE PROCEDURE [dal].[MarminAe__UpdateDocumentInfo]
	@Id INT,
	@MarminAeState INT,
	@MarminAeDocumentId NVARCHAR (50) = NULL,
	@MarminAeDocumentNumber NVARCHAR (50) = NULL,
	@MarminAeResult NVARCHAR (MAX) = NULL,
	@MarminAeLastEventAt DATETIMEOFFSET(7) = NULL,
	@RowsAffected INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Records the outcome of a submission, or what the vendor says about a document it already
	 * holds. Mirrors [dal].[Zatca__UpdateDocumentInfo].
	 *
	 * Runs in its own transaction AFTER the close has committed and after the HTTP call has
	 * returned, so a failure here cannot unwind accounting that is already correct. A failure
	 * leaves the document at 2 (SentAwaitingOutcome), which "Refresh e-invoice status" resolves
	 * by asking the vendor for the document's number.
	 *
	 * Conditional on the document still being closed. Reopening is refused from state 1 upward in
	 * Production, but Sandbox allows it in any state and clears the Marmin columns when it does,
	 * so an outcome arriving for a document reopened while the request was in flight would
	 * otherwise stamp "Submitted" and a vendor id onto an open document. @RowsAffected = 0 tells
	 * the caller that happened, so it can say so.
	 */

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = @MarminAeState,
		[MarminAeDocumentId] = ISNULL(@MarminAeDocumentId, [MarminAeDocumentId]),
		[MarminAeDocumentNumber] = ISNULL(@MarminAeDocumentNumber, [MarminAeDocumentNumber]),
		[MarminAeResult] = @MarminAeResult,
		[MarminAeLastEventAt] = ISNULL(@MarminAeLastEventAt, [MarminAeLastEventAt])
	WHERE [Id] = @Id
	AND [State] = 1;

	SET @RowsAffected = @@ROWCOUNT;
END;
GO
