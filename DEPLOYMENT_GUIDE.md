# Full Deployment Guide - BillColl_Mail Project

## 📦 Complete Developer Deployment Process (Start → Finish)

---

## STEP 1: DATABASE SETUP ⭐ START HERE

### Create PostgreSQL Database

```bash
# Connect to PostgreSQL admin
psql -U postgres -d postgres

# Execute in psql:
CREATE DATABASE billcoll_mail;
\c billcoll_mail;

# Run schema migration (create all tables)
\i others/BCAT_09252026_stage_postgresql.sql

# OPTIONAL: Load test seed data
\i others/seed_test_data.sql

# Verify setup
SELECT COUNT(*) FROM BillCollUser; -- should show 3 users
```

---

## STEP 2: BUILD BACKEND SERVICE

```bash
cd backend-api/

# Restore .NET dependencies
dotnet restore

# Build the API
dotnet build

# Run migrations (EF Core)
dotnet ef database update \
  --project src/Migrations \
  --startup-project src/API.BillColl.Mail

# Optional: Watch mode for development
dotnet run --project src/API.BillColl.Mail
```

**Environment Variables Required:**
```env
# In backend-api/.env or docker-compose.override.yml
BCAT__Mail__MailerSettings__SmtpServer=smtp.gmail.com
BCAT__Mail__MailerSettings__Port=587
BCAT__Mail__MailerSettings__EnableSsl=True
BCAT__Mail__MailerSettings__EmailId=your-email@gmail.com
BCAT__Mail__MailerSettings__Password=your-app-password

# Database connection
BCAT__Mail__DatabaseSettings__ConnectionString=Host=localhost;Database=billcoll_mail;Username=postgres;Password=yourpassword;Include Error Detail=True

# Twilio for SMS
BCAT__Mail__SmsServiceSettings__AccountSid=your-twilio-account-sid
BCAT__Mail__SmsServiceSettings__AuthToken=your-twilio-auth-token
BCAT__Mail__SmsServiceSettings__FromPhoneNumber=+1234567890

# SAP Business One connection
BCAT__Mail__DbiSettings__ConnectionId=your-sap-connection-id
```

---

## STEP 3: BUILD FRONTEND APPLICATION

```bash
cd frontend-app/

# Install Node dependencies
npm install

# Optional: Run migrations for frontend database
npx prisma migrate deploy

# Start development server
npm run dev

# Or build for production
npm run build
npm run preview
```

---

## STEP 4: CONTAINER DEPLOYMENT (Alternative)

### Deploy with Docker Compose

```bash
cd .docker/

# Build images and start all services
docker-compose up -d --build

# Verify containers running
docker-compose ps

# View logs
docker-compose logs -f backend-api
docker-compose logs -f frontend-app

# Stop services
docker-compose down
```

**Docker Compose Services:**
- `backend-api` → .NET 8 API (port 7000)
- `frontend-app` → React app (port 3000)
- `db` → PostgreSQL (port 5432)
- `redis` → Redis caching

---

## STEP 5: CONFIGURE EXTERNAL SERVICES

### Email Provider Setup

**Gmail/SMTP:**
1. Enable 2FA on Gmail account
2. Generate App Password (not your regular password!)
3. Add to `BCAT__Mail__MailerSettings__Password`

**Amazon SES (Optional):**
```bash
aws ses verify-email-address --email-address your-email@domain.com
```

### Twilio SMS Setup

1. Sign up at https://www.twilio.com
2. Get Account SID & Auth Token from dashboard
3. Configure `From PhoneNumber` for sender ID
4. Set environment variables in backend-api/.env

### SAP Business One Integration

Ensure connection string and credentials are configured:
```
BCAT__Mail__DbiSettings__ConnectionId=<your-sap-connection>
BCAT__Mail__SmsServiceSettings__Enabled=True
```

---

## STEP 6: RUN DATABASE SEED DATA (Optional)

```bash
cd backend-api/

# Load sample data for testing
dotnet ef database update \
  --project src/Data \
  --startup-project src/API.BillColl.Mail

# Or use SQL scripts directly:
psql -U postgres -d billcoll_mail -f others/seed_test_data.sql
```

**Sample data includes:**
- 3 BillCollUsers (admin, user1, user2)
- 50+ BillCollLedgers (billing records)
- Client contact information
- Email/SMS templates

---

## STEP 7: VERIFY DEPLOYMENT

### Backend API Health Check
```bash
curl http://localhost:7000/api/v1/health
# Expected: {"status":"OK","message":"BillColl Mailer Service is running"}
```

