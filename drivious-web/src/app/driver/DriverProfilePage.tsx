import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { IdCard, KeyRound, LogOut, Mail, MapPin, Phone } from "lucide-react";
import { drivers as driversApi } from "@/api/endpoints";
import { assetUrl } from "@/api/client";
import { roleLabels } from "@/lib/enums";
import { date, daysUntil, relativeDays } from "@/lib/format";
import { useAuth } from "@/auth/AuthContext";
import { ThemeToggle } from "@/theme";
import { ChangePasswordDialog } from "../ChangePasswordDialog";
import { Avatar, Badge, Button, Card, CardBody, CardHeader, CardTitle, Skeleton } from "@/ui";

export function DriverProfilePage() {
  const { user, roles, logout } = useAuth();
  const [passwordOpen, setPasswordOpen] = useState(false);

  const driverId = user?.driverId ?? null;

  const profile = useQuery({
    queryKey: ["drivers", driverId],
    queryFn: () => driversApi.get(driverId!),
    enabled: Boolean(driverId),
  });

  const licenceDays = daysUntil(profile.data?.licenseExpireDate);
  const licenceTone =
    licenceDays === null ? "neutral" : licenceDays < 0 ? "danger" : licenceDays <= 30 ? "warning" : "success";

  return (
    <>
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Profil</h1>
        <p className="mt-0.5 text-sm text-muted-foreground">Hesab və sürücü məlumatlarınız.</p>
      </div>

      <Card>
        <CardBody className="flex items-center gap-3.5">
          <Avatar
            src={assetUrl(profile.data?.imageUrl)}
            name={profile.data?.fullName ?? user?.userName}
            className="size-14"
          />

          <div className="min-w-0">
            <p className="truncate text-base font-semibold">
              {profile.data?.fullName ?? user?.userName}
            </p>
            <p className="truncate text-sm text-muted-foreground">{user?.email}</p>

            <div className="mt-1.5 flex flex-wrap gap-1">
              {roles.map((role) => (
                <Badge key={role} tone="primary">
                  {roleLabels[role] ?? role}
                </Badge>
              ))}
            </div>
          </div>
        </CardBody>
      </Card>

      {driverId ? (
        <Card>
          <CardHeader>
            <CardTitle>Sürücü məlumatları</CardTitle>
          </CardHeader>

          {profile.isLoading ? (
            <CardBody className="space-y-2.5">
              {Array.from({ length: 5 }, (_, i) => (
                <Skeleton key={i} className="h-9 w-full" />
              ))}
            </CardBody>
          ) : profile.data ? (
            <ul className="divide-y divide-border">
              <Item icon={<Phone />} label="Telefon">
                <a href={`tel:${profile.data.phoneNumber}`} className="hover:text-primary">
                  {profile.data.phoneNumber}
                </a>
              </Item>

              <Item icon={<Mail />} label="E-poçt">
                <a href={`mailto:${profile.data.email}`} className="hover:text-primary">
                  {profile.data.email}
                </a>
              </Item>

              <Item icon={<IdCard />} label="Vəsiqə nömrəsi">
                <span className="font-mono text-xs">{profile.data.driverLicenseNumber}</span>
              </Item>

              <Item icon={<IdCard />} label="Vəsiqənin bitmə tarixi">
                <span className="inline-flex items-center gap-2">
                  <span className="tnum">{date(profile.data.licenseExpireDate)}</span>
                  <Badge tone={licenceTone}>
                    {licenceDays !== null && licenceDays < 0
                      ? "bitib"
                      : relativeDays(profile.data.licenseExpireDate)}
                  </Badge>
                </span>
              </Item>

              <Item icon={<MapPin />} label="Ünvan">
                <span className="text-right">{profile.data.address || "—"}</span>
              </Item>

              <Item icon={<IdCard />} label="İşə qəbul">
                <span className="tnum">{date(profile.data.hireDate)}</span>
              </Item>
            </ul>
          ) : null}

          <CardBody className="border-t border-border">
            <p className="text-xs text-muted-foreground">
              Bu məlumatları dəyişmək üçün menecerlə əlaqə saxlayın.
            </p>
          </CardBody>
        </Card>
      ) : (
        <Card>
          <CardBody>
            <p className="text-sm font-medium">Sürücü kartı bağlı deyil</p>
            <p className="mt-1 text-xs text-muted-foreground">
              Hesabınız hələ bir sürücü kartına bağlanmayıb. Gəlir və təyinatlarınızı görmək üçün
              administratora müraciət edin.
            </p>
          </CardBody>
        </Card>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Tənzimləmələr</CardTitle>
        </CardHeader>

        <CardBody className="space-y-3">
          <div className="flex items-center justify-between gap-3">
            <span className="text-sm">Tema</span>
            <ThemeToggle />
          </div>

          <div className="flex flex-col gap-2 border-t border-border pt-3 sm:flex-row">
            <Button variant="secondary" onClick={() => setPasswordOpen(true)} className="sm:flex-1">
              <KeyRound />
              Şifrəni dəyiş
            </Button>

            <Button variant="dangerGhost" onClick={() => void logout()} className="sm:flex-1">
              <LogOut />
              Çıxış
            </Button>
          </div>
        </CardBody>
      </Card>

      <ChangePasswordDialog open={passwordOpen} onOpenChange={setPasswordOpen} />
    </>
  );
}

function Item({
  icon,
  label,
  children,
}: {
  icon: React.ReactNode;
  label: string;
  children: React.ReactNode;
}) {
  return (
    <li className="flex items-center justify-between gap-3 px-4 py-3">
      <span className="flex items-center gap-2.5 text-sm text-muted-foreground [&_svg]:size-4">
        {icon}
        {label}
      </span>
      <span className="min-w-0 text-sm">{children}</span>
    </li>
  );
}
