# Changelog

## 1.3.0 — 2026-11-17

- Rename the app to iEnter across the interface, tray, executable, downloads, and documentation.
- Keep the existing settings folder and single-instance guard for compatibility with Auto Enter installations.
- Preserve the vector icon, high-DPI rendering, and existing input behavior.

## 1.2.2 — 2026-11-17

- Fix compiler resource-argument quoting in PowerShell 7, while retaining Windows PowerShell 5.1 compatibility.
- Includes the vector icon and high-DPI fixes from 1.2.1.

## 1.2.1 — 2026-11-17

- Fix blurry header artwork by drawing vector paths at the actual control size instead of stretching a 32 px shell icon.
- Enable .NET Framework 4.8 PerMonitorV2 DPI awareness; scale custom controls and layouts with the screen.
- Include ten native Windows icon sizes and a 1024 px PNG preview, with editable Figma and SVG sources.
- Bundle `AutoEnter.exe.config`; keep it next to the executable when installing or updating.
- Input detection, ending-word matching, and Enter behavior are unchanged.

## 1.2.0 — 2026-11-17

- New light interface with rounded cards, a clear on/off switch, and compact options.
- Flat blue app icon with a heavier white return symbol.
- One-click English / Chinese switching, including tray menus and status messages.
- Language preference is saved; new installations start in English.
- Keeps automatic Enter, custom ending words, app scope, delay, and cancellation behavior.
- First public GitHub release, with Windows builds and automated tests.

## 1.1.0 — 2026-11-17

- Custom ending words in Chinese, English, or short phrases.
- Immediate saving, empty-word handling, and migration from the original `over` setting.

## 1.0.0 — 2026-11-16

- Initial Windows tray utility with automatic Enter and `over` modes.
