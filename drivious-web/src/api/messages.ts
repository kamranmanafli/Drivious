/**
 * The API answers in English. Rather than duplicating every string, this maps
 * the ones a person actually sees — the CRUD confirmations follow a handful of
 * regular shapes, so those are covered by rules instead of entries.
 *
 * Anything unmatched is returned untouched: a message the user cannot read is
 * still better than swallowing it.
 */

/** Entity names as the API writes them, and their Azerbaijani equivalents. */
const ENTITIES: Array<[string, string, string]> = [
  // [API singular, AZ nominative, AZ with possessive/accusative stem]
  ["Vehicle assignment", "Təyinat", "Təyinat"],
  ["Vehicle assignments", "Təyinatlar", "Təyinatlar"],
  ["Vehicle document", "Sənəd", "Sənəd"],
  ["Vehicle documents", "Sənədlər", "Sənədlər"],
  ["Vehicle", "Maşın", "Maşın"],
  ["Vehicles", "Maşınlar", "Maşınlar"],
  ["Driver", "Sürücü", "Sürücü"],
  ["Drivers", "Sürücülər", "Sürücülər"],
  ["Expense", "Xərc", "Xərc"],
  ["Expenses", "Xərclər", "Xərclər"],
  ["Income", "Gəlir", "Gəlir"],
  ["Incomes", "Gəlirlər", "Gəlirlər"],
  ["Fuel log", "Yanacaq qeydi", "Yanacaq qeydi"],
  ["Fuel logs", "Yanacaq qeydləri", "Yanacaq qeydləri"],
  ["Maintenance", "Servis", "Servis"],
  ["Maintenances", "Servislər", "Servislər"],
  ["Insurance", "Sığorta", "Sığorta"],
  ["Insurances", "Sığortalar", "Sığortalar"],
  ["Notification", "Bildiriş", "Bildiriş"],
  ["Notifications", "Bildirişlər", "Bildirişlər"],
  ["Dashboard", "İdarə paneli", "İdarə paneli"],
  ["User", "İstifadəçi", "İstifadəçi"],
];

/** Longest first, so "Vehicle document" is not matched as "Vehicle". */
const ENTITY_PATTERN = ENTITIES.map(([en]) => en)
  .sort((a, b) => b.length - a.length)
  .join("|");

const entityAz = (name: string) =>
  ENTITIES.find(([en]) => en.toLowerCase() === name.toLowerCase())?.[1] ?? name;

/** Suffix rules applied after an entity name is recognised. */
const CRUD_RULES: Array<[RegExp, (entity: string, m: RegExpMatchArray) => string]> = [
  [/^(.+) created successfully\.$/i, (e) => `${e} yaradıldı.`],
  [/^(.+) updated successfully\.$/i, (e) => `${e} yeniləndi.`],
  [/^(.+) deleted successfully\.$/i, (e) => `${e} həmişəlik silindi.`],
  [/^(.+) retrieved successfully\.$/i, (e) => `${e} yükləndi.`],
  [/^(.+) status changed successfully\.$/i, (e) => `${e} arxivə/geri köçürüldü.`],
  [/^(.+) returned successfully\.$/i, (e) => `${e} qaytarıldı.`],
  [/^(.+) not found\.$/i, (e) => `${e} tapılmadı.`],
  [/^(.+) could not be created\.$/i, (e) => `${e} yaradıla bilmədi.`],
  [/^(.+) could not be updated\.$/i, (e) => `${e} yenilənə bilmədi.`],
  [/^(.+) could not be deleted\.$/i, (e) => `${e} silinə bilmədi.`],
  [/^(.+) could not be saved\.$/i, (e) => `${e} yadda saxlanıla bilmədi.`],
  [/^(.+) status could not be changed\.$/i, (e) => `${e} statusu dəyişdirilə bilmədi.`],
  [/^Deleted (.+) retrieved successfully\.$/i, (e) => `Arxivdəki ${e.toLowerCase()} yükləndi.`],
  [/^(.+) is required\.$/i, (e) => `${e} seçilməlidir.`],
];

