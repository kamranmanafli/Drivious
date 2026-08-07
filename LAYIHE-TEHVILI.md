# Drivious — layihə təhvili

Bu sənəd layihənin hazırkı vəziyyətini izah edir: nə işləyir, harada yerləşir,
necə işə salınır və nə dəyişdirilib. 5 avqust 2026.

---

## 1. Layihə nədir

Filo idarəetmə sistemi. Bir backend, bir frontend, iki fərqli interfeys.

| Hissə | Texnologiya |
|---|---|
| Backend | ASP.NET Core 8 · EF Core · SQL Server · Identity + JWT |
| Frontend | React 18 · TypeScript · Vite 6 · Tailwind 4 · TanStack Query |

**Bir layihə, iki qabıq.** Login-də rol hansının açılacağını seçir:

| Qabıq | Rollar | Forma |
|---|---|---|
| Konsol | Administrator, Menecer | Masaüstü: sidebar, cədvəllər, filtrlər |
| Sürücü app | Sürücü | Mobil: aşağı naviqasiya, kartlar |
| `/admin/*` | **yalnız Administrator** | Menecerə görünmür |

İnterfeys dili Azərbaycan dilidir.

**Konsol səhifələri:** İdarə paneli · Maşınlar (+ detal səhifəsi) · Sürücülər ·
Təyinatlar · Gəlirlər · Xərclər · Yanacaq · Servis · Sığorta · Sənədlər ·
Bildirişlər · Arxiv · İstifadəçilər (yalnız admin)

**Sürücü app:** Ana səhifə · Qazancım · Filo (oxu) · Bildirişlər · Profil

---

## 2. Canlı ünvanlar

| | |
|---|---|
| Sayt | https://drivious-web.vercel.app |
| API | http://drivious.runasp.net |
| Hostinq | Frontend → Vercel · Backend + MSSQL → MonsterASP.NET |
| Qiymət | Hər ikisi pulsuz, kart tələb olunmur |

### Giriş məlumatları

| İstifadəçi | Şifrə | Rol |
|---|---|---|
| `admin` | `Drivious#Adm2026!` | Administrator |
| `menecer` | `Menecer123!` | Menecer |

**Dəvət kodu:** `DRIVIOUS-2026-XK7Q`

> ⚠️ Bu sənəddə şifrələr var. Açıq yerdə paylaşma.

---

## 3. Qeydiyyat necə işləyir

Qeydiyyat səhifəsində (`/register`) **rol seçilir**:

| Rol | Şərt |
|---|---|
| Sürücü | Sərbəst — heç nə lazım deyil |
| Menecer | Dəvət kodu tələb olunur |
| Administrator | Dəvət kodu tələb olunur |

Rol seçiləndə "Dəvət kodu" xanası avtomatik açılır; Sürücüdə görünmür.

Kod backend-də `Registration:InviteCode` açarındadır. **Kod hesab yaradılmazdan
əvvəl yoxlanılır** — səhv kodda bazada boş hesab qalmır. Kod konfiqurasiyada boş
olsa, Administrator və Menecer qeydiyyatı tamamilə bağlanır (təhlükəsiz default).

Bundan əlavə, admin `/admin/users` səhifəsindən **"Yeni istifadəçi"** düyməsi ilə
istənilən rolda hesab yarada bilər.

---

## 4. Lokal işə salmaq

### Backend

Lazımdır: .NET 8 SDK və işlək SQL Server.

Sirlər `appsettings.json`-da **saxlanılmır**. Hər mühitdə ayrıca verilir:

```bash
dotnet user-secrets set "ConnectionStrings:default" "<connection string>"
dotnet user-secrets set "Jwt:Key" "<uzun təsadüfi açar>"
dotnet user-secrets set "Seed:AdminUserName" "admin"
dotnet user-secrets set "Seed:AdminEmail" "admin@drivious.az"
dotnet user-secrets set "Seed:AdminPassword" "<şifrə>"
dotnet user-secrets set "Registration:InviteCode" "<dəvət kodu>"
```

