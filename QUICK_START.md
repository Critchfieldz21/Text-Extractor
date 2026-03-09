# 🚀 Quick Start Guide - Frontend/Backend Split

## 30-Second Setup

### Terminal 1 - Start Backend
```bash
cd Backend/TextExtractorBackend
dotnet run
```
✅ Will show: `Now listening on: http://localhost:5000`

### Terminal 2 - Start Frontend
```bash
cd Frontend/TextExtractorUI
dotnet run
```
✅ Will show: `Now listening on: http://localhost:3000`

### Browser
Open: **http://localhost:3000**

---

## What's Running?

| Component | URL | Role |
|-----------|-----|------|
| **Backend API** | http://localhost:5000 | Processes PDFs, Database |
| **Frontend UI** | http://localhost:3000 | Web Interface |
| **Database** | SQLite file | ShopTicket.db |

---

## File Structure

```
Project Root/
├── Backend/TextExtractorBackend/          ← Start: dotnet run
│   ├── Program.cs                         ← REST API configuration
│   ├── Services/                          ← Business logic
│   └── ShopTicket.db                      ← SQLite database
│
├── Frontend/TextExtractorUI/              ← Start: dotnet run
│   ├── Program.cs                         ← Blazor configuration
│   ├── Components/                        ← UI pages
│   └── appsettings.json                   ← Backend URL
│
└── Documentation/
    ├── QUICK_START.md                     ← This file
    ├── ARCHITECTURE_AND_SETUP.md
    └── ... (other guides)
```

---

## How It Works

```
You → Browser (http://localhost:3000)
        ↓
    Frontend (Blazor Server)
        ↓ HTTP
    Backend (REST API on port 5000)
        ↓
    PDF Processing, Database, YOLO Models
        ↓ HTTP Response
    Frontend displays results
```

---

## Common Commands

### Build Both
```bash
cd Backend/TextExtractorBackend && dotnet build
cd ../../Frontend/TextExtractorUI && dotnet build
```

### Build Only Backend
```bash
cd Backend/TextExtractorBackend && dotnet build
```

### Build Only Frontend
```bash
cd Frontend/TextExtractorUI && dotnet build
```

### Clean & Rebuild
```bash
# Backend
cd Backend/TextExtractorBackend
dotnet clean
dotnet build

# Frontend
cd ../../Frontend/TextExtractorUI
dotnet clean
dotnet build
```

---

## Ports

- **5000** - Backend HTTP
- **5001** - Backend HTTPS
- **3000** - Frontend HTTP
- **3001** - Frontend HTTPS

If ports are in use:
```bash
# Find what's using port 5000
lsof -i :5000

# Kill process
kill -9 <PID>
```

---

## Configuration

### Change Backend URL (Frontend)
Edit: `Frontend/TextExtractorUI/appsettings.json`
```json
{
  "BackendUrl": "http://localhost:5000"  ← Change here
}
```

---

## Logs

### Backend Logs
Look at console output where you ran `dotnet run`
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

### Frontend Logs
- Console output from `dotnet run`
- Browser console (Press F12, then Console tab)

---

## Database

SQLite database located at:
```
Backend/TextExtractorBackend/ShopTicket.db
```

To reset database:
```bash
rm Backend/TextExtractorBackend/ShopTicket.db
# It will be recreated on next run
```

---

## Features Supported

✅ Upload PDF files  
✅ Extract text automatically  
✅ YOLO model detection  
✅ Save results to database  
✅ Export as JSON  
✅ Export as CSV  
✅ View history  
✅ Cross-platform (macOS/Windows)  

---

## Troubleshooting

### Frontend won't start
- Check if port 3000 is available
- Check if backend is running
- Check appsettings.json BackendUrl

### Backend won't start
- Check if port 5000 is available
- Ensure ONNX models exist in Services/Models/
- Check if database file is accessible

### Can't connect frontend to backend
- Verify both are running
- Check appsettings.json BackendUrl
- Check browser console for errors (F12)
- Verify CORS is enabled in backend

### Database errors
- Delete ShopTicket.db and restart
- Check file permissions
- Ensure directory is writable

---

## Architecture

### Backend (REST API)
- Handles all PDF processing
- Manages SQLite database
- Runs YOLO models for detection
- Provides REST endpoints

### Frontend (Blazor Server)
- User interface only
- Communicates with backend via HTTP
- Handles file uploads
- Displays results

### They communicate via REST API
- Frontend makes HTTP requests
- Backend responds with JSON
- Completely independent applications

---

## Next Steps

1. ✅ Start both applications
2. ✅ Open http://localhost:3000
3. ✅ Upload a PDF file
4. ✅ Watch the processing
5. ✅ Download results

---

## Documentation

For more details, see:
- `ARCHITECTURE_AND_SETUP.md` - Full architecture guide
- `PROJECT_SPLIT_COMPLETE.md` - Comprehensive summary
- `MACOS_FIX_SUMMARY.md` - Platform compatibility

---

## That's It! 🎉

You now have a fully functional frontend/backend application!

Questions? Check the log output - it usually tells you exactly what's wrong.
