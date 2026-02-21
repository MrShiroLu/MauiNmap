# NmapMaui

NmapMaui is a cross-platform .NET MAUI application that provides a set of network and cryptography tools, including hash calculation and verification, password management, and more. The app is designed with a modern UI and supports Windows, Android, iOS, and MacCatalyst platforms.

## Features

- **Hash Calculator Page:**
    - Hash Calculation
    - Hash Verification

- **Password Management:**
    - Login Page
    - Register Page
    - Change Password Page

- **Base64 Tool Page:**
    - Base64 Encode
    - Base64 Decode

- **Cryptography Tools:**
    - Encryption Tool
    - Decryption Tool

- **Password Tools:**
    - Password Generator
    - Password Strength Checker
    - Password Leak Checker

- **Network Tools:**
    - Ping Tool
    - Port Scanner
    - DNS Lookup Tool

- **Database Integration:**
    - Local SQLite Database Logging
    - Operation History

---

## Dashboard Sections & Their Tools

### Network Tools
- **Ping Tool:** Test the reachability and latency of a host (IP address or domain) to diagnose network issues.
- **Port Scanner:** Scan a range of ports on a host to see which ones are open, useful for security and network analysis.
- **DNS Lookup Tool:** Query the IP address and DNS records of a domain name for DNS troubleshooting and configuration checks.

### Crypto Tools
- **Hash Calculator:** Generate hashes (MD5, SHA1, SHA256, SHA384, SHA512) for any text, or verify if a text matches a given hash.
- **Encryption Tool:** Encrypt text using a user-provided key.
- **Decryption Tool:** Decrypt previously encrypted text using the same key.

### Password Tools
- **Password Generator:** Create strong, random passwords with customizable length and character types.
- **Password Strength Checker:** Analyze the strength of a password and get suggestions for improvement.
- **Password Leak Checker:** Check if a password has appeared in known data breaches.

### Database Control
- **Database CRUD Operations:** Add, delete, update, and list records in the app's database. Manage your data and view operation history.

### Change Password
- **Change your current password:** Update your account password by entering your old password and confirming the new one for better security.

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Visual Studio 2022 or later (with MAUI workload installed)
- (Optional) Android/iOS emulators for mobile testing
 
### Build and Run

1. Clone the repository:
   ```sh
   git clone https://github.com/MrShiroLu/MauiNmap.git
   cd NmapMaui
   ```
2. Restore dependencies:
   ```sh
   dotnet restore
   ```
3. Build the project:
   ```sh
   dotnet build
   ```
4. Run the app for your desired platform:
   - Windows:
     ```sh
     dotnet run -f net9.0-windows10.0.19041.0
     ```
   - Android:
     ```sh
     dotnet run -f net9.0-android
     ```
   - iOS/MacCatalyst: (Requires macOS)
     ```sh
     dotnet run -f net9.0-ios
     dotnet run -f net9.0-maccatalyst
     ```

## Project Structure

```
NmapMaui/
├── Models/           # Data models (e.g., Hash, User)
├── Services/         # Business logic and services (cryptography, database, auth, etc.)
├── Views/            # UI pages (XAML)
│   ├── HashCalculatorPage.xaml
│   ├── Base64ToolPage.xaml
│   ├── ChangePasswordPage.xaml
│   ├── LoginPage.xaml
│   ├── RegisterPage.xaml
│   ├── DnsToolPage.xaml
│   ├── PingToolPage.xaml
│   └── ...
├── ViewModels/       # ViewModel classes for MVVM
├── Resources/        # Images, fonts, and styles
├── database/         # Local SQLite database
├── App.xaml          # Application definition
├── MauiProgram.cs    # App startup and DI
└── NmapMaui.csproj   # Project file
``` 
