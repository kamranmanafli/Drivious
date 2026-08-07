# Drivious — frontend təhvili

Bu sənəd `drivious-web/` qovluğundakı yeni React frontend-i və backend-də edilən
bir dəyişikliyi izah edir. Kod arxivin içindədir; bu fayl onu necə işə salmaq və
nəyə diqqət etmək lazım olduğunu deyir.

---

## 1. Nə qurulub

`drivious-web/` — 61 fayl, ~12 500 sətir TypeScript.
Vite 6 · React 18 · TypeScript · Tailwind CSS 4 · TanStack Query · Radix · Recharts.
İnterfeys dili Azərbaycan dilidir.

**Tək layihə, iki ayrı interfeys.** Login-də rol hansının açılacağını seçir:

| Qabıq | Rollar | Forma |
|---|---|---|
| Konsol | Admin, Manager | Masaüstü: sidebar, cədvəllər, filtrlər |
| Sürücü app | Driver | Mobil: aşağı naviqasiya, kartlar |
| `/admin/*` | **yalnız Admin** | Menecerə görünmür |

### Konsol səhifələri
Dashboard · Maşınlar (+ 7 tablı detal səhifəsi) · Sürücülər · Təyinatlar ·
Gəlirlər · Xərclər · Yanacaq · Servis · Sığorta · Sənədlər · Bildirişlər · Arxiv

### Admin bölməsi
İstifadəçilər (rol vermə, sürücü kartına bağlama) · Arxivdə həmişəlik silmə

### Sürücü app-ı
Ana səhifə · Qazancım · Filo (oxu) · Bildirişlər · Profil

Hər siyahı backend-in **dəqiq** dəstəklədiyi filtr, sıralama və səhifələmə
dəstini istifadə edir — nə az, nə çox.

---

## 2. İşə salmaq

```bash
cd drivious-web
npm install
npm run dev
```

Açılır: <http://localhost:5173> — bu origin artıq API-nin `Cors:AllowedOrigins`
siyahısındadır. Portu dəyişsəniz, həmin origin-i də oraya əlavə etməlisiniz,
yoxsa brauzer bütün sorğuları bloklayır.

| Əmr | Nə edir |
|---|---|
| `npm run dev` | Development server |
| `npm run build` | Tip yoxlaması + `dist/` |
| `npm run preview` | Build olunmuş versiyanı verir |
| `npm run typecheck` | Yalnız tip yoxlaması |

### `.env`

```
VITE_API_URL=http://localhost:5221
```

| Açar | Mənası |
|---|---|
| `VITE_API_URL` | ASP.NET API-nin origin-i |

---

## 3. Backend-ə qoşmaq

Qoşulma `.env`-dəki `VITE_API_URL`-dən başqa heç nə tələb etmir. Bütün sorğular
birbaşa API-yə gedir; arada nə adapter, nə saxta data var.

Portu `Properties/launchSettings.json`-dan yoxlayın və dəyişdiyiniz halda həmin
origin-i `appsettings.json`-dakı `Cors:AllowedOrigins` siyahısına da əlavə edin.

`VITE_API_URL`-in portunu `Properties/launchSettings.json`-dan yoxlayın.

---

## 4. Backend-də edilən dəyişiklik

**`GET /api/auths/users`** əlavə olundu — Admin-only, səhifələnmə + axtarış +
rol filtri, mövcud `PagedResult`/`ApiResponse` üslubunda.

**Səbəb:** API-də istifadəçi siyahısı endpoint-i yox idi. `assign-role` və
`link-driver` yalnız `userName` qəbul edir — yəni operator istifadəçi adını
artıq bilməlidir. Onsuz admin panelində cədvəl qurmaq mümkün deyildi.

### Dəyişən fayllar

| Fayl | Nə oldu |
|---|---|
| `DTOs/Auth/UserGetDTO.cs` | **YENİ** |
| `DTOs/Auth/UserQueryParameters.cs` | **YENİ** |
| `Services/Interfaces/IAuthService.cs` | `using` + metod imzası |
| `Services/Implements/AuthService.cs` | `using` + `GetUsersAsync` |
| `Controllers/AuthsController.cs` | `Users` action |
| `README.md` | endpoint cədvəlinə sətir |

