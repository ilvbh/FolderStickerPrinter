using System.Drawing.Printing;
using System.Drawing.Drawing2D;

namespace FolderStickerPrinter;

public sealed class MainForm : Form
{
    private const string TargetPrinter = "Brother MFC-L6900DW series";
    private readonly DataGridView grid = new();
    private readonly Panel preview = new();
    private readonly Label status = new();
    private readonly NumericUpDown copies = new();
    private readonly Button printButton = new();
    private readonly string[,] values = new string[5, 5];
    private readonly decimal[,] sizes = new decimal[5, 5];

    public MainForm()
    {
        Text = "Folder Sticker Printer";
        Width = 1450;
        Height = 900;
        MinimumSize = new Size(1100, 720);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9);

        for (int c = 0; c < 5; c++)
            for (int r = 0; r < 5; r++)
                sizes[c, r] = r == 0 ? 12 : 10;

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(12) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var header = new Panel { Dock = DockStyle.Fill };
        var title = new Label { Text = "Folder sticker sheet", Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true, Location = new Point(0, 4) };
        var subtitle = new Label { Text = "5 labels • 55 × 200 mm each • Times New Roman • direct printing", AutoSize = true, ForeColor = Color.DimGray, Location = new Point(2, 34) };
        header.Controls.Add(title); header.Controls.Add(subtitle);
        root.Controls.Add(header, 0, 0);
        root.SetColumnSpan(header, 2);

        var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        root.Controls.Add(left, 0, 1);

        var instructions = new Label {
            Dock = DockStyle.Fill, Text = "Enter one line per row. Each column is a separate folder label. Set each cell's font size in points.",
            ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleLeft
        };
        left.Controls.Add(instructions, 0, 0);

        BuildGrid();
        left.Controls.Add(grid, 0, 1);

