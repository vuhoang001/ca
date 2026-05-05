# Workflows — Auth Service

Tài liệu này mô tả **cách thức hoạt động (workflow)** của các chức năng chính: register, login, refresh,
logout/revoke, change password, RBAC (role/permission), audit, seeding. Tập trung vào *flow runtime*
(ai gọi ai, dữ liệu chảy qua đâu, side-effect gì), khác với `architecture.md` thiên về cấu trúc.

> Đọc `architecture.md` trước để có bức tranh tổng thể, rồi đọc file này để hiểu *cơ chế chạy*.

---

## 0. Bộ khung pipeline chung

Mọi request đi qua đúng các bước sau (xem `Api/Program.cs`):

```
HTTP request
  └─► Serilog request logging
  └─► UseExceptionHandler  (GlobalExceptionHandler bắt mọi exception → ProblemDetails)
  └─► UseRateLimiter        ("auth" 10/min, "default" 60/min)
  └─► UseAuthentication     (JwtBearer + OnTokenValidated kiểm tra denylist jti)
  └─► UseAuthorization      (fallback policy = RequireAuthenticatedUser)
  └─► Endpoint /api/v1/...
        ├─► Bind body → Command/Query
        ├─► ISender.Send(...)
        │    ├─► ValidationBehavior  (FluentValidation → ValidationException)
        │    ├─► LoggingBehavior     (log request name + payload)
        │    └─► Handler
        │          ├─► Repository / DomainService
        │          ├─► AppDbContext.SaveChangesAsync()
        │          │    ├─► ApplyAuditFields() (CreatedAtUtc/By, LastModifiedAtUtc/By)
        │          │    └─► EventDispatchInterceptor (dispatch domain events qua MediatR)
        │          └─► AuditService.WriteAsync (transaction RIÊNG)
        └─► Wrap kết quả vào ApiEnvelope<T> { data, message? }
```

Lưu ý: **AuditService có `SaveChangesAsync` của riêng nó**, nên một use-case ghi 2 transaction:
một cho business state, một cho audit log. Xem mục "Vấn đề" #6.

---

## 1. Register — `POST /api/v1/auth/register`

**File**: `Application/Src/Features/Auth/Commands/RegisterUserCommand.cs`
**Anonymous**, rate-limit `auth` (10/min).

```
Client
  │ POST { email, userName, password, tenantId? }
  ▼
RegisterUserCommandValidator   email/userName/password format & length
  │
  ▼
RegisterUserCommandHandler
  ├─► UserRepository.ExistsByNormalizedEmailAsync(EMAIL.ToUpperInvariant())
  │      └─► nếu trùng → throw ConflictException (409)
  ├─► new User(tenantId, userName, email, "")     ← đồng thời raise UserRegisteredDomainEvent
  ├─► PasswordService.HashPassword(user, password) (PasswordHasher<User> = PBKDF2)
  ├─► user.SetPassword(hash)                       ← regenerate SecurityStamp
  ├─► UserRepository.Add(user)
  ├─► UnitOfWork.SaveChangesAsync()
  │      ├─► ApplyAuditFields → CreatedAtUtc/By="system" (vì chưa đăng nhập)
  │      └─► EventDispatchInterceptor → dispatch UserRegisteredDomainEvent
  │            └─► UserRegisteredDomainEventHandler chỉ log Information, không có side-effect
  └─► AuditService.WriteAsync("user.registered", "User", id, {email, userName}, "Success")
       └─► commit audit_logs (transaction thứ 2)
  ▼
ApiEnvelope { data: { userId, email, userName, createdAtUtc } }
```

**Quan trọng**:
- User mới **không** được gán role nào ⇒ token sau khi login chỉ có `sub/email/...`, không có `roles`/`permissions`.
- `TenantId` được nhận **trực tiếp từ body** mà không validate → bất kỳ ai cũng có thể tạo user thuộc bất kỳ tenant nào.

---

## 2. Login — `POST /api/v1/auth/login`

