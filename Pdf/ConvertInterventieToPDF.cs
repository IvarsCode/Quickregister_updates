using System;
using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.IO.Font.Constants;
using iText.IO.Image;
using QuickRegister.Data;
using QuickRegister.Models;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Pdf.Canvas;

namespace QuickRegister.Pdf.ConvertInterventieToPDF
{
    class ServiceBonPdf
    {
        // A4 usable width = 595 - 30 (left) - 30 (right) = 535pt
        // Tijden table: { 5%, 25%, 25%, 10%, 25% } of 535pt
        //   #         =  5% =  26.75pt
        //   Begintijd = 25% = 133.75pt
        //   Eindtijd starts at 30% = 160.5pt  ← this is our column 2 alignment point
        private const float COL1 = 160f;
        private const float COL2 = 375f;

        private readonly AppDbContext _db;

        public ServiceBonPdf(AppDbContext db)
        {
            _db = db;
        }

        public string GeneratePdf(int interventieId, string medewerkerNaam)
        {
            string outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = System.IO.Path.Combine(
                outputFolder,
                $"Werkbon_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
            );
            var imageData = ImageDataFactory.Create("Pdf/voilap.png");
            var image = new Image(imageData);

            try
            {
                var interventie = GetInterventie(interventieId);
                var calls = GetInterventieCalls(interventieId);

                if (interventie == null)
                    throw new Exception($"Interventie with ID {interventieId} not found");

                using var writer = new PdfWriter(filePath);
                using var pdf = new PdfDocument(writer);
                using var doc = new Document(pdf, PageSize.A4);

                doc.SetMargins(30, 30, 50, 30);

                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                doc.SetFont(normalFont);
                doc.SetFontSize(8);

                // ===== HEADER =====

                Table header = new Table(new float[] { COL1 + COL2 - 120f, 120f });
                header.SetBorder(Border.NO_BORDER);

                header.AddCell(
                    new Cell()
                        .Add(new Paragraph("WERKBON").SetFont(boldFont).SetFontSize(14))
                        .SetBorder(Border.NO_BORDER)
                        .SetVerticalAlignment(VerticalAlignment.TOP)
                );

                Cell contactCell = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.TOP)
                    .SetTextAlignment(TextAlignment.LEFT);

                contactCell.Add(new Paragraph("Contact:")
                    .SetFont(boldFont).SetFontSize(8).SetMarginBottom(0));
                contactCell.Add(new Paragraph("service.nl@voilap.com")
                    .SetFont(normalFont).SetFontSize(8).SetMargin(0));
                contactCell.Add(new Paragraph("+31 180 315 858")
                    .SetFont(normalFont).SetFontSize(8).SetMargin(0));

                header.AddCell(contactCell);

                doc.Add(header);
                doc.Add(image.SetWidth(100).SetHorizontalAlignment(HorizontalAlignment.LEFT));
                doc.Add(CreateSeparator());

                // ===== ONLINE SUPPORT =====

                doc.Add(new Paragraph("ONLINE SUPPORT").SetFont(boldFont).SetFontSize(10));

                Table algemeen = new Table(new float[] { COL1, COL2 });
                AddRow(algemeen, "Geholpen door:", medewerkerNaam, boldFont, normalFont);
                AddRow(algemeen, "Uitvoerdatum:",
                    DateTime.Now.ToString("dd-MM-yyyy") + "\u00A0" + DateTime.Now.ToString("HH:mm"),
                    boldFont, normalFont);

                doc.Add(algemeen);
                doc.Add(CreateSeparator());

                // ===== BEDRIJF =====

                doc.Add(new Paragraph("Bedrijf:").SetFont(boldFont).SetFontSize(10));

                Table adres = new Table(new float[] { COL1, COL2 });
                AddRow(adres, "Naam:",
                    string.IsNullOrWhiteSpace(interventie.BedrijfNaam) ? "-" : interventie.BedrijfNaam,
                    boldFont, normalFont);
                AddRow(adres, "Adres:",
                    $"{interventie.StraatNaam ?? "-"} {interventie.AdresNummer ?? ""}",
                    boldFont, normalFont);
                AddRow(adres, "Plaats:",
                    (string.IsNullOrWhiteSpace(interventie.Postcode) ? "-" : interventie.Postcode)
                    + (string.IsNullOrWhiteSpace(interventie.Stad) ? "" : $"\u00A0{interventie.Stad}"),
                    boldFont, normalFont);
                AddRow(adres, "Land:",
                    string.IsNullOrWhiteSpace(interventie.Land) ? "-" : interventie.Land,
                    boldFont, normalFont);

                doc.Add(adres);
                doc.Add(CreateSeparator());

                // ===== MACHINE =====

                Table machine = new Table(new float[] { COL1, COL2 });
                AddRow(machine, "Support verleend op:", interventie.Machine ?? "-", boldFont, normalFont);
                doc.Add(machine);
                doc.Add(CreateSeparator());

                // ===== GEWERKTE TIJD =====

                doc.Add(new Paragraph("Gewerkte tijd(en)").SetFont(boldFont).SetFontSize(10));

                // Col widths chosen so that #(27) + Begintijd(133) = 160 = COL1
                // meaning Eindtijd column edge aligns exactly with COL2 values above
                Table tijden = new Table(new float[] { 27f, 133f, 133f, 54f, 188f });

                tijden.AddHeaderCell(Header("#", boldFont));
                tijden.AddHeaderCell(Header("Begintijd", boldFont));
                tijden.AddHeaderCell(Header("Eindtijd", boldFont));
                tijden.AddHeaderCell(Header("Totaal", boldFont));
                tijden.AddHeaderCell(Header("ContactPersoon", boldFont));

                int i = 0;

                foreach (var call in calls)
                {
                    i++;

                    string startText = call.StartCall.HasValue
                        ? call.StartCall.Value.ToString("dd-MM-yyyy") + "\u00A0" + call.StartCall.Value.ToString("HH:mm")
                        : "-";
                    string endText = call.EindCall.HasValue
                        ? call.EindCall.Value.ToString("dd-MM-yyyy") + "\u00A0" + call.EindCall.Value.ToString("HH:mm")
                        : "-";

                    string durationText = "-";
                    if (call.StartCall.HasValue && call.EindCall.HasValue)
                    {
                        var duration = call.EindCall.Value - call.StartCall.Value;
                        durationText = duration.ToString(@"hh\:mm");
                    }

                    string contactNaam = call.ContactpersoonNaam ?? "-";

                    tijden.AddCell(new Cell().Add(new Paragraph(i.ToString()).SetFontSize(8)));
                    tijden.AddCell(new Cell().Add(new Paragraph(startText).SetFontSize(8)));
                    tijden.AddCell(new Cell().Add(new Paragraph(endText).SetFontSize(8)));
                    tijden.AddCell(new Cell().Add(new Paragraph(durationText).SetFontSize(8)));
                    tijden.AddCell(new Cell().Add(new Paragraph(contactNaam).SetFontSize(8)));
                }

                doc.Add(tijden);

                var total = TimeSpan.FromMinutes(
                    calls
                        .Where(c => c.StartCall.HasValue && c.EindCall.HasValue)
                        .Sum(c => Math.Floor((c.EindCall!.Value - c.StartCall!.Value).TotalMinutes))
                );

                Table totalRow = new Table(new float[] { COL1, COL2 });
                AddRow(totalRow, "Totaal gewerkte tijd:", total.ToString(@"hh\:mm"), boldFont, boldFont);
                doc.Add(totalRow);

                // ===== NOTITIES =====

                doc.Add(CreateSeparator());
                doc.Add(new Paragraph("Gespreksnotities:").SetFont(boldFont).SetFontSize(10));

                for (int j = 0; j < calls.Count; j++)
                {
                    var call = calls[j];

                    doc.Add(CreateSeparator());

                    string callDateStr = call.StartCall.HasValue
                        ? call.StartCall.Value.ToString("dd-MM-yyyy") + "\u00A0" + call.StartCall.Value.ToString("HH:mm")
                        : "-";

                    doc.Add(new Paragraph($"Call {j + 1}  \u2002{callDateStr}\u2002 {call.ContactpersoonNaam ?? ""}")
                        .SetFont(boldFont).SetFontSize(9));

                    doc.Add(new Paragraph(call.ExterneNotities ?? "-")
                        .SetFont(normalFont).SetFontSize(8));
                }

                // ===== CONTACT PERSONEN =====

                doc.Add(CreateSeparator());
                doc.Add(new Paragraph("Contact Personen:").SetFont(boldFont).SetFontSize(10));

                List<string> contactpersonen = new List<string>();

                foreach (var call in calls)
                {
                    var naam = call.ContactpersoonNaam ?? "-";

                    if (!contactpersonen.Contains(naam) && naam != "-")
                    {
                        contactpersonen.Add(naam);

                        doc.Add(new Paragraph(naam).SetFont(boldFont).SetFontSize(9));

                        Table contactTable = new Table(new float[] { COL1, COL2 });

                        if (call.ContactpersoonTelefoonNummer != null)
                            AddRow(contactTable, "Telefoonnummer:", call.ContactpersoonTelefoonNummer, boldFont, normalFont);

                        if (call.ContactpersoonEmail != null)
                            AddRow(contactTable, "Email:", call.ContactpersoonEmail, boldFont, normalFont);

                        doc.Add(contactTable);
                    }
                }

                // ===== FOOTER =====

                string footerText = "Voilàp Netherlands B.V. | Hoogeveenenweg 204 | 2913 LV Nieuwerkerk a/d IJssel | www.elumatec.com";
                PdfFont footerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                for (int p = 1; p <= pdf.GetNumberOfPages(); p++)
                {
                    var page = pdf.GetPage(p);
                    var pageSize = page.GetPageSize();
                    var pdfCanvas = new PdfCanvas(page);

                    pdfCanvas.SetLineWidth(0.5f);
                    pdfCanvas.MoveTo(40, 35);
                    pdfCanvas.LineTo(pageSize.GetWidth() - 40, 35);
                    pdfCanvas.Stroke();

                    using var canvas = new Canvas(pdfCanvas, pageSize);
                    canvas.SetFont(footerFont).SetFontSize(8);
                    canvas.ShowTextAligned(
                        footerText,
                        pageSize.GetWidth() / 2,
                        20,
                        TextAlignment.CENTER
                    );
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating PDF:\n" + ex, ex);
            }

            return filePath;
        }

