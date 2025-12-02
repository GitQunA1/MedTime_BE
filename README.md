# 💊 MedTime - Ứng Dụng Nhắc Uống Thuốc Thông Minh

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white" />
  <img src="https://img.shields.io/badge/Firebase-FFCA28?style=for-the-badge&logo=firebase&logoColor=black" />
  <img src="https://img.shields.io/badge/PayOS-00D4AA?style=for-the-badge&logoColor=white" />
</p>

## 📋 Mục Lục

- [Giới Thiệu](#-giới-thiệu)
- [Tính Năng Chính](#-tính-năng-chính)
- [Công Nghệ Sử Dụng](#-công-nghệ-sử-dụng)
- [Cấu Trúc Dự Án](#-cấu-trúc-dự-án)
- [Database Schema](#-database-schema)
- [API Endpoints](#-api-endpoints)
- [Cài Đặt & Chạy](#-cài-đặt--chạy)
- [Biến Môi Trường](#-biến-môi-trường)
- [Tích Hợp Bên Thứ 3](#-tích-hợp-bên-thứ-3)

---

## 🎯 Giới Thiệu

**MedTime** là ứng dụng hỗ trợ người dùng quản lý và nhắc nhở uống thuốc đúng giờ. Ứng dụng được thiết kế đặc biệt cho:

- 👴 **Người cao tuổi** cần nhắc nhở uống thuốc thường xuyên
- 👨‍👩‍👧 **Người thân/Giám hộ** muốn theo dõi việc uống thuốc của người thân
- 🏥 **Bệnh nhân mãn tính** cần tuân thủ phác đồ điều trị phức tạp

### Bối cảnh

Việc quên uống thuốc là vấn đề phổ biến, đặc biệt với người cao tuổi và bệnh nhân mãn tính. MedTime giải quyết vấn đề này bằng cách:

1. **Nhắc nhở thông minh** qua push notification
2. **Theo dõi từ xa** - Người thân có thể giám sát việc uống thuốc
3. **Báo cáo chi tiết** về mức độ tuân thủ uống thuốc
4. **Gọi điện thoại** khi không phản hồi notification

---

## ✨ Tính Năng Chính

### 1. 📝 Quản Lý Thuốc & Đơn Thuốc

| Tính năng | Mô tả |
|-----------|-------|
| **Danh mục thuốc** | Quản lý thông tin thuốc (tên, loại, liều lượng, hình ảnh) |
| **Đơn thuốc (Prescription)** | Tạo đơn thuốc với thông tin bác sĩ, ngày bắt đầu/kết thúc |
| **Lịch uống thuốc** | Tự động tạo lịch dựa trên tần suất uống/ngày |
| **Số lượng còn lại** | Theo dõi số thuốc còn lại để nhắc mua thêm |

### 2. ⏰ Nhắc Nhở Thông Minh

| Tính năng | Mô tả |
|-----------|-------|
| **Push Notification** | Gửi thông báo qua Firebase Cloud Messaging |
| **Hỗ trợ iOS & Android** | Cấu hình riêng cho từng platform |
| **Repeat Pattern** | Hỗ trợ: Hàng ngày, theo ngày trong tuần, ngày cụ thể |
| **Ghi nhận phản hồi** | Đã uống, Hoãn, Bỏ qua, Không phản hồi |

### 3. 👨‍👩‍👧 Tính Năng Giám Hộ (Guardian)

| Tính năng | Mô tả |
|-----------|-------|
| **Thêm người được giám hộ** | Nhập mã UniqueCode 6 số để liên kết |
| **Quản lý thuốc từ xa** | Guardian có thể thêm/sửa/xóa thuốc cho patient |
| **Xem báo cáo** | Xem thống kê tuân thủ uống thuốc của patient |
| **Xem thống kê** | Dashboard, trends của patient |

### 4. 📊 Báo Cáo & Thống Kê

| Báo cáo | Nội dung |
|---------|----------|
| **Adherence Report** | Tỷ lệ tuân thủ, phân tích theo thuốc/thời gian trong ngày |
| **Missed Doses** | Chi tiết các lần bỏ uống thuốc |
| **Medicine Usage** | Thống kê sử dụng theo loại thuốc |
| **Dashboard** | Tổng quan: số thuốc, tỷ lệ hôm nay, chuỗi uống liên tiếp |
| **Trends** | Xu hướng theo ngày/tuần/tháng |

### 5. 💳 Thanh Toán Premium (PayOS)

| Gói | Thời hạn | Quyền lợi |
|-----|----------|-----------|
| **1 Tháng** | 30 ngày | Không giới hạn số đơn thuốc |
| **3 Tháng** | 90 ngày | Giảm giá + không giới hạn |
| **1 Năm** | 365 ngày | Giảm giá nhiều nhất |

**Lưu ý:** User miễn phí chỉ được tạo tối đa **2 đơn thuốc**.

### 6. 📞 Cuộc Gọi & Liên Hệ Khẩn Cấp

| Tính năng | Mô tả |
|-----------|-------|
| **Call Log** | Ghi nhận lịch sử cuộc gọi nhắc thuốc |
| **Emergency Contact** | Danh sách liên hệ khẩn cấp |
| **Call Status** | Trạng thái: Đã gọi, Không nghe máy, Bận, Thất bại |

### 7. 📅 Lịch Hẹn Khám Bệnh

- Quản lý lịch hẹn với bác sĩ
- Thông tin bệnh viện, ghi chú

---

## 🛠 Công Nghệ Sử Dụng

### Backend Framework
| Công nghệ | Version | Mục đích |
|-----------|---------|----------|
| **.NET** | 8.0 | Web API Framework |
| **ASP.NET Core** | 8.0 | HTTP Request handling |
| **Entity Framework Core** | 8.0.20 | ORM |

### Database
| Công nghệ | Mục đích |
|-----------|----------|
| **PostgreSQL** | Database chính |
| **Npgsql** | PostgreSQL driver cho .NET |
| **PostgreSQL Enums** | Lưu trữ enum an toàn |

### Authentication & Security
| Công nghệ | Mục đích |
|-----------|----------|
| **JWT Bearer** | Xác thực API |
| **ASP.NET Identity** | Password hashing |
| **Refresh Token** | Token làm mới |

### Third-party Services
| Dịch vụ | Mục đích |
|---------|----------|
| **Firebase Cloud Messaging (FCM)** | Push notification iOS/Android |
| **PayOS** | Cổng thanh toán Việt Nam |

### Libraries
| Package | Version | Mục đích |
|---------|---------|----------|
| **AutoMapper** | 12.0.0 | Object mapping |
| **Swashbuckle** | 6.6.2 | Swagger/OpenAPI documentation |
| **Hangfire** | 1.8.21 | Background jobs (dự kiến) |

---

## 📁 Cấu Trúc Dự Án

```
MedTime/
├── Controllers/           # API Controllers
│   ├── AuthController.cs          # Đăng nhập, đăng ký, refresh token
│   ├── UserController.cs          # Quản lý user
│   ├── MedicineController.cs      # CRUD thuốc
│   ├── PrescriptionController.cs  # CRUD đơn thuốc
│   ├── PrescriptionscheduleController.cs  # Lịch uống thuốc
│   ├── IntakelogController.cs     # Ghi nhận uống thuốc
│   ├── GuardianlinkController.cs  # Quản lý giám hộ
│   ├── ReportController.cs        # Báo cáo
│   ├── StatisticsController.cs    # Thống kê
│   ├── NotificationController.cs  # Gửi notification
│   ├── PaymentController.cs       # Thanh toán user
│   ├── AdminPaymentController.cs  # Analytics thanh toán (Admin)
│   ├── AppointmentController.cs   # Lịch hẹn khám
│   ├── CalllogController.cs       # Lịch sử cuộc gọi
│   └── EmergencycontactController.cs  # Liên hệ khẩn cấp
│
├── Services/              # Business Logic
│   ├── AuthService.cs             # Xác thực
│   ├── UserService.cs             # Logic user
│   ├── MedicineService.cs         # Logic thuốc
│   ├── PrescriptionService.cs     # Logic đơn thuốc
│   ├── PrescriptionscheduleService.cs  # Logic lịch
│   ├── IntakelogService.cs        # Logic ghi nhận
│   ├── GuardianlinkService.cs     # Logic giám hộ
│   ├── ReportService.cs           # Logic báo cáo
│   ├── FirebaseService.cs         # Push notification
│   ├── NotificationService.cs     # Notification logic
│   ├── PaymentService.cs          # Tích hợp PayOS
│   ├── PaymentAnalyticsService.cs # Thống kê thanh toán
│   ├── TokenCacheService.cs       # Cache refresh token
│   └── ...
│
├── Repositories/          # Data Access Layer
│   ├── BaseRepo.cs               # Generic repository
│   ├── UserRepo.cs
│   ├── MedicineRepo.cs
│   ├── PrescriptionRepo.cs
│   ├── GuardianlinkRepo.cs
│   ├── ReportRepo.cs
│   └── ...
│
├── Models/
│   ├── Entities/          # Database entities
│   │   ├── User.cs
│   │   ├── Medicine.cs
│   │   ├── Prescription.cs
│   │   ├── Prescriptionschedule.cs
│   │   ├── Intakelog.cs
│   │   ├── Guardianlink.cs
│   │   ├── Notificationhistory.cs
│   │   ├── Devicetoken.cs
│   │   ├── Premiumplan.cs
│   │   ├── Paymenthistory.cs
│   │   └── ...
│   │
│   ├── DTOs/              # Data Transfer Objects
│   ├── Requests/          # Request models
│   ├── Responses/         # Response models
│   └── Enums/             # Enumerations
│       ├── UserRoleEnum.cs        # USER, ADMIN
│       ├── MedicineTypeEnum.cs    # TABLET, CAPSULE, LIQUID...
│       ├── IntakeActionEnum.cs    # TAKEN, SKIPPED, POSTPONED...
│       ├── RepeatPatternEnum.cs   # DAILY, WEEKLY...
│       ├── PaymentStatusEnum.cs   # PENDING, PAID, FAILED...
│       └── ...
│
├── Data/
│   └── MedTimeDBContext.cs  # EF Core DbContext
│
├── Helpers/
│   ├── ApiResponse.cs            # Chuẩn hóa response
│   ├── JwtHelper.cs              # JWT utilities
│   ├── MappingProfile.cs         # AutoMapper config
│   └── PaginationExtensions.cs   # Phân trang
│
├── Settings/
│   ├── JwtSettings.cs
│   └── PayOSSettings.cs
│
├── docs/
│   └── GUARDIAN_FEATURE_API_DOCS.md  # API docs tính năng Guardian
│
├── appsettings.json       # Configuration
├── Program.cs             # Application entry point
└── Dockerfile             # Docker configuration
```

---

## 🗄 Database Schema

### Các Bảng Chính

```
┌─────────────┐     ┌──────────────┐     ┌─────────────────────┐
│    User     │────<│ Prescription │────<│ Prescriptionschedule│
└─────────────┘     └──────────────┘     └─────────────────────┘
      │                   │                        │
      │                   │                        │
      │                   ▼                        ▼
      │            ┌─────────────┐          ┌───────────┐
      │            │  Medicine   │          │ Intakelog │
      │            └─────────────┘          └───────────┘
      │
      ├────<┌───────────────┐
      │     │ Guardianlink  │ (Guardian ──── Patient)
      │     └───────────────┘
      │
      ├────<┌────────────────────┐
      │     │ Notificationhistory│
      │     └────────────────────┘
      │
      ├────<┌──────────────┐
      │     │ Devicetoken  │
      │     └──────────────┘
      │
      ├────<┌────────────────┐     ┌─────────────┐
      │     │ Paymenthistory │────>│ Premiumplan │
      │     └────────────────┘     └─────────────┘
      │
      ├────<┌──────────────────┐
      │     │ Emergencycontact │
      │     └──────────────────┘
      │
      ├────<┌─────────────┐
      │     │ Appointment │
      │     └─────────────┘
      │
      └────<┌───────────┐
            │  Calllog  │
            └───────────┘
```

### Enums trong Database (PostgreSQL)

| Enum | Giá trị |
|------|---------|
| `user_role` | USER, ADMIN |
| `medicine_type` | TABLET, CAPSULE, LIQUID, INJECTION, POWDER, CREAM, DROPS, INHALER, PATCH, SUPPOSITORY |
| `medicine_unit` | MG, ML, MCG, G, IU, PERCENT |
| `repeat_pattern` | DAILY, SPECIFIC_DAYS, INTERVAL |
| `intake_action` | TAKEN, POSTPONED, SKIPPED, NO_RESPONSE |
| `confirmed_by` | APP, CALL, GUARDIAN |
| `call_status` | INITIATED, ANSWERED, NO_ANSWER, BUSY, FAILED |
| `notification_status` | SENT, FAILED, PENDING |
| `payment_status` | PENDING, PAID, FAILED, CANCELLED |
| `premium_plan_type` | MONTHLY, QUARTERLY, YEARLY |

---

## 🔌 API Endpoints

### Authentication
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| POST | `/api/auth/register` | Đăng ký tài khoản |
| POST | `/api/auth/login` | Đăng nhập |
| POST | `/api/auth/refresh` | Làm mới token |
| POST | `/api/auth/logout` | Đăng xuất |

### User Management
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/user` | Danh sách users (Admin) |
| GET | `/api/user/{id}` | Chi tiết user |
| PUT | `/api/user/{id}` | Cập nhật profile |

### Medicines
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/medicine` | Danh sách thuốc |
| GET | `/api/medicine/{id}` | Chi tiết thuốc |
| POST | `/api/medicine` | Thêm thuốc |
| PUT | `/api/medicine/{id}` | Sửa thuốc |
| DELETE | `/api/medicine/{id}` | Xóa thuốc |

### Prescriptions
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/prescription?patientId={id}` | Danh sách đơn thuốc |
| POST | `/api/prescription?patientId={id}` | Tạo đơn thuốc (cho mình/patient) |
| PUT | `/api/prescription/{id}` | Cập nhật đơn thuốc |
| DELETE | `/api/prescription/{id}` | Xóa đơn thuốc |

### Guardian Link
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/guardianlink/my-patients` | Danh sách patients đang theo dõi |
| GET | `/api/guardianlink/my-guardians` | Danh sách guardians đang theo dõi mình |
| POST | `/api/guardianlink` | Thêm patient (bằng uniquecode) |
| DELETE | `/api/guardianlink/{guardianId}/{patientId}` | Xóa liên kết |

### Reports & Statistics
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/report/adherence?userId={id}` | Báo cáo tuân thủ |
| GET | `/api/report/missed-doses?userId={id}` | Báo cáo bỏ uống |
| GET | `/api/report/medicine-usage?userId={id}` | Báo cáo sử dụng thuốc |
| GET | `/api/statistics/dashboard?userId={id}` | Dashboard thống kê |
| GET | `/api/statistics/trends?userId={id}` | Xu hướng |

### Payments
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/payment/plans` | Danh sách gói Premium |
| POST | `/api/payment/create` | Tạo link thanh toán PayOS |
| GET | `/api/payment/status/{orderId}` | Kiểm tra trạng thái |
| POST | `/api/payment/payos-callback` | Webhook từ PayOS |

### Admin Payment Analytics
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/admin/payment/summary` | Tổng quan doanh thu |
| GET | `/api/admin/payment/daily-revenue` | Doanh thu theo ngày |
| GET | `/api/admin/payment/plan-breakdown` | Phân tích theo gói |
| GET | `/api/admin/payment/transactions` | Danh sách giao dịch |

---

## 🚀 Cài Đặt & Chạy

### Yêu Cầu
- .NET 8.0 SDK
- PostgreSQL 14+
- Firebase Project (để gửi notification)
- PayOS Account (để thanh toán)

### Các Bước

1. **Clone repository**
```bash
git clone https://github.com/GitQunA1/MedTime_BE.git
cd MedTime_BE/MedTime
```

2. **Cấu hình appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Port=...;Database=...;Username=...;Password=..."
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "https://api.medtime.vn",
    "Audience": "https://app.medtime.vn",
    "ExpirationMinutes": 60
  },
  "PayOSSettings": {
    "ClientId": "your-payos-client-id",
    "ApiKey": "your-payos-api-key",
    "ChecksumKey": "your-payos-checksum-key"
  }
}
```

3. **Thêm Firebase credentials**
- Tải file JSON từ Firebase Console
- Đặt vào thư mục gốc với tên `medtime-e523a-firebase-adminsdk-*.json`

4. **Chạy ứng dụng**
```bash
dotnet restore
dotnet run
```

5. **Truy cập Swagger UI**
```
http://localhost:5000/swagger
```

---

## 🔐 Biến Môi Trường

Cho production, sử dụng environment variables:

| Variable | Mô tả |
|----------|-------|
| `ConnectionStrings__DefaultConnection` | Connection string PostgreSQL |
| `JwtSettings__SecretKey` | JWT secret key |
| `PayOSSettings__ClientId` | PayOS Client ID |
| `PayOSSettings__ApiKey` | PayOS API Key |
| `PayOSSettings__ChecksumKey` | PayOS Checksum Key |
| `FIREBASE_CREDENTIALS` | Firebase JSON credentials (stringify) |

---

## 🔗 Tích Hợp Bên Thứ 3

### 1. Firebase Cloud Messaging (FCM)

**Mục đích:** Gửi push notification đến thiết bị iOS/Android

**Cách hoạt động:**
1. Mobile app đăng ký device token với Firebase
2. Gửi token về server qua `/api/devicetoken`
3. Server lưu token và gửi notification khi cần

**Tính năng:**
- Gửi đến 1 device
- Gửi multicast (max 500 devices)
- Gửi theo topic
- Hỗ trợ data payload

```csharp
// Ví dụ gửi notification
await _firebaseService.SendNotificationAsync(
    deviceToken: "fcm_token_here",
    title: "Đến giờ uống thuốc",
    body: "Paracetamol 500mg - 1 viên",
    data: new Dictionary<string, string> {
        { "prescriptionId", "123" },
        { "scheduleId", "456" }
    }
);
```

### 2. PayOS - Cổng Thanh Toán

**Mục đích:** Xử lý thanh toán gói Premium

**Luồng thanh toán:**
```
1. User chọn gói → POST /api/payment/create
2. Server tạo payment link với PayOS
3. Redirect user đến checkout URL
4. User thanh toán (QR/Banking)
5. PayOS gọi webhook → POST /api/payment/payos-callback
6. Server cập nhật Premium status
```

**Tính năng:**
- Tạo payment link với QR code
- Webhook callback khi thanh toán thành công
- Verify checksum đảm bảo an toàn
- Kiểm tra trạng thái giao dịch

---

## 👥 Roles & Permissions

| Role | Quyền |
|------|-------|
| **USER** | CRUD data của mình, xem data của patients (nếu là guardian) |
| **ADMIN** | Full access tất cả data và analytics |
| **GUARDIAN** | Xem/quản lý thuốc của patients được liên kết |

---

## 📱 Clients

- **Mobile App (React Native):** Ứng dụng cho người dùng cuối
- **Admin Dashboard (Next.js):** Quản trị viên

---

## 📄 License

© 2024 MedTime Team. All rights reserved.

---

## 👨‍💻 Contributors

- Backend Developer: [GitQunA1](https://github.com/GitQunA1)

---

<p align="center">
  Made with ❤️ for Vietnamese healthcare
</p>