**File**: `Application/Src/Features/Auth/Commands/LoginCommand.cs`
**Anonymous**, rate-limit `auth`.

```
Client
  │ POST { email, password, clientId? }
  ▼
LoginCommandHandler
  ├─► UserRepository.GetByNormalizedEmailAsync(includeRoles:true)
  │      └─► null → throw UnauthorizedException("Invalid credentials.") — KHÔNG audit
  ├─► PasswordService.VerifyPassword(user, hash, password)
  │      └─► false →
  │           ├─► AuditService.WriteAsync("auth.login", ..., "Failed")  ← chỉ log khi user TỒN TẠI
  │           └─► throw UnauthorizedException
  ├─► nếu clientId có:
  │      └─► ClientAppRepository.GetByClientIdAsync → null → 401
  │      └─► KHÔNG check IsActive, KHÔNG verify SecretHash
  ├─► roles       = UserRepository.GetRoleNamesAsync(userId) (chỉ Role.IsActive)
  ├─► permissions = UserRepository.GetPermissionCodesAsync(userId)
  │                  (qua user.UserRoles → Role.IsActive → RolePermissions → Permission.IsActive)
  ├─► JwtTokenService.GenerateTokens(...)
  │      ├─► Random 64-byte refresh token (Base64) — plaintext trả về client
  │      ├─► SHA-256 hex hash của refresh token — lưu DB
  │      ├─► JWT HS256, 15 phút (AccessTokenMinutes), gồm:
  │      │     sub, email, preferred_username, jti, NameIdentifier, Email,
  │      │     [tenant_id], [client_id], roles ClaimTypes.Role*, permissions "permissions"*
  │      └─► RefreshTokenExpiresAt = now + RefreshTokenDays (7)
  ├─► RefreshTokenRepository.Add(new RefreshToken(... TokenHash, JwtId, expires, ip, ua))
  ├─► user.MarkLogin(now)
  ├─► UnitOfWork.SaveChangesAsync()  ← persist refresh_token + LastLoginAtUtc
  └─► AuditService.WriteAsync("auth.login", ..., "Success")
  ▼
TokenResponse { accessToken, accessTokenExpiresAtUtc,
                refreshToken (plaintext!), refreshTokenExpiresAtUtc,
                user { id, email, userName, tenantId, roles, permissions } }
```

**Quan trọng**:
- **Không kiểm tra `User.Status`** (Pending/Locked/Disabled) → user disabled vẫn login được.
- **Không kiểm tra `ClientApp.IsActive`** và **không verify secret** cho confidential client.
- Nếu user đã có refresh token cũ, token cũ vẫn còn hiệu lực ⇒ đăng nhập 100 thiết bị = 100 token active.

---

## 3. Refresh token — `POST /api/v1/auth/refresh-token`

**File**: `Application/Src/Features/Auth/Commands/RefreshTokenCommand.cs`
**Anonymous**, rate-limit `auth`. Đây cũng là cơ chế **rotation**.

```
Client
  │ POST { refreshToken, clientId? }
  ▼
RefreshTokenCommandHandler
  ├─► tokenHash = SHA256-hex(refreshToken)
  ├─► RefreshTokenRepository.GetByTokenHashAsync(tokenHash, includeUser:true)
  │      └─► null → 401
  ├─► nếu !current.IsActive (RevokedAtUtc != null hoặc đã hết hạn) → 401
  ├─► nếu clientId có: ClientAppRepository.GetByClientIdAsync → 401 nếu null
  │     (KHÔNG check rằng clientId này chính là client đã issue refresh token này!)
  ├─► roles + permissions = tính lại từ DB (nên permission/role mới đã apply ngay)
  ├─► tokenResult = JwtTokenService.GenerateTokens(...)  ← JWT mới + refresh token mới
  ├─► nextRefreshToken = new RefreshToken(...)
  ├─► RefreshTokenRepository.Add(nextRefreshToken)
  ├─► current.Revoke("Rotated", nextRefreshToken.Id)   ← cũ trỏ tới mới qua ReplacedByTokenId
  ├─► UnitOfWork.SaveChangesAsync()                    ← cả 2 row commit cùng transaction ✓
  └─► AuditService.WriteAsync("auth.token.refreshed", ..., "Success")
  ▼
TokenResponse (giống login)
```

