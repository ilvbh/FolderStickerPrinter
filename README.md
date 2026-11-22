# Folder Sticker Printer — Version 2 prototype

Windows desktop starter app for the pre-printed self-adhesive folder-label sheet.

## Sheet and loading

- Physical sheet: 275 mm wide × 200 mm high.
- Five vertical labels, each 55 mm × 200 mm.
- Short 200 mm edge enters the Brother MFC-L6900DW MP Tray first.
- Pre-printed logo side faces up; the app does not print in the logo zone.
- Logo exclusion: 0–30 mm.
- Text bands: 31–64, 64–97, 97–131, 131–165, and 165–198 mm.

## Text

- Five different labels, each with five independent one-line text fields.
- Times New Roman.
- Row 1 is automatically uppercase and bold; its font size remains editable per label.
- Rows 2–5 have independent editable font sizes per label (4–48 pt).
- Text is centered horizontally and vertically and clipped to its own sticker cell.
- Preview font sizes are converted from points to millimetres for a closer physical-size representation.

## Printing changes in v2

- Defines the logical page as 275 × 200 mm and disables an additional landscape rotation. This is intended to avoid a double-orientation transform with the driver.
- Selects the Brother driver's MP / Multi-Purpose source by name and stops if it cannot find one. It never intentionally selects Trays 1–4.
- Applies the driver's reported hard-margin compensation plus user X/Y calibration offsets (in mm).
- User calibration offsets are available next to the Print button.

## Critical test notes

This is a revised prototype, not hardware-verified. Printer drivers differ in how they interpret custom form orientation, hard margins, and paper-source requests. The app cannot reliably query MP Tray empty status through the standard .NET printing API. The printer driver/firmware must be configured to pause/request paper for MP Tray rather than substitute another tray.

**Do not use an adhesive sheet for the first test.** First test on ordinary paper with the same 275 × 200 mm custom form and verify orientation and registration. If the driver does not offer that custom size, configure it in Brother Printing Preferences / printer server properties. Confirm the printer waits for paper in the MP Tray when it is empty; this behavior is controlled by the Brother driver and printer, not guaranteed by this application.

## Build on Windows

Requires .NET 8 SDK:

```powershell
dotnet build .\FolderStickerPrinter\FolderStickerPrinter.csproj -c Release
dotnet publish .\FolderStickerPrinter\FolderStickerPrinter.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
```

## GitHub Actions

The workflow is at `.github/workflows/build-windows.yml`. Upload the whole project to the repository root. It builds on pushes to `main` or manually via **Actions → Build Windows app → Run workflow**. Download the `FolderStickerPrinter-Windows` artifact from the completed run.

GitHub hosts the source and build artifact. The installed Windows desktop app performs the actual printer communication; GitHub Pages/browser-only hosting cannot enforce Windows printer tray selection.
