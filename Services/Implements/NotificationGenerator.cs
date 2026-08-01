using Drivious.Data;
using Drivious.Enums;
using Drivious.Models;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Drivious.Services.Implements
{
    public class NotificationGenerator : INotificationGenerator
    {
        private readonly AppDbContext _context;
        private readonly NotificationSettings _settings;
        private readonly ILogger<NotificationGenerator> _logger;

        public NotificationGenerator(
            AppDbContext context,
            IOptions<NotificationSettings> settings,
            ILogger<NotificationGenerator> logger)
        {
            _context = context;
            _settings = settings.Value;
            _logger = logger;
        }

        /// <summary>
        /// One notification the scan wants to exist. The key is what makes a scan
        /// repeatable: it names the reason, the row and the date being warned about,
        /// so running twice on the same day inserts nothing the second time.
        /// </summary>
        private sealed record Candidate(
            string Key,
            string Title,
            string Message,
            NotificationType Type,
            DateTime Date);

        public async Task<int> GenerateAsync(CancellationToken cancellationToken = default)
        {
            var today = DateTime.UtcNow.Date;
            var from = today.AddDays(-_settings.LeadDays);
            var to = today.AddDays(_settings.LeadDays);

            var candidates = new List<Candidate>();

            candidates.AddRange(await BuildInsuranceAsync(today, from, to, cancellationToken));
            candidates.AddRange(await BuildLicenceAsync(today, from, to, cancellationToken));
            candidates.AddRange(await BuildMaintenanceAsync(today, from, to, cancellationToken));
            candidates.AddRange(await BuildDocumentAsync(today, from, to, cancellationToken));

            if (candidates.Count == 0)
            {
                return 0;
            }

            var keys = candidates.Select(x => x.Key).ToList();

            // One round trip for the whole batch; inserting and catching the unique
            // index violation instead would abort the rest of the batch with it.
            var existing = await _context.Notifications
                .Where(x => x.ReferenceKey != null && keys.Contains(x.ReferenceKey))
                .Select(x => x.ReferenceKey!)
                .ToListAsync(cancellationToken);

            var fresh = candidates
                .Where(x => !existing.Contains(x.Key))
                .Select(x => new Notification
                {
                    Title = x.Title,
                    Message = x.Message,
                    Type = x.Type,
                    NotificationDate = x.Date,
                    ReferenceKey = x.Key,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            if (fresh.Count == 0)
            {
                return 0;
            }

            await _context.Notifications.AddRangeAsync(fresh, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Notification scan created {Count} notification(s).", fresh.Count);

            return fresh.Count;
        }

        private async Task<List<Candidate>> BuildInsuranceAsync(
            DateTime today, DateTime from, DateTime to, CancellationToken cancellationToken)
        {
            var rows = await _context.Insurances
                .Where(x => !x.IsDeleted && x.EndDate >= from && x.EndDate <= to)
                .Select(x => new
                {
                    x.Id,
                    x.EndDate,
                    x.CompanyName,
                    x.PolicyNumber,
                    Plate = x.Vehicle.PlateNumber
                })
                .ToListAsync(cancellationToken);

            return rows.Select(x =>
            {
                var (state, type, phrase) = Describe(today, x.EndDate);

                return new Candidate(
                    $"insurance-{state}:{x.Id}:{x.EndDate:yyyyMMdd}",
                    TitleFor("Insurance", state),
                    $"Policy {x.PolicyNumber} ({x.CompanyName}) for vehicle {x.Plate} " +
                    $"{phrase} ({x.EndDate:dd.MM.yyyy}).",
                    type,
                    x.EndDate);
            }).ToList();
        }

        private async Task<List<Candidate>> BuildLicenceAsync(
            DateTime today, DateTime from, DateTime to, CancellationToken cancellationToken)
        {
            var rows = await _context.Drivers
                .Where(x => !x.IsDeleted
                         && x.LicenseExpireDate >= from
                         && x.LicenseExpireDate <= to)
                .Select(x => new
                {
                    x.Id,
                    x.LicenseExpireDate,
                    x.FirstName,
                    x.LastName,
                    x.DriverLicenseNumber
                })
                .ToListAsync(cancellationToken);

            return rows.Select(x =>
            {
                var (state, type, phrase) = Describe(today, x.LicenseExpireDate);

                return new Candidate(
                    $"driver-licence-{state}:{x.Id}:{x.LicenseExpireDate:yyyyMMdd}",
                    TitleFor("Driving licence", state),
                    $"Licence {x.DriverLicenseNumber} of {x.FirstName} {x.LastName} " +
                    $"{phrase} ({x.LicenseExpireDate:dd.MM.yyyy}).",
                    type,
                    x.LicenseExpireDate);
            }).ToList();
        }

        private async Task<List<Candidate>> BuildMaintenanceAsync(
            DateTime today, DateTime from, DateTime to, CancellationToken cancellationToken)
        {
            var rows = await _context.Maintenances
                .Where(x => !x.IsDeleted
                         && x.NextMaintenanceDate != null
                         && x.NextMaintenanceDate >= from
                         && x.NextMaintenanceDate <= to)
                .Select(x => new
                {
                    x.Id,
                    Due = x.NextMaintenanceDate!.Value,
                    x.ServiceType,
                    Plate = x.Vehicle.PlateNumber
                })
                .ToListAsync(cancellationToken);

            return rows.Select(x =>
            {
                var overdue = x.Due.Date < today;

                var state = overdue ? "overdue" : "due";
                var type = overdue ? NotificationType.Error : NotificationType.Warning;

                var phrase = overdue
                    ? $"was due {(today - x.Due.Date).Days} day(s) ago"
                    : $"is due in {(x.Due.Date - today).Days} day(s)";

                return new Candidate(
                    $"maintenance-{state}:{x.Id}:{x.Due:yyyyMMdd}",
                    overdue ? "Service overdue" : "Service due",
                    $"{x.ServiceType} for vehicle {x.Plate} {phrase} ({x.Due:dd.MM.yyyy}).",
                    type,
                    x.Due);
            }).ToList();
        }

        private async Task<List<Candidate>> BuildDocumentAsync(
            DateTime today, DateTime from, DateTime to, CancellationToken cancellationToken)
        {
            var rows = await _context.VehicleDocuments
                .Where(x => !x.IsDeleted
                         && x.ExpiryDate != null
                         && x.ExpiryDate >= from
                         && x.ExpiryDate <= to)
                .Select(x => new
                {
                    x.Id,
                    Expiry = x.ExpiryDate!.Value,
                    x.Title,
                    x.DocumentType,
                    Plate = x.Vehicle.PlateNumber
                })
                .ToListAsync(cancellationToken);

            return rows.Select(x =>
            {
                var (state, type, phrase) = Describe(today, x.Expiry);

                return new Candidate(
                    $"document-{state}:{x.Id}:{x.Expiry:yyyyMMdd}",
                    TitleFor("Document", state),
                    $"{x.DocumentType} \"{x.Title}\" for vehicle {x.Plate} " +
                    $"{phrase} ({x.Expiry:dd.MM.yyyy}).",
                    type,
                    x.Expiry);
            }).ToList();
        }

        /// <summary>
        /// A date that has already passed is an error the fleet has to act on now; one
        /// still ahead is a warning. The state is part of the key, so a record produces
        /// one warning while it is approaching and one error once it has passed.
        /// </summary>
        private static (string State, NotificationType Type, string Phrase) Describe(
            DateTime today, DateTime date)
        {
            if (date.Date < today)
            {
                return ("expired", NotificationType.Error, $"expired {(today - date.Date).Days} day(s) ago");
            }

            return ("expiring", NotificationType.Warning, $"expires in {(date.Date - today).Days} day(s)");
        }

        /// <summary>Title case label for the state, used as the notification title.</summary>
        private static string TitleFor(string subject, string state) =>
            state == "expired" ? $"{subject} expired" : $"{subject} expiring";
    }
}