**Lưu ý đặc biệt**:
- **Token rotation hoạt động đúng**: row cũ vẫn tồn tại, được mark `RevokedAtUtc + RevocationReason="Rotated" + ReplacedByTokenId=...`. Đây là hành vi mong muốn để có thể detect refresh-token-reuse attack — *nhưng dự án CHƯA implement detection đó* (thấy reuse → revoke cả family).
- Nếu attacker chiếm refresh token và refresh trước nạn nhân → server cấp token mới cho attacker, nạn nhân thấy "no longer active" → mất session.
- **Refresh token không bị "buộc cứng" với client_id ban đầu**: ai có refresh token vẫn refresh được.
- **Không kiểm tra `User.Status`** ở đây nữa.

---

## 4. Logout / Revoke — `POST /api/v1/auth/revoke`

**File**: `Application/Src/Features/Auth/Commands/RevokeTokenCommand.cs`
**Authenticated**, permission `auth.tokens.revoke`, rate-limit `default`.

```
Client (Bearer access token)
  │ POST { refreshToken?, accessToken?, reason? }
  ▼
Validator: phải có ít nhất 1 trong refreshToken/accessToken
RevokeTokenCommandHandler
  ├─► nếu refreshToken có:
  │      ├─► tokenHash = SHA256-hex(...)
  │      ├─► refreshToken row = RefreshTokenRepository.GetByTokenHashAsync(... includeUser:true)
  │      │      └─► null → 404 NotFound
  │      ├─► nếu chưa revoke: row.Revoke(reason)   ← chỉ set RevokedAtUtc + RevocationReason
  │      └─► AuditService.WriteAsync("auth.token.revoked", ..., {kind:"refresh"}, "Success")
  ├─► nếu accessToken có:
  │      ├─► tokenService.ReadAccessToken(token)
  │      │      └─► validate signature/issuer/audience nhưng KHÔNG validate lifetime
  │      │      → trả về (jwtId, userId, expiresAt)
  │      ├─► nếu chưa có row active trong revoked_access_tokens với jti này:
  │      │      └─► Add(new RevokedAccessToken(jti, userId, expiresAt, reason))
  │      └─► AuditService.WriteAsync(..., {kind:"access"})
  └─► UnitOfWork.SaveChangesAsync()
```

**Cách "logout" hoạt động**:
- Access token là **stateless**, server không thể "xoá" được. Cách duy nhất để vô hiệu là **denylist `jti`**:
  trên mỗi request, `JwtBearerEvents.OnTokenValidated` gọi `RevokedAccessTokenRepository.ExistsActiveAsync(jti)`,
  nếu có row → `context.Fail("Token has been revoked.")` ⇒ 401.
- Refresh token revoke = ghi `RevokedAtUtc` ⇒ khi gọi `/refresh-token` thì `IsActive` trả false ⇒ 401.
- "Logout đầy đủ" của client = gửi cả `accessToken` lẫn `refreshToken` vào endpoint này.

**Quan trọng**:
- Endpoint này là **admin-style**: chỉ user có `auth.tokens.revoke` mới gọi được, **không tự động** cho phép user revoke token của chính họ. Nếu muốn user bình thường tự logout, hoặc phải thêm `RolesManage` vào role mặc định, hoặc bỏ permission requirement và check `userId == currentUser` trong handler.
- Không có endpoint "revoke tất cả token của user X" (kill switch).

---

## 5. Change password — `POST /api/v1/auth/change-password`

**File**: `Application/Src/Features/Auth/Commands/ChangePasswordCommand.cs`
**Authenticated**, rate-limit `default`.