/** Messages that do not follow the entity shape. */
const EXACT: Record<string, string> = {
  // ── Auth
  "Login successful.": "Giriş uğurlu oldu.",
  "Logged out successfully.": "Hesabdan çıxıldı.",
  "Token refreshed.": "Sessiya yeniləndi.",
  "User registered successfully.": "Qeydiyyat tamamlandı.",
  "Password changed successfully.": "Şifrə dəyişdirildi.",
  "Username or password is incorrect.": "İstifadəçi adı və ya şifrə yanlışdır.",
  "Username already exists.": "Bu istifadəçi adı artıq mövcuddur.",
  "Email already exists.": "Bu e-poçt artıq qeydiyyatdadır.",
  "Passwords do not match.": "Şifrələr uyğun gəlmir.",
  "New passwords do not match.": "Yeni şifrələr uyğun gəlmir.",
  "Invite code is not valid.": "Dəvət kodu yanlışdır.",
  "Refresh token is invalid or has expired.":
    "Sessiya bitib. Yenidən daxil olun.",
  "Refresh token not found.": "Sessiya tapılmadı.",
  "Refresh token is required.": "Sessiya açarı tələb olunur.",
  "Account linked to driver.": "Hesab sürücüyə bağlandı.",
  "Account unlinked from driver.": "Hesabın sürücü bağlantısı silindi.",
  "That driver is already linked to another account.":
    "Bu sürücü artıq başqa hesaba bağlıdır.",
  "Driver id is not valid. Omit the field to unlink instead.":
    "Sürücü ID-si düzgün deyil. Bağlantını silmək üçün sahəni boş buraxın.",

  // ── Business rules
  "This vehicle is already assigned to a driver. Return it first.":
    "Bu maşın artıq bir sürücüyə təyin olunub. Əvvəlcə qaytarın.",
  "This driver already holds a vehicle. Return it first.":
    "Bu sürücünün artıq maşını var. Əvvəlcə qaytarın.",
  "This vehicle has already been returned.": "Bu maşın artıq qaytarılıb.",
  "A vehicle with this VIN already exists.":
    "Bu VIN nömrəsi ilə maşın artıq mövcuddur.",
  "A vehicle with this plate number already exists.":
    "Bu dövlət nişanı ilə maşın artıq mövcuddur.",
  "Return date cannot be earlier than the assigned date.":
    "Qaytarılma tarixi təyinat tarixindən əvvəl ola bilməz.",
  "Returned date cannot be earlier than assigned date.":
    "Qaytarılma tarixi təyinat tarixindən əvvəl ola bilməz.",

  // ── Server / transport
  "An unexpected error occurred.": "Gözlənilməz xəta baş verdi.",

  // ── Validation: identity
  "Username is required.": "İstifadəçi adı tələb olunur.",
  "Username must be at least 3 characters.":
    "İstifadəçi adı ən azı 3 simvol olmalıdır.",
  "Username cannot exceed 50 characters.":
    "İstifadəçi adı 50 simvoldan çox ola bilməz.",
  "Username may only contain letters, digits and . _ @ + -":
    "İstifadəçi adında yalnız hərf, rəqəm və . _ @ + - işarələri ola bilər.",
  "Email is required.": "E-poçt tələb olunur.",
  "Email is not a valid address.": "E-poçt ünvanı düzgün deyil.",
  "Invalid email address.": "E-poçt ünvanı düzgün deyil.",
  "Email cannot exceed 100 characters.": "E-poçt 100 simvoldan çox ola bilməz.",
  "Password is required.": "Şifrə tələb olunur.",
  "Password must be at least 6 characters.": "Şifrə ən azı 6 simvol olmalıdır.",
  "Password must contain a digit.": "Şifrədə ən azı bir rəqəm olmalıdır.",
  "Password must contain a lowercase letter.":
    "Şifrədə kiçik hərf olmalıdır.",
  "Password must contain an uppercase letter.":
    "Şifrədə böyük hərf olmalıdır.",
  "Password must contain a non-alphanumeric character.":
    "Şifrədə xüsusi simvol (!, @, # kimi) olmalıdır.",
  "Password confirmation is required.": "Şifrə təsdiqi tələb olunur.",
  "Current password is required.": "Cari şifrə tələb olunur.",
  "New password is required.": "Yeni şifrə tələb olunur.",
  "New password must be at least 6 characters.":
    "Yeni şifrə ən azı 6 simvol olmalıdır.",
  "New password must contain a digit.": "Yeni şifrədə rəqəm olmalıdır.",
  "New password must contain a lowercase letter.":
    "Yeni şifrədə kiçik hərf olmalıdır.",
  "New password must contain an uppercase letter.":
    "Yeni şifrədə böyük hərf olmalıdır.",
  "New password must contain a non-alphanumeric character.":
    "Yeni şifrədə xüsusi simvol olmalıdır.",
  "New password must differ from the current one.":
    "Yeni şifrə cari şifrədən fərqli olmalıdır.",
  "Role is required.": "Rol seçilməlidir.",

  // ── Validation: vehicle
  "Brand is required.": "Marka tələb olunur.",
  "Brand must be at least 2 characters.": "Marka ən azı 2 simvol olmalıdır.",
  "Brand cannot exceed 50 characters.": "Marka 50 simvoldan çox ola bilməz.",
  "Model is required.": "Model tələb olunur.",
  "Model must be at least 2 characters.": "Model ən azı 2 simvol olmalıdır.",
  "Model cannot exceed 50 characters.": "Model 50 simvoldan çox ola bilməz.",
  "Plate number is required.": "Dövlət nişanı tələb olunur.",
  "Plate number cannot exceed 20 characters.":
    "Dövlət nişanı 20 simvoldan çox ola bilməz.",
  "VIN is required.": "VIN nömrəsi tələb olunur.",
  "VIN must be exactly 17 characters.": "VIN dəqiq 17 simvol olmalıdır.",
  "Color is required.": "Rəng tələb olunur.",
  "Color cannot exceed 30 characters.": "Rəng 30 simvoldan çox ola bilməz.",
  "Invalid fuel type.": "Yanacaq növü düzgün deyil.",
  "Invalid vehicle status.": "Maşın statusu düzgün deyil.",
  "Mileage cannot be negative.": "Yürüş mənfi ola bilməz.",
  "Vehicle image is required.": "Maşın şəkli tələb olunur.",

  // ── Validation: driver
  "First name is required.": "Ad tələb olunur.",
  "First name must be at least 2 characters.": "Ad ən azı 2 simvol olmalıdır.",
  "First name cannot exceed 50 characters.": "Ad 50 simvoldan çox ola bilməz.",
  "Last name is required.": "Soyad tələb olunur.",
  "Last name must be at least 2 characters.":
    "Soyad ən azı 2 simvol olmalıdır.",
  "Last name cannot exceed 50 characters.":
    "Soyad 50 simvoldan çox ola bilməz.",
  "Phone number is required.": "Telefon nömrəsi tələb olunur.",
  "Phone number format is invalid.":
    "Telefon formatı yanlışdır. Nümunə: +994501234567 və ya 0501234567",
  "Identity number is required.": "Ş/V nömrəsi (FIN) tələb olunur.",
  "Driver license number is required.": "Sürücülük vəsiqəsi nömrəsi tələb olunur.",
  "License expiration date must be in the future.":
    "Vəsiqənin bitmə tarixi gələcəkdə olmalıdır.",
  "Birth date is invalid.": "Doğum tarixi düzgün deyil.",
  "Hire date cannot be in the future.":
    "İşə qəbul tarixi gələcəkdə ola bilməz.",
  "Address is required.": "Ünvan tələb olunur.",
  "Address must be at least 5 characters.": "Ünvan ən azı 5 simvol olmalıdır.",
  "Address cannot exceed 200 characters.":
    "Ünvan 200 simvoldan çox ola bilməz.",
  "Driver image is required.": "Sürücü şəkli tələb olunur.",

  // ── Validation: money & dates
  "Amount must be greater than 0.": "Məbləğ 0-dan böyük olmalıdır.",
  "Price must be greater than 0.": "Qiymət 0-dan böyük olmalıdır.",
  "Cost must be greater than 0.": "Xərc 0-dan böyük olmalıdır.",
  "Fuel liters must be greater than 0.": "Litr 0-dan böyük olmalıdır.",
  "Expense date cannot be in the future.":
    "Xərc tarixi gələcəkdə ola bilməz.",
  "Income date cannot be in the future.":
    "Gəlir tarixi gələcəkdə ola bilməz.",
  "Fuel date cannot be in the future.":
    "Yanacaq tarixi gələcəkdə ola bilməz.",
  "Maintenance date cannot be in the future.":
    "Servis tarixi gələcəkdə ola bilməz.",
  "Notification date cannot be in the future.":
    "Bildiriş tarixi gələcəkdə ola bilməz.",
  "Assigned date cannot be in the future.":
    "Təyinat tarixi gələcəkdə ola bilməz.",
  "Upload date cannot be in the future.":
    "Yüklənmə tarixi gələcəkdə ola bilməz.",
  "Next maintenance date must be later than maintenance date.":
    "Növbəti servis tarixi servis tarixindən sonra olmalıdır.",
  "Start date cannot be later than end date.":
    "Başlama tarixi bitmə tarixindən sonra ola bilməz.",
  "End date must be later than start date.":
    "Bitmə tarixi başlama tarixindən sonra olmalıdır.",

  // ── Validation: text fields
  "Description is required.": "Təsvir tələb olunur.",
  "Description must be at least 5 characters.":
    "Təsvir ən azı 5 simvol olmalıdır.",
  "Description cannot exceed 500 characters.":
    "Təsvir 500 simvoldan çox ola bilməz.",
  "Title is required.": "Başlıq tələb olunur.",
  "Title must be at least 3 characters.": "Başlıq ən azı 3 simvol olmalıdır.",
  "Title cannot exceed 100 characters.":
    "Başlıq 100 simvoldan çox ola bilməz.",
  "Message is required.": "Mətn tələb olunur.",
  "Message must be at least 5 characters.": "Mətn ən azı 5 simvol olmalıdır.",
  "Message cannot exceed 1000 characters.":
    "Mətn 1000 simvoldan çox ola bilməz.",
  "Note cannot exceed 500 characters.": "Qeyd 500 simvoldan çox ola bilməz.",
  "Company name is required.": "Şirkət adı tələb olunur.",
  "Company name must be at least 2 characters.":
    "Şirkət adı ən azı 2 simvol olmalıdır.",
  "Company name cannot exceed 100 characters.":
    "Şirkət adı 100 simvoldan çox ola bilməz.",
  "Policy number is required.": "Polis nömrəsi tələb olunur.",
  "Policy number must be at least 3 characters.":
    "Polis nömrəsi ən azı 3 simvol olmalıdır.",
  "Policy number cannot exceed 50 characters.":
    "Polis nömrəsi 50 simvoldan çox ola bilməz.",
  "Station name is required.": "Məntəqə adı tələb olunur.",
  "Station name must be at least 2 characters.":
    "Məntəqə adı ən azı 2 simvol olmalıdır.",
  "Station name cannot exceed 100 characters.":
    "Məntəqə adı 100 simvoldan çox ola bilməz.",
  "Service center is required.": "Servis mərkəzi tələb olunur.",
  "Service center must be at least 2 characters.":
    "Servis mərkəzi ən azı 2 simvol olmalıdır.",
  "Service center cannot exceed 100 characters.":
    "Servis mərkəzi 100 simvoldan çox ola bilməz.",

  // ── Validation: enums & files
  "Invalid expense category.": "Xərc kateqoriyası düzgün deyil.",
  "Invalid service type.": "Servis növü düzgün deyil.",
  "Invalid document type.": "Sənəd növü düzgün deyil.",
  "Invalid notification type.": "Bildiriş növü düzgün deyil.",
  "Only image files are allowed.": "Yalnız şəkil faylı yükləmək olar.",
  "Image cannot exceed 5 MB.": "Şəkil 5 MB-dan böyük ola bilməz.",
  "Document cannot exceed 10 MB.": "Sənəd 10 MB-dan böyük ola bilməz.",
  "Document file is required.": "Sənəd faylı tələb olunur.",
  "Vehicle is required.": "Maşın seçilməlidir.",
  "Driver is required.": "Sürücü seçilməlidir.",
};

