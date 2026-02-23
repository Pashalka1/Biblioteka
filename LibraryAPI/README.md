# 📚 Library API

Pilnīga bibliotēkas pārvaldības REST API, izstrādāta ar ASP.NET Core 8, Entity Framework Core, JWT autentifikāciju un Swagger dokumentāciju.

---

## 🗂️ Projekta struktūra

```
LibraryAPI/
├── Controllers/
│   ├── AuthController.cs        # POST /api/auth/register, POST /api/auth/login
│   ├── BooksController.cs       # GET/POST /api/books
│   ├── AuthorsGenresController.cs # GET/POST /api/authors, /api/genres
│   └── BorrowsController.cs     # GET/POST /api/borrows, POST /api/borrows/{id}/return
├── Models/
│   └── Models.cs                # User, Author, Book, Genre, Borrow
├── DTOs/
│   └── DTOs.cs                  # Request/Response objekti
├── Data/
│   └── LibraryDbContext.cs      # EF Core konteksts ar seed datiem
├── Services/
│   └── JwtService.cs            # JWT tokena ģenerēšana
├── Program.cs                   # Lietotnes konfigurācija
├── appsettings.json             # DB un JWT iestatījumi
└── LibraryAPI.csproj
```

---

## 🗄️ Datubāzes shēma (4 tabulas + relācijas)

```
Users          Authors
─────────      ─────────────
Id (PK)        Id (PK)
Name           FullName
Email (unique) Country
PasswordHash   BirthYear
Role           
               ↓ 1:N
Genres    →  Books
─────────     ──────────────
Id (PK)       Id (PK)
Name          Title
Description   ISBN (unique)
              PublishedYear
              TotalCopies
              AvailableCopies
              AuthorId (FK)
              GenreId (FK)
              ↓ 1:N
Borrows
──────────────────────
Id (PK)
UserId (FK → Users)
BookId (FK → Books)
BorrowedAt
DueDate
ReturnedAt
Status (Active/Returned/Overdue)
```

---

## 🚀 Palaišana

### Prasības
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)

### Soļi

```bash
# 1. Klonē/iegūsti projektu, pāreji uz mapi
cd LibraryAPI

# 2. Atjauno NuGet pakotnes
dotnet restore

# 3. Palaid projektu
dotnet run
```

Atvērt pārlūkā: **http://localhost:5000** → automātiski atvērsies Swagger UI

---

## 📋 API Endpointi

### 🔐 Autentifikācija
| Metode | URL | Apraksts | Auth |
|--------|-----|----------|------|
| POST | /api/auth/register | Reģistrē jaunu lietotāju | ❌ |
| POST | /api/auth/login | Autorizācija, atgriež JWT | ❌ |

### 📗 Grāmatas
| Metode | URL | Apraksts | Auth |
|--------|-----|----------|------|
| GET | /api/books | Visu grāmatu saraksts | ❌ |
| GET | /api/books/{id} | Grāmata pēc ID | ❌ |
| POST | /api/books | Pievienot grāmatu | 🔑 Admin |

### ✍️ Autori
| Metode | URL | Apraksts | Auth |
|--------|-----|----------|------|
| GET | /api/authors | Visu autoru saraksts | ❌ |
| GET | /api/authors/{id} | Autors pēc ID | ❌ |
| POST | /api/authors | Pievienot autoru | 🔑 Admin |

### 🏷️ Žanri
| Metode | URL | Apraksts | Auth |
|--------|-----|----------|------|
| GET | /api/genres | Visu žanru saraksts | ❌ |
| POST | /api/genres | Pievienot žanru | 🔑 Admin |

### 📦 Izsniegumi
| Metode | URL | Apraksts | Auth |
|--------|-----|----------|------|
| GET | /api/borrows | Izsniegumu saraksts | 🔑 User/Admin |
| POST | /api/borrows | Izsniedz grāmatu | 🔑 User |
| POST | /api/borrows/{id}/return | Atgriez grāmatu | 🔑 User/Admin |

---

## 🔑 JWT Autentifikācija - kā izmantot

### 1. Reģistrācija
```json
POST /api/auth/register
{
  "name": "Jānis Bērziņš",
  "email": "janis@example.lv",
  "password": "parole123"
}
```

### 2. Login → saņem tokenu
```json
POST /api/auth/login
{
  "email": "janis@example.lv",
  "password": "parole123"
}
// Atbilde:
{
  "token": "eyJhbGci...",
  "name": "Jānis Bērziņš",
  "role": "User"
}
```

### 3. Izmantot tokenu pieprasījumos
```
Authorization: Bearer eyJhbGci...
```

### Admin izveidošana
Datubāzē manuāli nomainīt lietotāja Role uz "Admin":
```sql
UPDATE Users SET Role = 'Admin' WHERE Email = 'tavs@email.lv';
```

---

## 🧪 Swagger testēšana

1. Atver http://localhost:5000
2. Izpildi **POST /api/auth/login** → kopē `token` vērtību
3. Noklikšķini uz pogas **Authorize** (augšā pa labi)
4. Ievadi: `Bearer eyJhbGci...` (ar "Bearer " priekšā)
5. Tagad vari testēt visus aizsargātos endpointus

---

## 🏗️ Tehnoloģijas

| Tehnoloģija | Versija | Izmantojums |
|-------------|---------|-------------|
| ASP.NET Core | 8.0 | Web framework |
| Entity Framework Core | 8.0 | ORM, datubāze |
| SQLite | - | Datubāze (viegli palaist) |
| JWT Bearer | 8.0 | Autentifikācija |
| Swashbuckle | 6.5 | Swagger dokumentācija |
| BCrypt.Net | 4.0 | Paroļu šifrēšana |

---

## 📐 JWT Tokena struktūra

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9   ← Header (algoritms)
.
eyJzdWIiOiIxIiwiZW1haWwiOiIuLi4ifQ      ← Payload (lietotāja dati)
.
SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV      ← Signature (verifikācija)
```

Payload satur: UserId, Email, Name, Role, iat, exp