Migration **lazım deyil** — sxem dəyişmir, yalnız oxu sorğusudur.

### ⚠️ Vacib: kompilyasiya yoxlanılmayıb

Bu maşında .NET SDK yox idi, ona görə `dotnet build` işlədə bilmədim. Kod
diqqətlə yazılıb və mövcud üsluba uyğundur, amma **ilk işə salanda kompilyasiyanı
yoxlayın**:

```bash
dotnet build
```

Sonra Swagger-də sınayın: `/swagger` → `GET /api/auths/users` (Admin tokeni ilə).

---

## 5. Backend-də tapdığım 4 məhdudiyyət

Bunlar qüsur deyil — sadəcə frontend onlara uyğun qurulub. Bilməkdə fayda var:

| # | Tapıntı | Frontend nə edir |
|---|---|---|
| 1 | `/api/dashboards` **yalnız Admin+Manager** | Sürücünün ana səhifəsi öz siyahılarından hesablanır (API onsuz da sürücüyə görə süzür) |
| 2 | Maşın və sürücü yaradarkən **şəkil məcburidir** (`NotNull` validator) | Formada şəkil ulduzlu/məcburidir — boş göndərmək 400 verir |
| 3 | Sürücüdə `Address` model-də `string?`, amma validator-da **məcburi** | Formada məcburi sahə kimi göstərilir |
| 4 | Bildirişlər **qlobaldır**, istifadəçiyə bağlı deyil | Hamı eyni siyahını görür, "oxundu" hamıya təsir edir. Konsolda bu yazılıb; sürücüdə siyahı yalnız oxunur |

---

## 6. JSON adlandırma tələsi (vacib)

System.Text.Json camelCase siyasəti yalnız **baş hərflər ardıcıllığını** kiçildir.
Nəticədə eyni məna daşıyan iki sahə fərqli adla gəlir:

| C# | JSON |
|---|---|
| `Vehicle.ImageURL` | `imageURL` |
| `Driver.ImageUrl` | `imageUrl` |
| `Vehicle.VIN` | `vin` |

Frontend tiplərində bu **dəqiq** əks olunub (`src/api/types.ts`). Backend-də bu
adları dəyişsəniz, frontend-də də dəyişməlidir — yoxsa sahə `undefined` gəlir və
şəkillər sakitcə görünmür.

---

## 7. Qeydiyyat və ilk hesab

`/register` ekranı `POST /api/auths/register` çağırır və uğurlu qeydiyyatdan
sonra eyni məlumatla avtomatik daxil olur. Sahə qaydaları backend-dəki
`RegisterDTOValidator` ilə eynidir — istifadəçi adı 3–50 simvol və yalnız
`a-zA-Z0-9._@+-`, şifrədə böyük hərf, kiçik hərf, rəqəm və bir işarə.

Özü qeydiyyatdan keçən hər kəs **Sürücü** rolu alır; konsolu görmək üçün Admin
`/admin/users` səhifəsindən rolu yüksəltməlidir. İlk Admin hesabı isə
`Seed:AdminEmail` / `Seed:AdminPassword` ilə seed edilir — bax kök `README.md`.

---

## 8. Fayl strukturu

```
drivious-web/src/
  api/          Klient, endpoint-lər, DTO tipləri, mesaj tərcüməsi
  auth/         Sessiya konteksti və route mühafizəçiləri
  lib/          Enum etiketləri, formatlama, class-name köməkçisi
  ui/           Dizayn sistemi (Button, Input, Card, Dialog, Badge…)
  components/   DataTable, Toolbar, formalar, seçicilər
  app/
    console/    Admin + Manager ekranları
    admin/      Yalnız Admin
    driver/     Sürücü ekranları
  routes.tsx    Rola görə qabığı seçir
```

---

## 9. Bir neçə qərar haqqında

- **Bir sütun tərifi hər iki görünüşü idarə edir.** Hər `Column` mobil kartda
  harada duracağını bildirir (`title`, `subtitle`, `trailing`, `meta`) — ona görə
  cədvələ əlavə etdiyiniz sütun telefonda unudula bilmir.