/** Patterns carrying a value the message needs to keep. */
const PARAMETERISED: Array<[RegExp, (m: RegExpMatchArray) => string]> = [
  [
    /^Year must be between (\d+) and (\d+)\.$/,
    (m) => `İl ${m[1]} ilə ${m[2]} arasında olmalıdır.`,
  ],
  [
    /^Scan complete\. (\d+) notification\(s\) created\.$/,
    (m) =>
      m[1] === "0"
        ? "Skan bitdi. Yeni xəbərdarlıq yoxdur."
        : `Skan bitdi. ${m[1]} yeni xəbərdarlıq yaradıldı.`,
  ],
  [
    /^This vehicle has (\d+) related record\(s\) and cannot be permanently deleted\..*$/,
    (m) =>
      `Bu maşının ${m[1]} əlaqəli qeydi var, həmişəlik silinə bilməz. Onun əvəzinə arxivə göndərin.`,
  ],
  [
    /^This driver has (\d+) related record\(s\) and cannot be permanently deleted\..*$/,
    (m) =>
      `Bu sürücünün ${m[1]} əlaqəli qeydi var, həmişəlik silinə bilməz. Onun əvəzinə arxivə göndərin.`,
  ],
  [
    /^User assigned to the (\w+) role\.$/,
    (m) => `İstifadəçiyə "${roleAz(m[1])}" rolu verildi.`,
  ],
  [
    /^User is already in the (\w+) role\.$/,
    (m) => `İstifadəçi artıq "${roleAz(m[1])}" rolundadır.`,
  ],
  [
    /^Role '(\w+)' does not exist\. Valid roles: (.+)\.$/,
    (m) => `"${m[1]}" rolu mövcud deyil. Mümkün rollar: ${m[2]}.`,
  ],
];

function roleAz(role: string): string {
  return { Admin: "Administrator", Manager: "Menecer", Driver: "Sürücü" }[role] ?? role;
}

/** Translates one API message. Unknown text is passed through unchanged. */
export function translate(message: string | null | undefined): string {
  if (!message) return "";

  const text = message.trim();

  const exact = EXACT[text];
  if (exact) return exact;

  for (const [pattern, render] of PARAMETERISED) {
    const match = text.match(pattern);
    if (match) return render(match);
  }

  for (const [pattern, render] of CRUD_RULES) {
    const match = text.match(pattern);
    if (!match) continue;

    const subject = match[1];
    // Only rewrite when the subject is one of ours; otherwise a sentence like
    // "Something else is required." would be mangled into nonsense.
    if (!new RegExp(`^(${ENTITY_PATTERN})$`, "i").test(subject)) continue;

    return render(entityAz(subject), match);
  }

  return text;
}