        var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 12, 0, 0) };
        bottom.Controls.Add(new Label { Text = "Copies:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        copies.Minimum = 1; copies.Maximum = 99; copies.Value = 1; copies.Width = 60;
        bottom.Controls.Add(copies);
        printButton.Text = "Print using MP Tray";
        printButton.AutoSize = true;
        printButton.Height = 38;
        printButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        printButton.Click += (_, _) => PrintSheets();
        bottom.Controls.Add(printButton);
        var clear = new Button { Text = "Clear all", AutoSize = true, Height = 38 };
        clear.Click += (_, _) => ClearAll();
        bottom.Controls.Add(clear);
        left.Controls.Add(bottom, 0, 2);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(14, 0, 0, 0) };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(right, 1, 1);
        right.Controls.Add(new Label { Text = "Sheet preview (not to physical scale)", Font = new Font("Segoe UI", 11, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        preview.Dock = DockStyle.Fill;
        preview.BackColor = Color.FromArgb(238, 240, 243);
        preview.Paint += PaintPreview;
        right.Controls.Add(preview, 0, 1);

        status.Dock = DockStyle.Bottom;
        status.Height = 30;
        status.TextAlign = ContentAlignment.MiddleLeft;
        status.ForeColor = Color.DarkSlateGray;
        status.Text = $"Printer target: {TargetPrinter}  |  Paper source: MP Tray only";
        Controls.Add(status);
    }

    private void BuildGrid()
    {
        grid.Dock = DockStyle.Fill;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.RowHeadersWidth = 88;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.ColumnHeadersHeight = 42;
        grid.RowTemplate.Height = 64;
        grid.EditMode = DataGridViewEditMode.EditOnEnter;

        for (int c = 0; c < 5; c++)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn {
                Name = $"Label{c + 1}", HeaderText = $"LABEL {c + 1}\nText", FillWeight = 75,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn {
                Name = $"Size{c + 1}", HeaderText = "Size\npt", FillWeight = 25,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        string[] rowNames = { "1  TITLE (BOLD CAPS)", "2  Row 2", "3  Row 3", "4  Row 4", "5  Row 5" };
        for (int r = 0; r < 5; r++)
        {
            int idx = grid.Rows.Add();
            grid.Rows[idx].HeaderCell.Value = rowNames[r];
            for (int c = 0; c < 5; c++)
            {
                grid.Rows[idx].Cells[c * 2].Value = "";
                grid.Rows[idx].Cells[c * 2 + 1].Value = sizes[c, r].ToString("0.##");
            }
        }
        grid.CellValueChanged += (_, e) => { if (e.RowIndex >= 0) preview.Invalidate(); };
        grid.CurrentCellDirtyStateChanged += (_, _) => { if (grid.IsCurrentCellDirty) grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };
        grid.DataError += (_, e) => { e.ThrowException = false; };
    }

    private void SyncValues()
    {
        for (int r = 0; r < 5; r++)
        for (int c = 0; c < 5; c++)
        {
            values[c, r] = Convert.ToString(grid.Rows[r].Cells[c * 2].Value) ?? "";
            string raw = Convert.ToString(grid.Rows[r].Cells[c * 2 + 1].Value) ?? "";
            if (!decimal.TryParse(raw, out decimal pt) || pt < 4 || pt > 48)
                throw new InvalidOperationException($"Invalid font size for Label {c + 1}, Row {r + 1}. Use 4–48 pt.");
            sizes[c, r] = pt;
        }
    }

    private void ClearAll()
    {
        for (int r = 0; r < 5; r++)
        for (int c = 0; c < 5; c++)
        {
            grid.Rows[r].Cells[c * 2].Value = "";
            grid.Rows[r].Cells[c * 2 + 1].Value = r == 0 ? "12" : "10";
        }
        preview.Invalidate();
    }

    private void PaintPreview(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(preview.BackColor);
        float margin = 24;
        float scale = Math.Min((preview.ClientSize.Width - margin * 2) / 275f, (preview.ClientSize.Height - margin * 2) / 200f);
        float x0 = (preview.ClientSize.Width - 275 * scale) / 2;
        float y0 = (preview.ClientSize.Height - 200 * scale) / 2;
        using var sheetPen = new Pen(Color.DimGray, 1.5f);
        using var linePen = new Pen(Color.LightGray, 1);
        using var logoBrush = new SolidBrush(Color.FromArgb(225, 231, 237));
        using var labelFont = new Font("Segoe UI", 7);
        e.Graphics.FillRectangle(Brushes.White, x0, y0, 275 * scale, 200 * scale);
        e.Graphics.DrawRectangle(sheetPen, x0, y0, 275 * scale, 200 * scale);
        for (int c = 0; c < 5; c++)
        {
            float x = x0 + c * 55 * scale;
            e.Graphics.DrawLine(linePen, x, y0, x, y0 + 200 * scale);
            e.Graphics.FillRectangle(logoBrush, x + 1, y0 + 1, 55 * scale - 2, 30 * scale - 1);
            TextRenderer.DrawText(e.Graphics, "PRE-PRINTED LOGO", labelFont,
                Rectangle.Round(new RectangleF(x, y0 + 10 * scale, 55 * scale, 10 * scale)),
                Color.Gray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            for (int r = 0; r < 5; r++)
            {
                float top = new[] { 31f, 64f, 97f, 131f, 165f }[r];
                float bottom = new[] { 64f, 97f, 131f, 165f, 198f }[r];
                float cy = y0 + ((top + bottom) / 2f) * scale;
                e.Graphics.DrawLine(linePen, x, y0 + top * scale, x + 55 * scale, y0 + top * scale);
                string text = grid.Rows[r].Cells[c * 2].Value?.ToString() ?? "";
                if (r == 0) text = text.ToUpperInvariant();
                float pt = float.TryParse(grid.Rows[r].Cells[c * 2 + 1].Value?.ToString(), out var f) ? f : (r == 0 ? 12 : 10);
                using var font = new Font("Times New Roman", Math.Max(4, Math.Min(48, pt)) * scale * 0.65f, r == 0 ? FontStyle.Bold : FontStyle.Regular);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
                var rect = new RectangleF(x + 1 * scale, y0 + top * scale, 53 * scale, (bottom - top) * scale);
                e.Graphics.DrawString(text, font, Brushes.Black, rect, sf);
            }
        }
        e.Graphics.DrawLine(sheetPen, x0 + 275 * scale, y0, x0 + 275 * scale, y0 + 200 * scale);
    }

    private void PrintSheets()
    {
        try
        {
            SyncValues();
            using var doc = new PrintDocument();
            doc.DocumentName = "Folder sticker sheet";
            doc.PrinterSettings.PrinterName = TargetPrinter;
            if (!doc.PrinterSettings.IsValid)
                throw new InvalidOperationException($"Printer '{TargetPrinter}' is not installed. Install/select the Brother MFC-L6900DW driver first.");

            // Explicitly select the Brother driver's MP / Multi-Purpose source. Never accept a fallback.
            var mp = doc.PrinterSettings.PaperSources.Cast<PaperSource>()
                .FirstOrDefault(s => s.SourceName.Contains("MP", StringComparison.OrdinalIgnoreCase)
                                  || s.SourceName.Contains("Multi-Purpose", StringComparison.OrdinalIgnoreCase)
                                  || s.SourceName.Contains("Multipurpose", StringComparison.OrdinalIgnoreCase));
            if (mp is null)
            {
                string available = string.Join(", ", doc.PrinterSettings.PaperSources.Cast<PaperSource>().Select(s => s.SourceName));
                throw new InvalidOperationException("The Brother driver did not expose an MP Tray paper source, so printing was stopped. Available sources: " + available);
            }

            doc.DefaultPageSettings.PaperSource = mp;
            doc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            doc.DefaultPageSettings.Landscape = true;
            doc.DefaultPageSettings.PaperSize = new PaperSize("Folder labels 200 x 275 mm", 7874, 10827);
            doc.OriginAtMargins = false;
            doc.PrinterSettings.Copies = (short)copies.Value;
            doc.PrintPage += DrawPrintPage;

            // Windows/driver reports general printer status, not always per-tray status.
            // The driver remains responsible for the empty-MP prompt; this app never selects another bin.
            doc.Print();
            status.Text = "Print job sent with MP Tray selected. If the printer requests paper, load the sticker sheet in the MP Tray only.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Printing stopped", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            status.Text = "Printing stopped. No alternate tray was selected.";
        }
    }

    private void DrawPrintPage(object? sender, PrintPageEventArgs e)
    {
        Graphics g = e.Graphics;
        g.PageUnit = GraphicsUnit.Millimeter;
        g.PageScale = 1f;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

        float[] tops = { 31, 64, 97, 131, 165 };
        float[] bottoms = { 64, 97, 131, 165, 198 };

        for (int c = 0; c < 5; c++)
        for (int r = 0; r < 5; r++)
        {
            string text = values[c, r];
            if (r == 0) text = text.ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(text)) continue;

            float pt = (float)sizes[c, r];
            using var font = new Font("Times New Roman", pt, r == 0 ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Point);
            using var sf = new StringFormat {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };
            // Keep a small safety inset while maximizing usable label width.
            var rect = new RectangleF(c * 55f + 0.5f, tops[r] + 0.5f, 54f, bottoms[r] - tops[r] - 1f);
            g.SetClip(rect);
            g.DrawString(text, font, Brushes.Black, rect, sf);
            g.ResetClip();
        }
        e.HasMorePages = false;
    }
}