        // ===== DATABASE =====

        private Interventie? GetInterventie(int id)
        {
            try
            {
                return InterventieRepository.GetById(_db, id);
            }
            catch
            {
                return null;
            }
        }

        private List<InterventieCall> GetInterventieCalls(int interventieId)
        {
            try
            {
                var interventie = InterventieRepository.GetById(_db, interventieId);
                return interventie?.Calls?.ToList() ?? new List<InterventieCall>();
            }
            catch
            {
                return new List<InterventieCall>();
            }
        }

        // ===== HELPERS =====

        private static void AddRow(Table table, string label, string value, PdfFont bold, PdfFont normal)
        {
            table.AddCell(
                new Cell()
                    .Add(new Paragraph(label).SetFont(bold).SetFontSize(8))
                    .SetBorder(Border.NO_BORDER)
            );
            table.AddCell(
                new Cell()
                    .Add(new Paragraph(value).SetFont(normal).SetFontSize(8))
                    .SetBorder(Border.NO_BORDER)
            );
        }

        private static Cell Header(string text, PdfFont bold)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(bold).SetFontSize(8))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY);
        }

        private static LineSeparator CreateSeparator()
        {
            var line = new LineSeparator(new SolidLine(1f));
            line.SetMarginTop(5);
            line.SetMarginBottom(5);
            return line;
        }
    }
}