```
Client (Bearer)
  │ POST { currentPassword, newPassword }
  ▼
Handler
  ├─► userId = currentUserContext.UserId  (từ JWT NameIdentifier/sub) — null → 401
  ├─► user = UserRepository.GetByIdAsync(userId)
  ├─► PasswordService.VerifyPassword(currentPassword) → false → 401
  ├─► user.SetPassword(newHash)   ← regenerate SecurityStamp (nhưng KHÔNG ai đọc nó)
  ├─► UnitOfWork.SaveChangesAsync()
  └─► AuditService.WriteAsync("auth.password.changed", "User", userId, null, "Success")
```

**Quan trọng — đây là vấn đề bảo mật nghiêm trọng**:
- Sau khi đổi mật khẩu, **access token cũ và refresh token cũ vẫn còn hiệu lực**. Không có flow nào revoke token theo `userId`.
- `SecurityStamp` được sinh mới nhưng **không** được check trong `JwtBearerEvents.OnTokenValidated`.
- Hậu quả: nếu mật khẩu bị lộ, attacker đã login → user đổi password → attacker vẫn dùng access token + refresh token cũ tới khi access token hết 15 phút và refresh token hết 7 ngày.

---

## 6. Token validation mỗi request

`Infrastructure/Src/DependencyInjection.cs` cấu hình `JwtBearer`:

```
TokenValidationParameters: ValidateIssuer/Audience/Lifetime/IssuerSigningKey, ClockSkew=30s
JwtBearerEvents.OnTokenValidated:
  └─► jti = principal.FindFirst("jti")
  └─► if RevokedAccessTokenRepository.ExistsActiveAsync(jti) → context.Fail
```

`AuthorizationBuilder.SetFallbackPolicy = RequireAuthenticatedUser()` ⇒ mọi endpoint đều cần auth trừ
khi gọi `.AllowAnonymous()`. Endpoint dùng quyền cụ thể qua `.RequireAuthorization(p => p.RequirePermission("auth.x.y"))`,
được handle bởi `PermissionAuthorizationHandler`:

```
context.User.FindAll("permissions")
  → HashSet (OrdinalIgnoreCase)
  → Contains(requirement.Permission) ? Succeed
```

Cấp quyền dựa trên claim, không hỏi DB ⇒ **thay đổi role/permission trong DB chưa có hiệu lực với token đang còn hạn**.
Nó chỉ áp dụng cho request **sau lần login/refresh kế tiếp**.

---

## 7. RBAC — Role, Permission, Assignment

### 7.1 Tạo permission → tạo role → gán permission cho role → gán role cho user

```
1) POST /permissions       (auth.permissions.manage)  → CreatePermissionCommand
2) POST /roles             (auth.roles.manage)        → CreateRoleCommand
3) POST /roles/{id}/permissions   { permissionIds: [...] }
       └─► AssignPermissionsToRoleCommandHandler
              ├─► roleRepo.GetByIdAsync(includeRolePermissions:true)
              ├─► permissionRepo.GetByIdsAsync — fail nếu thiếu id nào
              └─► roleRepo.AssignPermissionsAsync(role, permissions, assignedBy)
                   └─► REPLACE: xoá row không có trong list, thêm row mới
                       (xem RoleRepository / UserRepository — RemoveRange + Add)
4) POST /users/{id}/roles  { roleIds: [...] }   (auth.users.manage)
       └─► AssignRolesToUserCommandHandler  — cùng pattern REPLACE
```

**Hệ quả của REPLACE**:
- Truyền `[]` không xóa được hết vì validator có `RuleFor(...).NotEmpty()`. Muốn dọn sạch role/permission của 1 entity hiện không có cách qua API.
- Truyền `[a]` cho user đang có `[a, b, c]` ⇒ b, c bị **xoá**. Đây là hành vi đã document (xem CLAUDE.md & architecture.md), nhưng UI client phải gửi *full set*, dễ gây mất quyền nếu sai.

### 7.2 Check quyền của 1 user — `GET /users/{userId}/permissions/{permissionCode}`