Sonra:

```bash
dotnet run
```

Açılır: <http://localhost:5221> · Swagger: `/swagger`

Miqrasiyalar **start-da avtomatik tətbiq olunur** — boş bazaya ayrıca əmr vermək
lazım deyil. Admin hesabı da start-da seed edilir.

### Frontend

```bash
cd drivious-web
npm install
npm run dev
```

Açılır: <http://localhost:5173> — bu origin artıq API-nin `Cors:AllowedOrigins`
siyahısındadır. Portu dəyişsən, həmin origin-i də oraya əlavə etməlisən.

`drivious-web/.env`:

```
VITE_API_URL=http://localhost:5221
```

| Əmr | Nə edir |
|---|---|
| `npm run dev` | Development server |
| `npm run build` | Tip yoxlaması + `dist/` |
| `npm run typecheck` | Yalnız tip yoxlaması |

### macOS qeydi

SQL Server-in macOS versiyası yoxdur. Mac-da Docker ilə `azure-sql-edge`
konteyneri işlədilir (arm64 üçün yeganə işlək variant). Windows-da bu lazım deyil.

---

## 5. Deploy necə olunur

### Frontend → Vercel

```bash
cd drivious-web
vercel deploy --prod
```

### Backend → MonsterASP

```bash
dotnet publish -c Release -o ./publish
```

Çıxan faylları zip-lə → MonsterASP paneli → **Websites → Manage → File Manager**
→ **wwwroot** qovluğuna yüklə → **Unzip** (root-a yox, məhz `wwwroot`-a).

Prod sirləri `appsettings.Production.json` faylındadır (user-secrets serverdə
işləmir). Bu fayl publish çıxışının içindədir.

---

## 6. Vacib: HTTPS və proksi

MonsterASP-in **pulsuz planında HTTPS yoxdur** (support ticket tələb edir).
Vercel isə `https`-dədir. Brauzer `https` səhifədən `http` API-yə sorğu
göndərməyə icazə vermir (mixed content bloku).

**Həll:** Vercel proksi kimi işləyir. `drivious-web/vercel.json`-da:

```
/api/*     →  http://drivious.runasp.net/api/*
/Images/*  →  http://drivious.runasp.net/Images/*
/Files/*   →  http://drivious.runasp.net/Files/*
```

Nəticədə brauzer yalnız Vercel origin-i ilə danışır. Buna görə
`.env.production`-da `VITE_API_URL` **qəsdən boşdur** — sorğular öz origin-inə
gedir.

Bunun ikinci faydası: canlı saytda **CORS ümumiyyətlə iştirak etmir**, çünki
sorğular artıq cross-origin deyil.

⚠️ `vercel.json`-da SPA catch-all qaydası (`/((?!assets/).*)` → `index.html`)
proksi qaydalarından **sonra** gəlməlidir. Əks halda `/api`-ni udur.

---

## 7. Soyuq start

Pulsuz hostinq planında sayt boşdayanda söndürülür. Ondan sonrakı ilk sorğu
tətbiqi oyadır və 10–30 saniyə çəkə bilər. Bu qüsur deyil, planın davranışıdır.

İstiləşəndən sonra ölçdüyümüz cavab vaxtları: **0.3–0.4 saniyə**.

Buna görə frontend-də:

- Sorğu 4 saniyəni keçəndə **"Server oyanır…"** bildirişi çıxır
- Timeout 60 saniyədir (soyuq startı kəsməsin deyə uzun seçilib)
- Bildiriş skaneri start-dan 20 saniyə sonra işə düşür ki, ilk istifadəçinin
  sorğusu ilə yarışmasın

---

## 8. Bu təhvildə nə dəyişdi

### Əlavə olunanlar

| Nə | Harada |
|---|---|
| Qeydiyyat səhifəsi | `drivious-web/src/app/RegisterPage.tsx` |
| Rol seçimi + dəvət kodu | `RegisterDTO`, `AuthService.ResolveRegistrationRole` |
| Admin "Yeni istifadəçi" | `drivious-web/src/app/admin/UsersPage.tsx` |
| Ortaq giriş qabığı | `drivious-web/src/app/AuthShell.tsx` |
| Vercel konfiqurasiyası | `drivious-web/vercel.json` |