- **Siyahı vəziyyəti URL-dədir.** Səhifə, axtarış, sıralama və filtrlər query
  parametrləridir — filtrlənmiş görünüş yenilənməyə davam gətirir və linklə
  paylaşıla bilir.

- **Formatlama `az-AZ` lokalından asılı deyil.** Hər brauzer Azərbaycan dili
  datası ilə gəlmir; gəlməyənlər root lokala düşür və tarixi `2026-08-15`,
  məsafəni `170,933 km` kimi yazır. Tarix, rəqəm və ay adları `lib/format.ts`-də
  əl ilə qurulur → `15.08.2026`, `170 933 km`.

- **API mesajları tərcümə olunur, əvəz edilmir.** `api/messages.ts` serverin
  ingiliscəsini AZ-yə çevirir; adi CRUD təsdiqləri qayda ilə tutulur. Uyğunluq
  tapılmayan mətn olduğu kimi ötürülür — oxunmayan mesaj udulmuş mesajdan yaxşıdır.

- **Yüklənmiş fayl URL-ləri `VITE_API_URL`-ə köçürülür.** API onları öz sorğu
  host-undan qurur; localhost-da saxlanmış şəkil həmin host-u saxlayır.

- **Roldan asılı gizlətmə təhlükəsizlik sərhədi deyil.** API eyni bölgünü özü
  tətbiq edir; frontend sadəcə mütləq 403 verəcək ekranı göstərmir.

---

## 10. Yoxlanılanlar

| Yoxlama | Nəticə |
|---|---|
| `tsc --noEmit` | ✅ keçir |
| `vite build` | ✅ keçir |
| Brauzer: hər iki qabıq | ✅ |
| Brauzer: qaranlıq + işıqlı tema | ✅ |
| Brauzer: mobil (375px) + masaüstü (1440px) | ✅ |
| `dotnet build` | ❌ **işlədilməyib** — SDK yox idi |

### Qurulma zamanı tapılıb düzəldilən 3 qüsur

1. Səhifə yeniləndikdə sessiya itirdi — tokenlər yalnız yaddaşda saxlanırdı.
2. Brauzer `az-AZ` lokalını dəstəkləmir → tarixlər `2026-08-15`, rəqəmlər
   `170,933 km` çıxırdı. Bütün formatlama `Intl`-dən çıxarıldı.
3. Nümunə datada cari aya gəlir düşmürdü → sürücü ekranında "Bu ay 0 ₼".

---

## 11. Qalan işlər

- [ ] `dotnet build` ilə backend dəyişikliyini yoxla
- [ ] Swagger-də `GET /api/auths/users`-i Admin tokeni ilə sına
- [ ] API işə düşəndən sonra `/register` → avtomatik login axınını uçdan-uca sına
- [ ] Köhnə `drivious-frontend/` skeleti toxunulmadan qalıb — lazım deyilsə silin
- [ ] Prod-a çıxarkən deploy olunmuş origin-i `Cors:AllowedOrigins`-ə əlavə edin
- [ ] 100-dən çox maşın/sürücü olarsa, seçicilər (`components/pickers.tsx`)
      axtarışlı variantla əvəzlənməlidir — indi API-nin maksimum səhifə
      ölçüsü (100) ilə məhdudlaşır

---

## 12. Nəzərə alınmalı

- **Bildirişlər qlobaldır.** İstifadəçiyə ünvanlanmır, ona görə hamı eyni siyahını
  görür və "oxundu" hamıya təsir edir. Əgər hər istifadəçiyə ayrı bildiriş
  lazımdırsa, bu backend işidir (`Notification`-a `UserId` + filtr).
- **Dashboard yalnız Manager və Admin üçündür.** Sürücünün ana səhifəsi öz
  siyahılarından hesablanır. Sürücüyə də ümumi rəqəmlər lazımdırsa, backend-də
  ya rol açılmalı, ya ayrıca sürücü dashboard endpoint-i olmalıdır.