```
CheckUserPermissionQueryHandler
  ├─► user = UserRepository.GetByIdAsync(userId)
  ├─► permissions = UserRepository.GetPermissionCodesAsync(userId)
  └─► granted = permissions.Contains(code, OrdinalIgnoreCase)
```

Đây là check **DB-level**, không phụ thuộc claim trong token người gọi → phản ánh state thực.

### 7.3 Hệ quả: lệch nhau giữa DB & token

| Hành động                               | Có hiệu lực ngay? |
|-----------------------------------------|--------------------|
| Tạo/đổi/xoá role / permission           | Có (DB)            |
| Assign role cho user                    | Có (DB), KHÔNG thay đổi token đang dùng |
| Token cũ vẫn pass `permissions` cũ      | Cho đến khi expire hoặc refresh |
| `CheckUserPermissionQuery`              | Đọc trực tiếp DB → đúng state mới |
| `[Authorize].RequirePermission(...)`    | Đọc claim → state cũ trong access token |

Để force apply quyền mới: cần (a) chờ token hết hạn, (b) user refresh, hoặc (c) revoke access token thủ công.
Hiện không có endpoint "revoke all tokens of user" để chủ động apply.

---

## 8. Audit log

`AuditService.WriteAsync` tạo `AuditLog` với `tenantId/userId/ip/userAgent/correlationId` từ
`HttpCurrentUserContext` rồi *gọi `unitOfWork.SaveChangesAsync()` ngay*. Đây là **transaction tách biệt**
khỏi business save, nên:

- Business save thành công ⇒ audit save có thể fail độc lập ⇒ mất audit trail.
- Audit save trước business save (ví dụ failed login) ⇒ vẫn thấy được trong DB ngay cả khi business fail.

Các event hiện được log: `user.registered`, `auth.login` (Success/Failed), `auth.token.refreshed`,
`auth.token.revoked`, `auth.password.changed`, `role.created/updated/deleted/permissions.assigned`,
`permission.created/updated/deleted`, `user.roles.assigned`.

**Không log**: failed login khi email không tồn tại (hand back 401 trước, không có entityId nên skip).

---

## 9. Audit field stamping trên entity

`AppDbContext.SaveChangesAsync` override gọi `ApplyAuditFields()`:
- `EntityState.Added`: `CreatedAtUtc=now`, `CreatedBy = currentUser.Email ?? "system"` (chỉ set nếu null)
- `EntityState.Added/Modified`: `LastModifiedAtUtc=now`, `LastModifiedBy = currentUser.Email ?? "system"`

**Cảnh báo**:
- Override chỉ chặn version `SaveChangesAsync(CancellationToken)`. Nếu code nào gọi `SaveChanges()` (sync) hay
  `SaveChangesAsync(bool, CancellationToken)` thì audit field sẽ KHÔNG được stamp.

---

## 10. Domain events

`User` raise `UserRegisteredDomainEvent` trong constructor. `EventDispatchInterceptor`
(`SaveChangesInterceptor.SavingChangesAsync`) lấy mọi entity `IHasDomainEvents`, dispatch qua
`MediatorDomainEventDispatcher` rồi clear. Hiện chỉ có `UserRegisteredDomainEventHandler` log Information,
không gửi email confirm hay flow nào khác — đây là **stub**.

---

## 11. Seeding

`Api/Program.cs` gọi `app.Services.InitializeDatabaseAsync()` (`OpenApiExtensions.cs`):

```
Database.MigrateAsync()
DbSeeder.SeedAsync()
  └─► nếu Users.Any() → return ngay (idempotent)
  └─► tạo Tenant("Default Tenant", "default")
  └─► tạo 7 Permission từ PermissionCodes.All
  └─► tạo Role("Administrator", isSystem=true), Role("User")
  └─► gán toàn bộ 7 permission vào Administrator (không vào User)
  └─► tạo admin user (email/userName/password từ Seed config), Status=Active, EmailConfirmed=true
  └─► gán admin → Administrator
  └─► tạo ClientApp("default-web", Public)
  └─► SaveChangesAsync()
```