Əvvəl `authApi.register` kodda var idi, amma onu **heç bir səhifə çağırmırdı** —
yəni qeydiyyat interfeysi ümumiyyətlə yox idi.

### Düzəldilənlər

- **Demo rejimi tamamilə silindi** (`src/api/demo/`, `VITE_DEMO`, login-dəki demo
  hesab paneli, layout-lardakı "Demo" nişanı). Artıq tək yol real API-dir.
- `UseHttpsRedirection()` yalnız Development-dən kənarda işləyir. Əvvəl şərtsiz
  idi və CORS preflight-a 307 qaytarırdı — preflight yönləndirməyə getmədiyi üçün
  başqa maşındakı frontend heç bir sorğu göndərə bilmirdi.
- Xəta mesajı `VITE_API_URL` boş olanda "Serverə qoşulmaq mümkün olmadı ()"
  şəklində çıxırdı.
- Timeout xətası tərcümə olunmurdu (ingiliscə `timeout of 60000ms exceeded`).
- `*.tsbuildinfo` build artefaktları repodan çıxarıldı.

### API kontraktı yoxlanıldı

Frontend-in çağırdığı **hər** endpoint backend-lə tutuşduruldu: 19 route + HTTP
metodu, 10 resurs adı, bütün filtr parametrləri, `sortBy` açarları və yaratma/
yeniləmə gövdələrinin sahə adları. Yeganə uyğunsuzluq `GET /api/auths/users`
idi — frontend onu çağırırdı, backend-də yox idi. İndi mövcuddur.

---

## 9. Bilinməli məhdudiyyətlər

1. **Bildirişlər qlobaldır** — istifadəçiyə ünvanlanmır, hamı eyni siyahını görür
   və "oxundu" hamıya təsir edir. Hər istifadəçiyə ayrı bildiriş lazımdırsa, bu
   backend işidir (`Notification`-a `UserId` + filtr).
2. **İdarə paneli yalnız Menecer və Administrator üçündür.** Sürücünün ana
   səhifəsi öz siyahılarından hesablanır.
3. **Maşın və sürücü yaradarkən şəkil məcburidir** (`NotNull` validator).
4. **Sürücü rolundakı hesab** sürücü kartına bağlanmayana qədər heç bir gəlir və
   təyinat görmür. Bağlantı `/admin/users` səhifəsindən verilir.
5. Seçicilər (`components/pickers.tsx`) API-nin maksimum səhifə ölçüsü ilə
   (100) məhdudlaşır. 100-dən çox maşın/sürücü olarsa, axtarışlı variantla
   əvəzlənməlidir.

---

## 10. Fayl strukturu

```
Drivious/
  Controllers/     HTTP endpoint-lər, rol atributları
  Services/        Biznes qaydaları
    Background/    Bildiriş skaneri
  Data/            DbContext, seed
  Models/          EF entity-ləri
  DTOs/            Sorğu və cavab formaları
  Validators/      FluentValidation qaydaları
  Migrations/      EF miqrasiyaları

  drivious-web/    ← əsl frontend
    src/api/       Klient, endpoint-lər, tiplər, mesaj tərcüməsi
    src/app/       Ekranlar (console / admin / driver)
    src/ui/        Dizayn sistemi
    vercel.json    Proksi + SPA route qaydaları

  drivious-frontend/   köhnə prototip — işlək tətbiqin hissəsi DEYİL
```

---

## 11. Növbəti addımlar üçün ideyalar

- MonsterASP-ə support ticket açıb pulsuz HTTPS istəmək — alınsa, Vercel
  proksisini saxlamaq da olar, birbaşa qoşulmaq da
- Nümunə data doldurmaq (maşın, sürücü, gəlir/xərc) ki, İdarə paneli boş
  görünməsin
- Bildirişləri istifadəçiyə bağlamaq
