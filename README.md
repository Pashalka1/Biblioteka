# 📚 Elektroniskā Bibliotēka API

ASP.NET Core 8 Web API ar EntityFramework, SQLite, Swagger un JWT autentifikāciju.

## 🗄️ Datubāzes struktūra (5 tabulas)

| Tabula | Apraksts |
|--------|----------|
| **Users** | Lietotāji: vārds, e-pasts, parole (bcrypt), loma (Reader/Librarian/Admin) |
| **Authors** | Autori: vārds, uzvārds, biogrāfija, dzimšanas gads |
| **Categories** | Kategorijas: Daiļliteratūra, Zinātne, Vēsture, Tehnoloģijas |
| **Books** | Grāmatas: nosaukums, ISBN, gads, eksemplāri → FK uz Author, Category |
| **Loans** | Aizdevumi: datums, termiņš, statuss → FK uz User, Book |

## 🔗 API Endpointi

### 🔐 Autentifikācija
| Metode | URL | Apraksts |
|--------|-----|----------|
| POST | `/api/auth/register` | Reģistrācija |
| POST | `/api/auth/login` | Pieslēgšanās → JWT tokens |

### 📖 Grāmatas
Metode | URL | Autorizācija
-------|-----|--------------
GET    | /api/books            | Publiski (atbalsta ?search= un ?categoryId=)
GET    | /api/books/{id}       | Publiski
POST   | /api/books            | Librarian, Admin
PUT    | /api/books/{id}       | Librarian, Admin
DELETE | /api/books/{id}       | Librarian, Admin

### 👤 Autori
| Metode | URL | Autorizācija |
|--------|-----|--------------|
| GET | `/api/authors` | Publiski |
| GET | `/api/authors/{id}` | Publiski |
| POST | `/api/authors` | Librarian, Admin |

### 🏷️ Kategorijas
| Metode | URL | Autorizācija |
|--------|-----|--------------|
| GET | `/api/categories` | Publiski |
| POST | `/api/categories` | Admin |

### 📋 Aizdevumi
| Metode | URL | Autorizācija |
|--------|-----|--------------|
| GET | `/api/loans` | Autentificēts (Readers redz tikai savus) |
| POST | `/api/loans` | Autentificēts |
| POST | `/api/loans/{id}/return` | Autentificēts |

## 🚀 Palaišana

```bash
# 1. Atjaunot pakotnes
dotnet restore

# 2. Palaist projektu
dotnet run

# 3. Atvērt Swagger UI
# http://localhost:5000
```

## 🔑 JWT Autentifikācija

1. Izsauc `POST /api/auth/login` ar:
   ```json
   { "email": "admin@library.lv", "password": "Admin123!" }
   ```
2. Saņem tokenu: `eyJhbGciOiJIUzI1NiIs...`
3. Swagger UI: klikšķini **Authorize** 🔒 → ievadi `Bearer <tokens>`
4. Tagad vari izsaukt aizsargātos endpointus!

## 🏗️ Projekta struktūra

```
LibraryAPI/
├── Program.cs              # App konfigurācija, middleware, DI
├── appsettings.json        # JWT, DB connection string
├── Models/
│   └── Models.cs           # User, Author, Category, Book, Loan
├── Data/
│   └── LibraryDbContext.cs # EF DbContext + seed dati
├── DTOs/
│   └── DTOs.cs             # Request/Response objekti
├── Services/
│   └── JwtService.cs       # JWT tokenu ģenerēšana
└── Controllers/
    ├── AuthController.cs           # /api/auth/*
    ├── BooksController.cs          # /api/books/*
    ├── AuthorsCategoriesController.cs # /api/authors/*, /api/categories/*
    └── LoansController.cs          # /api/loans/*
```

## 🛡️ Lomas

- **Reader** – var skatīt grāmatas, aizņemties, atgriezt savas grāmatas
- **Librarian** – var pievienot, rediģēt un dzēst grāmatas, pievienot autorus, redz visus aizdevumus
- **Admin** – pilna piekļuve, var pievienot kategorijas

## 📦 Izmantotās tehnoloģijas

- **ASP.NET Core 8** – Web API ietvars
- **EntityFramework Core 8** – ORM datu bāzei
- **SQLite** – Viegla relāciju datu bāze
- **Swashbuckle.AspNetCore** – Swagger/OpenAPI dokumentācija
- **Microsoft.AspNetCore.Authentication.JwtBearer** – JWT autentifikācija
- **BCrypt.Net-Next** – Paroļu hešošana
