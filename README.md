# Folder Sticker Printer

A Windows desktop starter app for the pre-printed, self-adhesive folder-label sheet used with a Brother MFC-L6900DW series printer.

## What it does

- Five independent folder labels across one 275 × 200 mm sheet.
- Each label is 55 × 200 mm.
- Leaves the pre-printed logo zone (0–30 mm) blank.
- Five one-line text rows per label, centered.
- Row 1 is automatically uppercase and bold.
- Each text cell has its own font-size setting (4–48 pt).
- Uses Times New Roman.
- Sends jobs directly through the installed Windows printer driver.
- Explicitly selects the driver's MP / Multi-Purpose Tray source; stops if no MP source is exposed.

## Important printing notes

The app selects the MP Tray in the Windows print settings and does not select Trays 1–4. The Brother driver and printer firmware still control how an empty tray or paper mismatch is handled. Windows does not consistently expose reliable per-tray-empty status through the standard .NET printing API. Therefore, before production use:

1. Install the official Brother MFC-L6900DW series driver.
2. Open the printer's Printing Preferences and configure the MP Tray / manual feed behavior so an empty MP Tray causes a paper request or pause, not automatic source substitution.
3. Ensure the custom paper size is configured as 200 × 275 mm (feed width × feed length) and the media type is Labels, if supported by the driver.
4. Test with one sacrificial sheet and confirm the printer asks for the MP Tray when it is empty. Do not test with a valuable adhesive sheet until the driver behavior is verified.

This starter app deliberately stops if the installed driver does not expose a paper source whose name contains `MP`, `Multi-Purpose`, or `Multipurpose`. It does not silently choose another source.

## Build on Windows

Requires the .NET 8 SDK.

```powershell
dotnet build .\FolderStickerPrinter\FolderStickerPrinter.csproj -c Release
dotnet publish .\FolderStickerPrinter\FolderStickerPrinter.csproj -c Release -r win-x64 --self-contained true
```

The published app will be under:

`FolderStickerPrinter\bin\Release\net8.0-windows\win-x64\publish\`

Run `FolderStickerPrinter.exe`.

## GitHub

Create a GitHub repository and upload this project. The source can be hosted on GitHub. GitHub Pages cannot run native Windows printer control; the locally installed Windows application is the component that prints directly.

## Geometry

- Physical sheet: 275 × 200 mm.
- Printer feed orientation: 200 mm feed width × 275 mm feed length, landscape page layout.
- Five labels: 55 × 200 mm each.
- Logo exclusion: 0–30 mm.
- Text row bands: 31–64, 64–97, 97–131, 131–165, 165–198 mm.
- Small print safety inset: 0.5 mm inside each row.
