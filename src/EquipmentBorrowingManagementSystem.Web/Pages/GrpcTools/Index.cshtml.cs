using System.ComponentModel.DataAnnotations;
using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Web.Pages.GrpcTools;

public class IndexModel : EbmsPageModel
{
    private readonly GrpcNotificationService _grpc;

    public IndexModel(GrpcNotificationService grpc)
    {
        _grpc = grpc;
    }

    [BindProperty]
    public GrpcSendInput Input { get; set; } = new();

    public GrpcSendResult? Result { get; set; }
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (EnsureStaffOrAdmin() is IActionResult redirect)
        {
            return redirect;
        }

        if (CurrentAuth is not null)
        {
            Input.UserEmail = CurrentAuth.Email;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (EnsureStaffOrAdmin() is IActionResult redirect)
        {
            return redirect;
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            Result = await _grpc.SendAsync(
                Input.UserId,
                Input.UserEmail.Trim(),
                Input.Title.Trim(),
                Input.Message.Trim(),
                Input.NotificationType.Trim(),
                cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        return Page();
    }

    public class GrpcSendInput
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; } = 1;

        [Required, EmailAddress]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = "EBMS Notification";

        [Required]
        public string Message { get; set; } = "Test message from Web gRPC Tools.";

        [Required]
        public string NotificationType { get; set; } = "Manual";
    }
}