Lưu ý:
- **`DbSeeder` tự new `PasswordService()`** thay vì lấy từ DI. Hiện service này không có dependency,
  nên không sao, nhưng đây là code smell.
- Role `User` không có permission nào → user mặc định login được nhưng không có quyền gì.

---

## 12. Tenant model — hiện trạng

- `User`, `Role`, `ClientApp` có `TenantId?` (nullable).
- Không có endpoint nào để CRUD tenant.
- `RegisterUserCommand` nhận `tenantId` từ body **không validate**.
- `Role` unique theo `(TenantId, NormalizedName)` — đúng với multi-tenant logic.
- Token claim `tenant_id` chỉ embed nếu user có TenantId, nhưng *không có chỗ nào filter dữ liệu theo tenant_id của caller*.
  ⇒ Multi-tenancy mới có ở mức data model, **chưa có enforcement** ở handler.

---

## 13. Bảng trạng thái nhanh (đánh giá)

| Khu vực             | Đánh giá                                  |
|---------------------|-------------------------------------------|
| Layering / DI       | ✅ Sạch, đúng Clean Architecture           |
| MediatR + Validator | ✅ Pipeline chuẩn                         |
| JWT issue + sign    | ✅ HS256, claim đầy đủ                    |
| Refresh rotation    | ✅ Hash + rotation + replacement link    |
| Access revocation   | ✅ Denylist `jti` qua middleware         |
| Permission claim    | ✅ Embedded vào token, kiểm tra qua handler |
| Audit log           | ⚠ 2-transaction, có thể mất nếu audit fail |
| Change password     | ❌ Không revoke token cũ (xem #5)         |
| User.Status check   | ❌ Không enforce ở Login/Refresh         |
| ClientApp validate  | ❌ Không verify secret, không check IsActive |
| Logout cá nhân      | ❌ Không có endpoint "self-revoke"       |
| Tenant enforcement  | ❌ Chưa có (model có, runtime chưa)      |
| Email confirm flow  | ❌ Chỉ có cờ, không có flow gửi mail     |
| Domain event handler| ⚠ Stub (chỉ log)                          |
| Multi-device cap    | ❌ Không giới hạn số refresh token        |
| Cleanup expired     | ❌ Không có job dọn refresh_tokens / revoked_access_tokens |
| Logging payload     | ❌ Log password/refresh-token nguyên văn (xem Vấn đề #1) |

---

## 14. Sequence diagram tóm tắt

### Login + use API + refresh + logout

```
Client                    API                   DB
  │ POST /login            │                     │
  ├───────────────────────►│ verify pwd          │
  │                        ├────────────────────►│ users
  │                        │ insert refresh row  │
  │                        ├────────────────────►│ refresh_tokens
  │                        │ stamp lastLogin     │
  │ access+refresh         │                     │
  │◄───────────────────────┤                     │
  │                        │                     │
  │ GET /api/.../X (Bearer)│                     │
  ├───────────────────────►│ JwtBearer validate  │
  │                        │ check denylist jti  │
  │                        ├────────────────────►│ revoked_access_tokens
  │                        │ PermissionHandler   │
  │                        │ → handler           │
  │                        │                     │
  │ POST /refresh-token    │                     │
  ├───────────────────────►│ hash, lookup        │
  │                        ├────────────────────►│ refresh_tokens
  │                        │ generate new pair   │
  │                        │ Revoke(old, Rotated)│
  │                        │ insert(new)         │
  │                        ├────────────────────►│
  │ new access+refresh     │                     │
  │◄───────────────────────┤                     │
  │                        │                     │
  │ POST /revoke {access,refresh}                │
  ├───────────────────────►│ deny jti            │
  │                        ├────────────────────►│ revoked_access_tokens
  │                        │ revoke refresh row  │
  │                        ├────────────────────►│ refresh_tokens
  │ 204                    │                     │
  │◄───────────────────────┤                     │
```
