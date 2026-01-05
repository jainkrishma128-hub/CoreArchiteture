# Running the Application

## Quick Start - Run Both Projects

To run both the Web application and API together, use the provided PowerShell script:

```powershell
cd "d:\My Archi\Core"
.\run-both.ps1
```

This will:
1. Start the **API** at `http://localhost:5089` in a new window
2. Start the **Web App** at `http://localhost:5000` in a new window

**Note:** The Web application requires the API to be running for all CRUD operations to work.

---

## Manual Startup (Alternative)

If you prefer to run them manually:

### Terminal 1 - Start API
```powershell
cd "d:\My Archi\Core\src\CommonArchitecture.API"
dotnet run
```

### Terminal 2 - Start Web
```powershell
cd "d:\My Archi\Core\src\CommonArchitecture.Web"
dotnet run
```

---

## Access the Applications

- **Web Application**: http://localhost:5000
- **API Swagger**: http://localhost:5089/swagger

---

## Architecture

```
┌─────────────────┐
│   Web Browser   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐      HTTP Requests      ┌─────────────────┐
│   Web App       │─────────────────────────▶│   API Server    │
│  (Port 5000)    │                          │  (Port 5089)    │
└─────────────────┘                          └────────┬────────┘
                                                      │
                                                      ▼
                                              ┌─────────────────┐
                                              │   SQL Server    │
                                              │   Database      │
                                              └─────────────────┘
```

The Web application acts as the frontend and makes HTTP calls to the API for all data operations.

---

## Troubleshooting

### Port Already in Use
If you get an error that a port is already in use:

**Option 1:** Kill the process using the port
```powershell
# Find process on port 5089 (API)
netstat -ano | findstr :5089

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F
```

**Option 2:** Change the port in configuration files
- API: Edit `src\CommonArchitecture.API\Properties\launchSettings.json`
- Web: Edit `src\CommonArchitecture.Web\appsettings.json` (update `ApiSettings:BaseUrl`)

### Products Not Saving
If products aren't saving in the Web app:
1. Verify the API is running at `http://localhost:5089`
2. Check the browser console for errors
3. Verify the API URL in `appsettings.json` matches the running API port

---

## Development Tips

- Use the `run-both.ps1` script for quick testing
- Each project runs in its own PowerShell window for easy monitoring
- Check console output in each window for errors
- API Swagger UI is useful for testing endpoints directly
