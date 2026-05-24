# NmapMaui - Desktop Security and Network Diagnostics Suite

NmapMaui is a cross-platform desktop application built with .NET MAUI that provides a comprehensive control panel for network diagnostics and cryptography utilities. 

Designed for systems analysts, network engineers, and security professionals, NmapMaui features a high-performance, dark cyber-terminal theme. The application compiles natively for Windows, macOS, iOS, and Android.

---

## Technical Design & Visual Theme

The user interface utilizes a customized, professional cybersecurity terminal aesthetic:
* **Typography**: Integrated with the Google Fonts monospaced typeface **IBM Plex Mono** across all native control elements, entries, log outputs, and labels to ensure maximum technical readability.
* **Color System**: Curated, high-contrast dark theme tokens:
  * **Primary Accents**: Neon Cyber Yellow (`#FFEF46`)
  * **Main Background**: Solid Pitch Black (`#000000`)
  * **Layered Containers**: Solid Obsidian Charcoal (`#0E0E10`)
  * **Core Reading Text**: Pure White (`#FFFFFF`) and High-Contrast Silver (`#E5E5EA`)
* **Background Controls**: Matrix visual backgrounds are rendered strictly on the entry panels (Login Gate and MainPage Dashboard). Tool sub-pages automatically fall back to solid black layouts to ensure absolute visual clarity during dense diagnostic sessions.

---

## Authentication & Security Rules

The authentication layers in the client application and the API backend enforce standard security validation rules:
1. **Username Validation**:
   - Minimum username length is restricted to **3 characters** for flexibility.
2. **Password Complexity**:
   - Must be at least **8 characters** in length.
   - Must contain at least **1 uppercase letter** (`A-Z`).
   - Must contain at least **1 numeric digit** (`0-9`).
   - *These criteria are enforced uniformly across Registration, API connections, and Password Updates.*

---

## Local Document Exporting

All diagnostic and tool scan outputs can be exported locally in JSON, CSV, and PDF formats:
* **Output Path**: Automatically saved inside a local folder named `Documents` created at the root directory of the project.
* **Streamlined Flow**: The Windows `FileSavePicker` dial is bypassed, providing a silent and fast file generation mechanism.
* **Repository Exclusions**: The local `/Documents` directory is registered in `.gitignore` to prevent any exported data from being tracked by git.

---

## Tool Directory

### Network Diagnostics
* **Port Scanner**: Audits target hosts asynchronously to identify active TCP ports.
* **ICMP Ping**: Dispatches ICMP echo requests to measure target network latency and availability.
* **DNS Resolver**: Queries A and AAAA records of a domain for name resolution checks.
* **Gobuster Web**: Enumerates web directories and fuzzer subdomains via HTTP endpoint testing.
* **Netcat Utility**: Establishes raw TCP connections, performs socket listening, and grabs port banners.

### Cryptographic Utilities
* **Hash Calculator**: Computes and verifies checksum algorithms including MD5, SHA-1, SHA-256, and SHA-512.
* **Base64 Coder**: Encodes raw text to Base64 format or decodes Base64 strings.
* **Cipher Engine**: Encrypts and decrypts text using symmetric key algorithms.
* **Credential Generator**: Generates high-entropy random passwords based on length and charset criteria.
* **Strength Estimator**: Calculates complexity scores and cracks latency estimation.
* **Breach Auditor**: Interfaces with the HIBP API to check if credentials appear in public data breaches.

---

## Installation & Running the Project

### Prerequisites
* [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
* Visual Studio 2022 or VS Code (with MAUI workload configured)
* SqlServer (For the API database backend)

---

### Step 1: Run the Backend API Server
The suite includes an ASP.NET Core minimal API backend to process remote scans and log database entries.

1. Navigate to the API project folder:
   ```powershell
   cd NmapMaui.Api
   ```
2. Start the server using the .NET CLI:
   ```powershell
   dotnet run
   ```
   *(By default, the server listens at `http://localhost:5000`)*

---

### Step 2: Run the Desktop Client Application
In another terminal session, navigate to the main directory of the project to launch the .NET MAUI desktop app.

1. Restore project dependencies:
   ```powershell
   dotnet restore
   ```
2. Run the application targeting your native operating system:
   * **Windows**:
     ```powershell
     dotnet run -f net9.0-windows10.0.19041.0
     ```
   * **Android**:
     ```powershell
     dotnet run -f net9.0-android
     ```
   * **macOS / iOS**:
     ```powershell
     dotnet run -f net9.0-maccatalyst
     dotnet run -f net9.0-ios
     ```

---

## Directory Structure

```
MauiNmap/
├── NmapMaui.Api/         # ASP.NET Core minimal API project
│   ├── Program.cs        # Main entry, database configuration, and JWT middleware
│   └── ...
├── Models/               # Data schema definitions for client models
├── Services/             # Business logic layers (ExportService, AuthService, CryptoService)
├── ViewModels/           # ViewModels for MVVM layout binding
├── Views/                # XAML Pages (LoginPage, MainPage, sub-tool dashboards)
├── Resources/
│   ├── Fonts/            # Custom IBM Plex Mono TTF font files
│   ├── Images/           # Design assets, system icons, and bg_cyber.png
│   └── Styles/           # Main Colors.xaml & Styles.xaml files
├── Documents/            # [Ignored] Automatically generated local report exports folder
├── NmapMaui.csproj       # Main MAUI client project file
└── .gitignore            # Excludes build assets, local dbs, and /Documents/
```