### Test Endpoints
```bash
# Get all users
curl http://localhost:7000/api/v1/users

# Send test email (requires authentication)
curl -X POST http://localhost:7000/api/v1/emails \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"to":"test@example.com","template":"invoice","data":{"id":1}}'
```

### Frontend Access
Open browser: `http://localhost:3000` or `https://your-domain.com`

---

## STEP 8: CONFIGURE PRODUCTION DNS

### Update Docker Compose for Production

```yaml
# .docker/docker-compose.prod.yml (example)
version: '3.8'
services:
  backend-api:
    build:
      context: ../../backend-api
      dockerfile: ../Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    ports:
      - "7001:7000"  # Different port for prod

  frontend-app:
    build:
      context: ../../frontend-app
      dockerfile: ../Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    ports:
      - "3001:3000"
```

### Environment Variables (Production)

Create `.env.production` in backend-api/:
```env
ASPNETCORE_ENVIRONMENT=Production
BCAT__Mail__MailerSettings__SmtpServer=your-smtp.provider.com
BCAT__Mail__MailerSettings__Port=587
# Add all production credentials here
BCAT__Mail__DatabaseSettings__ConnectionString=Host=prod-db.example.com;...
```

### Set Up Reverse Proxy (Nginx)

```bash
nginx -c /etc/nginx/sites-available/billcoll-mail.conf
ln -s /etc/nginx/sites-available/billcoll-mail.conf \
    /etc/nginx/sites-enabled/billcoll-mail
systemctl restart nginx
```

**Nginx Configuration Example:**
```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    location /api {
        proxy_pass http://localhost:7000;
    }
}
```

---

## STEP 9: DEPLOYMENT CHECKLIST ✅

Before going live:

- [ ] PostgreSQL database created and migrated
- [ ] Backend API builds without errors
- [ ] Frontend application builds successfully
- [ ] Email service tested (send test emails)
- [ ] SMS service configured (Twilio credentials valid)
- [ ] SAP Business One connection verified (if using)
- [ ] All environment variables set in production
- [ ] Database backups configured
- [ ] SSL/TLS certificates installed for domain
- [ ] CORS policies configured correctly
- [ ] Rate limiting enabled on API
- [ ] Monitoring/logging setup (Application Insights, etc.)

---

## 🔄 REBUILD & RERUN COMMANDS

**Full rebuild from scratch:**
```bash
# 1. Delete old containers/volumes (Docker)
docker-compose -f .docker/docker-compose.yml down -v

# 2. Build fresh images
docker-compose -f .docker/docker-compose.yml build --no-cache

# 3. Start all services
docker-compose -f .docker/docker-compose.yml up -d

# 4. Verify deployment
curl http://localhost:7000/api/v1/health
```

---

## 📊 SYSTEM MONITORING

### Check Service Health
```bash
docker-compose ps
docker-compose logs --tail=50 backend-api
```

### Database Connection Test
```sql
SELECT 'Database connected' as status, getdate() as time FROM BillCollUsers LIMIT 1;
```

### API Metrics Endpoint (if configured)
```bash
curl http://localhost:7000/api/v1/metrics
```

---

## 🐛 TROUBLESHOOTING

### "Cannot connect to database"
- Verify PostgreSQL is running: `docker-compose ps | grep db`
- Check connection string in environment variables
- Test with `psql -h localhost -U postgres -d billcoll_mail`

### "Email not sending"
- Confirm SMTP credentials are correct (use app password, not regular password)
- Check firewall rules for port 587/465
- Verify Gmail "Less secure apps" disabled → use App Passwords instead

### "Frontend can't connect to API"
- Check CORS settings in backend-api
- Verify proxy configuration in Nginx
- Check browser console for specific errors

---

## 📞 SUPPORT CONTACTS

**Internal Teams:**
- SAP Business One: [your-sap-support@domain.com]
- Twilio Support: https://support.twilio.com
- AWS SES Support: https://console.aws.amazon.com/support/home

**Documentation:**
- API Docs: Swagger at `/swagger` or OpenAPI spec
- Frontend Docs: Check frontend-app/README.md
- Docker Compose: `.docker/docker-compose.yml`

---

## 📝 NEXT STEPS AFTER DEPLOYMENT

1. **User Accounts**: Register real users through admin panel
2. **Template Customization**: Update email/SMS templates in database
3. **Scheduling**: Configure auto-billing schedules
4. **Monitoring**: Set up alerts for failed sends
5. **Backups**: Configure PostgreSQL automated backups
6. **CI/CD**: Set up GitHub Actions/GitLab CI for automated builds

---

**Deployment Complete!** 🎉

The BillColl_Mail system is now running and ready to send billing statements via email and SMS.
