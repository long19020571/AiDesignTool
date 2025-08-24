All Rights Reserved © 2025 Nguyen Viet Long

This project and all associated files are the intellectual property of Nguyen Viet Long.

You are not allowed to use, copy, modify, distribute, or exploit this project in any form without explicit written permission from the author.

For inquiries about usage, licensing, or collaboration, please contact:
nguyenvietlong1201@gmail.com

# AiDesignTool 🎨

AiDesignTool is a **WPF desktop application (C#)** designed to automate and enhance workflows with **Adobe Illustrator templates**.  
It leverages the **Adobe Illustrator COM library** to automate design tasks, modify templates, and streamline the design and production process.

---

## ✨ Features

- 🖼 Load and manage Adobe Illustrator design templates.
- ⚡ Automate repetitive Illustrator tasks via COM automation.
- 🛠 Programmatically apply template modifications and batch operations.
- 💾 Save and export processed templates in multiple formats (AI, PNG, etc.).
- 📦 Manage design configurations, panels, and profiles with a built-in database.
- 🧩 Advanced geometry and text manipulation using NetTopologySuite and SkiaSharp.
- 🔄 Error checking and interactive correction for design placement.

---

## 🏗 Project Structure

- **/LCommands/**: Core logic for Illustrator automation, data processing, and database operations.
- **/LObjects/**: Data models for design configs, panels, arts, and mappings.
- **MainWindow.xaml / MainWindow.xaml.cs**: WPF UI and main application logic.
- **ViewModel**: MVVM pattern for UI binding and state management.
- **Database**: Uses LiteDB for local storage of profiles, panels, and configurations.

---

## ⚙️ Technologies & Innovations

- **.NET 8**: Modern, high-performance runtime for desktop applications.
- **WPF (Windows Presentation Foundation)**: Rich desktop UI framework.
- **Adobe Illustrator COM Interop**: Deep integration and automation of Illustrator.
- **LiteDB**: Embedded NoSQL database for fast, local data storage.
- **NetTopologySuite**: Advanced geometry processing for vector shapes and placement.
- **SkiaSharp**: High-performance 2D graphics for font and path manipulation.
- **MVVM Architecture**: Clean separation of UI and business logic for maintainability.

---

## 🚀 What Makes It Different?

- **Full automation** of Illustrator tasks, reducing manual work and errors.
- **Batch processing** of templates and orders, suitable for production environments.
- **Customizable design logic** via configuration and "magic" actions.
- **Interactive error handling**: Detects and allows correction of placement or data mismatches.
- **Extensible**: Easily add new design rules, templates, or export formats.

---

## 🏁 Getting Started

1. Install **Adobe Illustrator** and ensure COM access is enabled.
2. Build the solution with **.NET 8** and required NuGet packages.
3. Run the application, load your templates, and start automating your design workflow.

---

For more details, see the source code and contact the author for advanced usage or integration.
