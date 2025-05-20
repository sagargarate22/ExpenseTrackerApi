# 💸 Expense Tracker

A full-featured Expense Tracker Web API that allows users to manage their daily expenses, track spending by category, and receive email reports. Built using ASP.NET Core, Entity Framework, Hangfire, Razor views, and deployed with Azure Blob Storage for file handling.

---

## 🚀 Features

- 📝 Add, update, delete daily expenses
- 📊 Get monthly reports with category-wise breakdown (Pie Chart)
- 📅 Daily email summary at 9 PM (via Hangfire)
- 🧾 Razor HTML email templates
- 🔒 User authentication
- ☁️ File upload/download via Azure Blob Storage
- 📂 View/download reports as PDF
- 📤 Email with username, email, and total expense
- 📅 Monthly report with charts using Bootstrap & HTML

---

## 🧰 Tech Stack

| Layer            | Technology Used             |
|------------------|-----------------------------|
| Frontend         | HTML, Razor Views, Bootstrap |
| Backend          | ASP.NET Core Web API         |
| Database         | SQL Server                   |
| ORM              | Entity Framework Core        |
| Job Scheduling   | Hangfire                     |
| Email            | SMTP (Outlook) + Razor View  |
| File Storage     | Azure Blob Storage           |

---

## 📦 Folder Structure

ExpenseTracker/
│
├── Controllers/
├── Models/
├── Services/
├── Views/EmailTemplates/
├── Data/
├── Utilities/
├── wwwroot/
└── Program.cs


---

## 📬 Email Features

- **Daily Report at 9 PM:** Total expenses + HTML summary
- **Monthly Report:** 
  - Total expenses
  - Category-wise breakdown
  - Visualized with a Pie Chart
- Built using Razor views for clean, professional formatting

---

## 📄 PDF Generation

- View/download expenses in PDF format
- Files stored in Azure Blob and served via secure links

---

## 🛠️ Setup Instructions

1. **Clone the repository:**

```bash
git clone https://github.com/your-username/expense-tracker.git
cd expense-tracker
```
2. **Restore packages:**
```bash
dotnet restore
```

3. **Update appsettings.json with your credentials:**
```json
{
  "SmtpSettings": {
    "Server": "smtp.example.com",
    "Port": 587,
    "SenderName": "Your Name",
    "SenderEmail": "you@example.com",
    "Username": "you@example.com",
    "Password": "your-app-password"
  },
  "ConnectionStrings": {
    "DatabaseConnectionString": "your-db-connection-string",
    "AzureBlobStorage": "your-azure-blob-connection-string"
  },
  "BlobStorage": {
    "ConnectionString": "your-azure-connection-string",
    "ContainerName": "your-container"
  }
}
```

4. **Run the app:**
```bash
dotnet run
```

--- 
# 🗓️ Hangfire Dashboard
Accessible at:
http://localhost:5000/hangfire

View and manage scheduled jobs (daily/monthly email reports).

## 📌 Notes
- #### You must have valid SMTP settings (like Outlook) for email sending.

- #### Hangfire must be running to schedule and send emails on time.


---
# 🙋‍♂️ Author
**Sagar Garate**

**LinkedIn :** [LinkedIn](https://www.linkedin.com/in/sagar-garate-3573ab233)

**GitHub :** [GitHub](https://github.com/sagargarate22)