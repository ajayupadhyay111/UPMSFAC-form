using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UPMSF.Server.Data;
using UPMSF.Server.Services;
using UPMSF.Shared;

namespace UPMSF.Server.Controllers;

public partial class ApplicationsController
{
    /// <summary>Document slots that must be present before leaving the Documents step.</summary>
    private static readonly DocumentType[] RequiredDocuments = Enum.GetValues<DocumentType>();

    // ---------------------------------------------------------------- Step 4: upload a document
    [HttpPost("{id:int}/documents/{docType}")]
    [RequestSizeLimit(DocumentRules.MaxFileSizeBytes + 1024)]
    public async Task<ActionResult<DocumentDto>> UploadDocument(
        int id, DocumentType docType, IFormFile file, [FromServices] FileStorageService storage, CancellationToken ct)
    {
        var app = await Owned(id).Include(a => a.Documents).FirstOrDefaultAsync(ct);
        if (app is null) return NotFound();
        if (app.CurrentStep != ApplicationStep.Documents) return Locked(app);
        if (file is null || file.Length == 0) return BadRequest(new { message = "No file uploaded." });
        if (file.Length > DocumentRules.MaxFileSizeBytes)
            return BadRequest(new { message = "File exceeds the 25 MB limit." });

        var expected = DocumentRules.AllowedExtension(docType);
        var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        if (ext != expected)
            return BadRequest(new { message = $"This slot requires a .{expected} file." });

        var path = await storage.SaveAsync(app.ApplicationNumber, docType.ToString(), expected, file.OpenReadStream(), ct);

        var existing = app.Documents.FirstOrDefault(d => d.DocumentType == docType);
        if (existing is not null)
        {
            if (existing.StoredPath != path) storage.Delete(existing.StoredPath);
            existing.OriginalFileName = file.FileName;
            existing.StoredPath = path;
            existing.ContentType = file.ContentType;
            existing.SizeBytes = file.Length;
            existing.UploadedAtUtc = DateTime.UtcNow;
        }
        else
        {
            app.Documents.Add(new ApplicationDocument
            {
                ApplicationId = app.Id,
                DocumentType = docType,
                OriginalFileName = file.FileName,
                StoredPath = path,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                UploadedAtUtc = DateTime.UtcNow
            });
        }
        app.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(new DocumentDto
        {
            DocumentType = docType,
            OriginalFileName = file.FileName,
            SizeBytes = file.Length,
            UploadedAtUtc = DateTime.UtcNow
        });
    }

    // ---------------------------------------------------------------- view/download a document
    [HttpGet("{id:int}/documents/{docType}/content")]
    public async Task<IActionResult> GetDocumentContent(int id, DocumentType docType)
    {
        var app = await Owned(id).Include(a => a.Documents).FirstOrDefaultAsync();
        if (app is null) return NotFound();
        var doc = app.Documents.FirstOrDefault(d => d.DocumentType == docType);
        if (doc is null || !System.IO.File.Exists(doc.StoredPath)) return NotFound();

        var contentType = string.IsNullOrEmpty(doc.ContentType) ? "application/octet-stream" : doc.ContentType;
        var stream = new FileStream(doc.StoredPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        // PDFs open inline (in a browser tab); other types download.
        return File(stream, contentType, fileDownloadName: contentType == "application/pdf" ? null : doc.OriginalFileName);
    }

    // ---------------------------------------------------------------- Step 4 -> proceed
    [HttpPost("{id:int}/documents/proceed")]
    public async Task<IActionResult> ProceedDocuments(int id)
    {
        var app = await Owned(id).Include(a => a.Documents).Include(a => a.Payment).FirstOrDefaultAsync();
        if (app is null) return NotFound();
        if (app.CurrentStep != ApplicationStep.Documents) return Locked(app);

        var have = app.Documents.Select(d => d.DocumentType).ToHashSet();
        var missing = RequiredDocuments.Where(r => !have.Contains(r)).Select(r => r.ToString()).ToList();
        if (missing.Count > 0)
            return BadRequest(new { message = "Please upload all required documents.", missing });

        // create the payment record (Step 5)
        app.Payment ??= new Payment
        {
            ApplicationId = app.Id,
            BaseFee = 400000m,
            GstPercent = 18m,
            GstAmount = Math.Round(400000m * 0.18m, 2),
            TotalAmount = 400000m + Math.Round(400000m * 0.18m, 2),
            Status = PaymentStatus.Pending
        };
        app.CurrentStep = ApplicationStep.Payment;
        app.Status = ApplicationStatus.PaymentPending;
        app.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---------------------------------------------------------------- Step 5: mock payment
    [HttpPost("{id:int}/payment")]
    public async Task<ActionResult<PaymentDto>> Pay(int id)
    {
        var app = await Owned(id).Include(a => a.Payment).FirstOrDefaultAsync();
        if (app is null) return NotFound();
        if (app.CurrentStep != ApplicationStep.Payment) return Locked(app);
        if (app.Payment is null) return BadRequest(new { message = "Complete the documents step first." });
        if (app.Payment.Status == PaymentStatus.Success)
            return BadRequest(new { message = "Payment already completed." });

        // --- MOCK gateway: always succeeds. Replace with real gateway later. ---
        app.Payment.Status = PaymentStatus.Success;
        app.Payment.TransactionId = "MOCK" + app.ApplicationNumber.Replace("-", "") + DateTime.UtcNow.ToString("HHmmss");
        app.Payment.PaidAtUtc = DateTime.UtcNow;
        app.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new PaymentDto
        {
            BaseFee = app.Payment.BaseFee,
            GstPercent = app.Payment.GstPercent,
            Status = app.Payment.Status,
            TransactionId = app.Payment.TransactionId,
            PaidAtUtc = app.Payment.PaidAtUtc
        });
    }

    // ---------------------------------------------------------------- Final submission
    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        var app = await Owned(id).Include(a => a.Payment).FirstOrDefaultAsync();
        if (app is null) return NotFound();
        if (app.Status == ApplicationStatus.Submitted) return NoContent();
        if (app.CurrentStep != ApplicationStep.Payment)
            return BadRequest(new { message = "Complete the previous steps before final submission." });
        // Short (dashboard) applications carry no fee; full applications require successful payment.
        if (!app.IsShortForm && app.Payment?.Status != PaymentStatus.Success)
            return BadRequest(new { message = "Payment must be completed before final submission." });

        app.Status = ApplicationStatus.Submitted;
        app.CurrentStep = ApplicationStep.Submitted;
        app.SubmittedAtUtc = DateTime.UtcNow;
        app.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
