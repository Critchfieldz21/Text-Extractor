# Vercel Deployment Guide

## Prerequisites

Before deploying to Vercel, ensure:
- ✅ Code is pushed to GitHub (main branch)
- ✅ Both Frontend and Backend build locally without errors
- ✅ Environment variables are configured

## Architecture Overview

```
┌─────────────────┐
│   Vercel CDN    │
│   (Frontend)    │
│  :3000 → :5000  │
└────────┬────────┘
         │ HTTPS
         ▼
┌─────────────────┐
│  Vercel Serverless│
│  (Backend API)  │
│  processes PDF  │
│  stores DB      │
└─────────────────┘
```

## Step 1: Push to GitHub

```bash
cd /Users/zacharycritchfield/Desktop/untitled\ folder/Text-Extractor
git add .
git commit -m "Refactor: Decouple Frontend/Backend for Vercel deployment"
git push origin main
```

## Step 2: Configure Environment Variables

You'll need to set these in Vercel after creating the projects:

### Backend Environment Variables
- **No additional required** (uses defaults)
- Optional: `FrontendUrl` - Frontend URL (auto-detected by Vercel)

### Frontend Environment Variables
- **`BackendUrl`** - Set to your Backend deployment URL
  - Example: `https://text-extractor-backend.vercel.app`

## Step 3: Deploy Backend First

1. Go to https://vercel.com/new
2. Select **GitHub** and choose your repository
3. Configure project:
   - **Framework**: Other/None (it's .NET)
   - **Build Command**: `cd Backend/TextExtractorBackend && dotnet publish -c Release -o out`
   - **Output Directory**: `Backend/TextExtractorBackend/out`
   - **Install Command**: `cd Backend/TextExtractorBackend && dotnet restore`

4. Add environment variables (if needed):
   - Leave as defaults for now

5. Click **Deploy**
6. Wait for deployment to complete
7. Note the Backend URL (e.g., `text-extractor-backend.vercel.app`)

## Step 4: Deploy Frontend

1. Go to https://vercel.com/new
2. Select **GitHub** and choose your repository again
3. Configure project:
   - **Framework**: Other/None (it's .NET Blazor)
   - **Build Command**: `cd Frontend/TextExtractorUI && dotnet publish -c Release -o out`
   - **Output Directory**: `Frontend/TextExtractorUI/out`
   - **Install Command**: `cd Frontend/TextExtractorUI && dotnet restore`

4. Add environment variables:
   - `BackendUrl`: `https://<your-backend-url>`
     - Example: `https://text-extractor-backend.vercel.app`

5. Click **Deploy**

## Step 5: Test the Deployment

1. Open your Frontend URL in browser
2. Upload a test PDF
3. Check that it processes correctly
4. Verify data appears in History

## Important Notes

### Database
- SQLite database file will be created in `/tmp` on each Vercel container
- Data persists **only during the current container instance**
- When container restarts, database resets
- **To persist data long-term**: Migrate to PostgreSQL or MongoDB

### File Uploads
- Max file size: 10 MB (Vercel limit)
- Processing timeout: 60 seconds (Vercel limit)
- PDFs larger than this will fail

### CORS
- Already configured to accept requests from any origin
- Safe for production

## Troubleshooting

### Backend returns 404
- Check `BackendUrl` environment variable in Frontend
- Ensure Backend is deployed and running

### PDF upload fails
- Check file size (max 10 MB)
- Check PDF format (must be valid shop ticket)
- Check browser console for errors

### Data not persisting
- This is expected! SQLite doesn't persist on Vercel
- Migrate to cloud database to fix

## Next Steps (Optional)

### Use Cloud Database Instead of SQLite

1. **PostgreSQL** (Recommended)
   ```
   Update database connection in Backend
   Run migrations on cloud database
   Update connection string in appsettings
   ```

2. **MongoDB**
   ```
   Install MongoDB driver
   Update data access layer
   ```

3. **Azure SQL Database**
   ```
   Create Azure SQL instance
   Update connection string
   Run migrations
   ```

## Quick Deployment Checklist

- [ ] Code pushed to GitHub
- [ ] Verified local builds work
- [ ] Backend environment ready
- [ ] Frontend environment ready
- [ ] Backend deployed to Vercel
- [ ] Note Backend URL
- [ ] Frontend deployed with BackendUrl set
- [ ] Tested file upload
- [ ] Tested history view
- [ ] Tested export functionality

## Current Status

✅ Architecture refactored for Vercel
✅ Frontend and Backend decouple
✅ HTTP API communication working
✅ Local testing complete
⏳ Ready for Vercel deployment

---

**Questions?** Check the other documentation files:
- `QUICK_REFERENCE.md` - Quick commands
- `VERCEL_TROUBLESHOOTING.md` - Common issues
- `ARCHITECTURE_REFACTOR.md` - Architecture details
