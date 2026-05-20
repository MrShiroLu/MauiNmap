# NmapMaui.Api

Minimal API katmanı (proje ön raporundaki taahhüt). Komutları sunucu tarafında çalıştırıp sonuçları Microsoft SQL Server'a (EF Core) kaydeder.

## Çalıştırma

```bash
cd NmapMaui.Api
dotnet run
```

API varsayılan olarak `http://localhost:5000` üzerinde dinler.

## Endpoint'ler

| Yol | Method | Açıklama |
|---|---|---|
| `/` | GET | Sağlık kontrolü |
| `/nmap/scan` | POST | `{ host, startPort, endPort }` |
| `/gobuster/scan` | POST | `{ url, wordlist }` |
| `/logs` | GET | Son 200 tarama logu |

MAUI istemcisi şu an Process'leri yerel olarak çalıştırıyor; bu API'yi `HttpClient` ile çağıracak şekilde geçirmek gelecek refactor.
