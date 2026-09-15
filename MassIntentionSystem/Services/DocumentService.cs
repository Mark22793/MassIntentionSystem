using System.Text;
using MassIntentionSystem.Data;
using MassIntentionSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace MassIntentionSystem.Services
{
    public class DocumentService
    {
        private readonly ApplicationDbContext _context;

        public DocumentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateMassIntentionDocAsync(DateTime massDate, TimeSpan massTime, int priestId)
        {
            var priest = await _context.Priests.FindAsync(priestId);

            // Kukunin lang ang mga Mass Intentions na Verified ang payment para sa napiling Petsa at Oras
            var intentions = await _context.MassIntentions
                .Where(m => m.MassDate.Date == massDate.Date
                         && m.MassTime == massTime
                         && m.PaymentStatus == "Verified")
                .ToListAsync();

            var sb = new StringBuilder();
            sb.Append("<html><head><style>");
            sb.Append("body { font-family: Arial, sans-serif; margin: 30px; }");
            sb.Append("h2 { color: #4a154b; text-align: center; margin-bottom: 2px; text-transform: uppercase; }");
            sb.Append("h3 { text-align: center; color: #555; margin-top: 0; }");
            sb.Append(".meta-info { margin-bottom: 20px; font-size: 14px; }");
            sb.Append("h4 { color: #7b2cbf; border-bottom: 2px solid #d4af37; padding-bottom: 4px; margin-top: 20px; }");
            sb.Append("ul { margin-top: 5px; line-height: 1.5; }");
            sb.Append("</style></head><body>");

            sb.Append("<h2>NATIONAL SHRINE OF OUR LADY OF LA NAVAL DE MANILA</h2>");
            sb.Append("<h3>SANTO DOMINGO CHURCH, QUEZON CITY</h3>");
            sb.Append("<div class='meta-info'>");
            sb.Append($"<p><b>MASS DATE:</b> {massDate:MMMM dd, yyyy} | <b>TIME:</b> {DateTime.Today.Add(massTime):hh:mm tt}</p>");
            sb.Append($"<p><b>CELEBRANT / PRIEST:</b> {priest?.Name ?? "N/A"}</p>");
            sb.Append("</div><hr/>");

            // 1. Healing & Good Health
            sb.Append("<h4>I. HEALING & GOOD HEALTH</h4><ul>");
            var healingList = intentions.Where(i => i.Category == IntentionCategory.Healing).ToList();
            if (healingList.Any())
            {
                foreach (var item in healingList)
                    sb.Append($"<li>{item.OfferingNames}</li>");
            }
            else { sb.Append("<li><i>None</i></li>"); }
            sb.Append("</ul>");

            // 2. Thanksgiving
            sb.Append("<h4>II. THANKSGIVING</h4><ul>");
            var thanksgivingList = intentions.Where(i => i.Category == IntentionCategory.Thanksgiving).ToList();
            if (thanksgivingList.Any())
            {
                foreach (var item in thanksgivingList)
                    sb.Append($"<li>{item.OfferingNames}</li>");
            }
            else { sb.Append("<li><i>None</i></li>"); }
            sb.Append("</ul>");

            // 3. Eternal Repose
            sb.Append("<h4>III. ETERNAL REPOSE (FOR THE FAITHFUL DEPARTED)</h4><ul>");
            var eternalList = intentions.Where(i => i.Category == IntentionCategory.EternalRepose).ToList();
            if (eternalList.Any())
            {
                foreach (var item in eternalList)
                    sb.Append($"<li>+ {item.OfferingNames}</li>");
            }
            else { sb.Append("<li><i>None</i></li>"); }
            sb.Append("</ul>");

            // 4. Other Intentions
            sb.Append("<h4>IV. OTHER INTENTIONS & SPECIAL PETITIONS</h4><ul>");
            var otherList = intentions.Where(i => i.Category == IntentionCategory.Other).ToList();
            if (otherList.Any())
            {
                foreach (var item in otherList)
                    sb.Append($"<li>{item.OfferingNames}</li>");
            }
            else { sb.Append("<li><i>None</i></li>"); }
            sb.Append("</ul></body></html>");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}