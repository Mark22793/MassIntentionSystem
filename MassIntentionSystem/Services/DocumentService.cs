using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MassIntentionSystem.Data;
using MassIntentionSystem.Models;

namespace MassIntentionSystem.Services
{
    public class DocumentService
    {
        private readonly ApplicationDbContext _context;

        public DocumentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Single Mass Time Doc Generator
        public async Task<byte[]> GenerateMassIntentionDocAsync(DateTime massDate, TimeSpan? massTime = null, int priestId = 0)
        {
            var selectedTimes = new Dictionary<TimeSpan, int>();
            if (massTime.HasValue)
            {
                selectedTimes.Add(massTime.Value, priestId);
            }

            return await GenerateMultiTimeMassDocAsync(massDate, selectedTimes);
        }

        // Main Multi-Time Document Generator
        public async Task<byte[]> GenerateMultiTimeMassDocAsync(DateTime massDate, Dictionary<TimeSpan, int>? timePriestMap = null)
        {
            // 1. Kunin ang lahat ng Mass Intentions sa petsang ito
            var query = _context.MassIntentions
                .Where(m => m.MassDate.Date == massDate.Date)
                .AsQueryable();

            // Filter ayon sa napiling time slot kung may pumasok mula sa controller
            if (timePriestMap != null && timePriestMap.Any())
            {
                var targetTimes = timePriestMap.Keys.ToList();
                query = query.Where(m => targetTimes.Contains(m.MassTime));
            }

            var intentions = await query.ToListAsync();

            // Default Mass Schedule list para hindi mabakante ang dokumento kahit walang records
            var defaultTimes = new List<TimeSpan>
            {
                new TimeSpan(6, 0, 0),
                new TimeSpan(7, 30, 0),
                new TimeSpan(9, 0, 0),
                new TimeSpan(10, 30, 0),
                new TimeSpan(12, 0, 0),
                new TimeSpan(16, 0, 0),
                new TimeSpan(17, 30, 0),
                new TimeSpan(19, 0, 0)
            };

            // I-group ang mga pumasok na records ayon sa MassTime
            var groupedByTime = intentions
                .GroupBy(m => m.MassTime)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Kung may pinalabas na specific filter ang user pero walang records, gagamitin ang target times
            var timesToProcess = (timePriestMap != null && timePriestMap.Any())
                ? timePriestMap.Keys.OrderBy(t => t).ToList()
                : (groupedByTime.Any() ? groupedByTime.Keys.OrderBy(t => t).ToList() : defaultTimes);

            StringBuilder html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<style>");

            // LANDSCAPE + LONG PAPER SIZE (13in x 8.5in / Folio Landscape)
            html.AppendLine("   @page Section1 {");
            html.AppendLine("       size: 13in 8.5in;");
            html.AppendLine("       mso-page-orientation: landscape;");
            html.AppendLine("       margin: 0.4in 0.4in 0.4in 0.4in;");
            html.AppendLine("   }");
            html.AppendLine("   div.Section1 { page: Section1; }");

            // BASE STYLES
            html.AppendLine("   body { font-family: 'Arial', sans-serif; font-size: 9.5pt; color: #000; }");
            html.AppendLine("   .header { text-align: center; margin-bottom: 15px; font-weight: bold; }");
            html.AppendLine("   .header .title { font-size: 13pt; text-transform: uppercase; }");
            html.AppendLine("   .header .subtitle { font-size: 10.5pt; color: #333; }");
            html.AppendLine("   .header .date-title { font-size: 11.5pt; margin-top: 4px; text-decoration: underline; }");

            // 2-COLUMN LAYOUT FOR LANDSCAPE
            html.AppendLine("   .columns-container { column-count: 2; column-gap: 25px; width: 100%; }");
            html.AppendLine("   .mass-block { break-inside: avoid; page-break-inside: avoid; margin-bottom: 12px; border: 1px solid #777; padding: 8px; background-color: #ffffff; }");

            // STYLES PARA SA MASS HEADER BAR
            html.AppendLine("   .mass-header-table { width: 100%; border-bottom: 2px solid #000; margin-bottom: 6px; padding-bottom: 2px; }");
            html.AppendLine("   .mass-header-priest { font-weight: bold; font-size: 10pt; text-transform: uppercase; text-align: left; }");
            html.AppendLine("   .mass-header-time { font-weight: bold; font-size: 10.5pt; text-transform: uppercase; text-align: right; }");

            html.AppendLine("   .category-title { font-weight: bold; font-size: 9.5pt; margin-top: 5px; margin-bottom: 2px; color: #111; text-transform: uppercase; }");
            html.AppendLine("   .names-list { margin: 0; padding-left: 14px; font-size: 9pt; }");
            html.AppendLine("   .names-list li { margin-bottom: 1px; }");
            html.AppendLine("   .no-names { font-style: italic; color: #666; font-size: 8.5pt; margin-left: 14px; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body class='Section1'>");
            html.AppendLine("<div class='Section1'>");

            // HEADER SECTION
            html.AppendLine("<div class='header'>");
            html.AppendLine("   <div class='title'>NATIONAL SHRINE OF OUR LADY OF LA NAVAL DE MANILA</div>");
            html.AppendLine("   <div class='subtitle'>SANTO DOMINGO CHURCH, QUEZON CITY</div>");
            html.AppendLine($"  <div class='date-title'>MASS INTENTIONS FOR {massDate.ToString("MMMM dd, yyyy").ToUpper()}</div>");
            html.AppendLine("</div>");

            html.AppendLine("<div class='columns-container'>");

            foreach (var time in timesToProcess)
            {
                string formattedTime = DateTime.Today.Add(time).ToString("hh:mm tt");

                // Kunin ang mga intention para sa oras na ito
                var currentIntentions = groupedByTime.ContainsKey(time) ? groupedByTime[time] : new List<MassIntention>();

                // --- 1. RESOLVE PRIEST NAME ---
                string priestDisplay = "";

                // A. Subukan muna mula sa timePriestMap kung may nakapares na priestId
                if (timePriestMap != null && timePriestMap.ContainsKey(time) && timePriestMap[time] > 0)
                {
                    int priestId = timePriestMap[time];
                    var priestObj = await _context.Priests.FindAsync(priestId);
                    if (priestObj != null)
                    {
                        priestDisplay = priestObj.Name;
                    }
                }

                // B. Kung wala sa map, kunin sa PriestName property ng mga MassIntention records
                if (string.IsNullOrEmpty(priestDisplay))
                {
                    var recordWithPriest = currentIntentions.FirstOrDefault(m => !string.IsNullOrEmpty(m.PriestName));
                    if (recordWithPriest != null)
                    {
                        priestDisplay = recordWithPriest.PriestName!;
                    }
                }

                // C. Formatting sa pangalan ng Pari (lalagyan ng FR. kung wala pa)
                string formattedPriestText = "";
                if (!string.IsNullOrWhiteSpace(priestDisplay))
                {
                    string pName = priestDisplay.Trim();
                    formattedPriestText = pName.StartsWith("Fr.", StringComparison.OrdinalIgnoreCase)
                        ? pName.ToUpper()
                        : "FR. " + pName.ToUpper();
                }

                html.AppendLine("<div class='mass-block'>");

                // MASS HEADER BAR (Pari sa Kaliwa, Mass Time sa Kanan)
                html.AppendLine("  <table class='mass-header-table'>");
                html.AppendLine("    <tr>");
                html.AppendLine($"     <td class='mass-header-priest'>{formattedPriestText}</td>");
                html.AppendLine($"     <td class='mass-header-time'>MASS TIME: {formattedTime}</td>");
                html.AppendLine("    </tr>");
                html.AppendLine("  </table>");

                // 1. HEALING & GOOD HEALTH
                var healing = currentIntentions.Where(m => m.Category == IntentionCategory.Healing).ToList();
                html.AppendLine("  <div class='category-title'>I. HEALING & GOOD HEALTH</div>");
                AppendNamesList(html, healing);

                // 2. THANKSGIVING
                var thanksgiving = currentIntentions.Where(m => m.Category == IntentionCategory.Thanksgiving).ToList();
                html.AppendLine("  <div class='category-title'>II. THANKSGIVING</div>");
                AppendNamesList(html, thanksgiving);

                // 3. ETERNAL REPOSE
                var eternalRepose = currentIntentions.Where(m => m.Category == IntentionCategory.EternalRepose).ToList();
                html.AppendLine("  <div class='category-title'>III. ETERNAL REPOSE (FOR THE FAITHFUL DEPARTED)</div>");
                AppendNamesList(html, eternalRepose);

                // 4. OTHER INTENTIONS & SPECIAL PETITIONS
                var others = currentIntentions.Where(m => m.Category == IntentionCategory.Other ||
                                                           m.Category.ToString().Contains("Special")).ToList();
                html.AppendLine("  <div class='category-title'>IV. OTHER INTENTIONS & SPECIAL PETITIONS</div>");
                AppendNamesList(html, others);

                html.AppendLine("</div>");
            }

            html.AppendLine("</div>"); // End columns-container
            html.AppendLine("</div>"); // End Section1
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return Encoding.UTF8.GetBytes(html.ToString());
        }

        private void AppendNamesList(StringBuilder html, List<MassIntention> items)
        {
            if (items != null && items.Any())
            {
                html.AppendLine("  <ul class='names-list'>");
                foreach (var item in items)
                {
                    if (!string.IsNullOrWhiteSpace(item.OfferingNames))
                    {
                        var namesArray = item.OfferingNames.Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var name in namesArray)
                        {
                            html.AppendLine($"    <li>{name.Trim()}</li>");
                        }
                    }
                }
                html.AppendLine("  </ul>");
            }
            else
            {
                html.AppendLine("  <div class='no-names'>- NONE -</div>");
            }
        }
    }
}