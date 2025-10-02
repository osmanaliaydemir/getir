# 💰 Kapıda Nakit Ödeme Sistemi - Detaylı İş Planı

## 🎯 **GENEL BAKIŞ**

Türkiye'de en yaygın ödeme yöntemi olan **kapıda nakit ödeme** sistemi için optimize edilmiş implementation planı.

### **Avantajlar:**
- ✅ Kredi kartı güvenlik endişesi yok
- ✅ 3D Secure, API entegrasyonu yok
- ✅ Basit implementation
- ✅ Türkiye'de %70+ kullanım oranı
- ✅ Chargeback riski yok

### **Dezavantajlar:**
- ❌ Para toplama riski (kurye)
- ❌ Para üstü verme zorluğu
- ❌ Cash flow gecikmesi
- ❌ Manuel settlement süreci

---

## 📋 **SPRINT 8: CASH PAYMENT SYSTEM** (1 hafta)

### **GÜN 1-2: Database & Entity Design**

#### **1.1 Payment Entity Güncellemesi**
```csharp
public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string PaymentMethod { get; set; } = "Cash"; // "Cash", "CreditCard", "BankTransfer"
    public string Status { get; set; } // "Pending", "Collected", "Settled", "Failed"
    public decimal Amount { get; set; }
    public DateTime? CollectedAt { get; set; }
    public DateTime? SettledAt { get; set; }
    public Guid? CollectedByCourierId { get; set; }
    public string? CollectionNotes { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual Order Order { get; set; }
    public virtual Courier? CollectedByCourier { get; set; }
}
```

#### **1.2 Courier Cash Collection Tracking**
```csharp
public class CourierCashCollection
{
    public Guid Id { get; set; }
    public Guid CourierId { get; set; }
    public Guid PaymentId { get; set; }
    public decimal CollectedAmount { get; set; }
    public DateTime CollectedAt { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } // "Collected", "HandedToMerchant", "HandedToCompany"
    
    // Navigation properties
    public virtual Courier Courier { get; set; }
    public virtual Payment Payment { get; set; }
}
```

#### **1.3 Settlement System**
```csharp
public class CashSettlement
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Commission { get; set; }
    public decimal NetAmount { get; set; }
    public DateTime SettlementDate { get; set; }
    public string Status { get; set; } // "Pending", "Completed", "Failed"
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Merchant Merchant { get; set; }
    public virtual ICollection<Payment> Payments { get; set; }
}
```

### **GÜN 3: Service Implementation**

#### **2.1 Cash Payment Service**
```csharp
public interface ICashPaymentService
{
    Task<Result<PaymentResponse>> CreateCashPaymentAsync(CreateCashPaymentRequest request);
    Task<Result> MarkPaymentAsCollectedAsync(Guid paymentId, Guid courierId, decimal collectedAmount);
    Task<Result> MarkPaymentAsFailedAsync(Guid paymentId, string reason);
    Task<Result<PagedResult<PaymentResponse>>> GetPendingPaymentsAsync(Guid courierId, PaginationQuery query);
    Task<Result<CourierCashSummaryResponse>> GetCourierCashSummaryAsync(Guid courierId, DateTime? date = null);
}
```

#### **2.2 Key Methods Implementation**
```csharp
public class CashPaymentService : BaseService, ICashPaymentService
{
    public async Task<Result<PaymentResponse>> CreateCashPaymentAsync(CreateCashPaymentRequest request)
    {
        // 1. Order validation
        // 2. Create payment record
        // 3. Update order payment status
        // 4. Send notification to courier
        // 5. Return payment response
    }

    public async Task<Result> MarkPaymentAsCollectedAsync(Guid paymentId, Guid courierId, decimal collectedAmount)
    {
        // 1. Validate payment exists and is pending
        // 2. Update payment status to "Collected"
        // 3. Create CourierCashCollection record
        // 4. Update order status if needed
        // 5. Send notification to merchant
    }
}
```

### **GÜN 4: API Endpoints**

#### **3.1 Customer Endpoints**
```csharp
// Customer - Create order with cash payment
POST /api/v1/orders
{
    "merchantId": "guid",
    "items": [...],
    "paymentMethod": "Cash", // Önemli!
    "deliveryAddress": "...",
    "notes": "Para üstü: 5 TL"
}
```

#### **3.2 Courier Endpoints**
```csharp
// Courier - Get pending cash collections
GET /api/v1/courier/payments/pending

// Courier - Mark payment as collected
POST /api/v1/courier/payments/{paymentId}/collect
{
    "collectedAmount": 85.50,
    "notes": "Para üstü: 4.50 TL verildi"
}

// Courier - Mark payment as failed
POST /api/v1/courier/payments/{paymentId}/fail
{
    "reason": "Customer not available",
    "notes": "2 kez arandı, cevap vermedi"
}
```

#### **3.3 Merchant Endpoints**
```csharp
// Merchant - Get cash collection summary
GET /api/v1/merchant/payments/cash-summary

// Merchant - Get settlement history
GET /api/v1/merchant/settlements
```

#### **3.4 Admin Endpoints**
```csharp
// Admin - Process settlements
POST /api/v1/admin/settlements/process

// Admin - View all cash collections
GET /api/v1/admin/payments/cash-collections
```

### **GÜN 5: Business Logic & Validation**

#### **4.1 Cash Payment Validation**
```csharp
public class CashPaymentValidator : AbstractValidator<CreateCashPaymentRequest>
{
    public CashPaymentValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Payment amount must be greater than 0");

        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");
            
        // Para üstü kontrolü
        RuleFor(x => x.ChangeAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Change amount cannot be negative");
    }
}
```

#### **4.2 Courier Collection Validation**
```csharp
public class CourierCollectionValidator : AbstractValidator<CollectPaymentRequest>
{
    public CourierCollectionValidator()
    {
        RuleFor(x => x.CollectedAmount)
            .GreaterThan(0)
            .WithMessage("Collected amount must be greater than 0");

        RuleFor(x => x.CourierId)
            .NotEmpty()
            .WithMessage("Courier ID is required");
    }
}
```

---

## 🔄 **İŞ AKIŞI (Workflow)**

### **1. Order Creation Flow**
```
Customer → Create Order (PaymentMethod: "Cash") 
         → Payment Status: "Pending"
         → Order Status: "Confirmed"
         → Courier Assignment
         → Notification to Courier: "Cash collection required"
```

### **2. Courier Collection Flow**
```
Courier → Arrives at Customer Location
       → Collects Cash Payment
       → Marks Payment as "Collected"
       → Updates Order Status to "Delivered"
       → Handles Change Money
       → Reports to System
```

### **3. Settlement Flow**
```
Daily/Weekly → System calculates merchant earnings
             → Creates Settlement record
             → Admin processes settlement
             → Merchant receives payment
             → Commission deducted
```

---

## 📊 **DATABASE SCHEMA CHANGES**

### **1. Payments Table Update**
```sql
ALTER TABLE Payments ADD COLUMN PaymentMethod NVARCHAR(50) NOT NULL DEFAULT 'Cash';
ALTER TABLE Payments ADD COLUMN CollectedAt DATETIME2 NULL;
ALTER TABLE Payments ADD COLUMN SettledAt DATETIME2 NULL;
ALTER TABLE Payments ADD COLUMN CollectedByCourierId UNIQUEIDENTIFIER NULL;
ALTER TABLE Payments ADD COLUMN CollectionNotes NVARCHAR(500) NULL;
ALTER TABLE Payments ADD COLUMN FailureReason NVARCHAR(500) NULL;

-- Foreign Key
ALTER TABLE Payments ADD CONSTRAINT FK_Payments_Couriers 
    FOREIGN KEY (CollectedByCourierId) REFERENCES Couriers(Id);
```

### **2. New Tables**
```sql
-- Courier Cash Collections
CREATE TABLE CourierCashCollections (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CourierId UNIQUEIDENTIFIER NOT NULL,
    PaymentId UNIQUEIDENTIFIER NOT NULL,
    CollectedAmount DECIMAL(18, 2) NOT NULL,
    CollectedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Notes NVARCHAR(500) NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Collected',
    CONSTRAINT FK_CourierCashCollections_Couriers FOREIGN KEY (CourierId) REFERENCES Couriers(Id),
    CONSTRAINT FK_CourierCashCollections_Payments FOREIGN KEY (PaymentId) REFERENCES Payments(Id)
);

-- Cash Settlements
CREATE TABLE CashSettlements (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    TotalAmount DECIMAL(18, 2) NOT NULL,
    Commission DECIMAL(18, 2) NOT NULL,
    NetAmount DECIMAL(18, 2) NOT NULL,
    SettlementDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    Notes NVARCHAR(500) NULL,
    CONSTRAINT FK_CashSettlements_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id)
);
```

---

## 🎯 **BAŞARIM KRİTERLERİ**

### **Functional Requirements**
- [ ] Customer cash payment ile sipariş verebilmeli
- [ ] Courier cash collection'ı mark edebilmeli
- [ ] Para üstü hesaplama çalışmalı
- [ ] Failed payment handling olmalı
- [ ] Merchant settlement görüntüleyebilmeli

### **Non-Functional Requirements**
- [ ] Payment collection %99+ success rate
- [ ] Real-time payment status updates
- [ ] Courier notification system
- [ ] Audit trail for all cash transactions
- [ ] Daily settlement reports

---

## ⚠️ **RİSK YÖNETİMİ**

### **1. Cash Collection Risks**
- **Risk:** Courier para toplayamıyor
- **Mitigation:** 
  - Customer contact validation
  - Multiple delivery attempts
  - Clear collection procedures
  - Courier training

### **2. Change Money Issues**
- **Risk:** Para üstü hesaplama hataları
- **Mitigation:**
  - Automated change calculation
  - Courier app validation
  - Customer confirmation

### **3. Settlement Delays**
- **Risk:** Merchant'a ödeme gecikmesi
- **Mitigation:**
  - Automated daily settlements
  - Clear settlement terms
  - Merchant dashboard tracking

---

## 📱 **COURIER APP FEATURES**

### **Cash Collection Screen**
```
┌─────────────────────────┐
│ Order #12345            │
│ Amount: ₺85.50          │
│ Customer: John Doe      │
│ Phone: +90 555 123 4567 │
├─────────────────────────┤
│ [COLLECT CASH]          │
│ [MARK AS FAILED]        │
│                         │
│ Change Amount: ₺4.50    │
│ Notes: _______________  │
└─────────────────────────┘
```

### **Daily Cash Summary**
```
┌─────────────────────────┐
│ Today's Collections     │
│ Total: ₺1,250.00        │
│ Orders: 15              │
│ Failed: 2               │
│                         │
│ [HAND OVER TO MERCHANT] │
└─────────────────────────┘
```

---

## 🚀 **IMPLEMENTATION TIMELINE**

| Day | Task | Deliverable |
|-----|------|-------------|
| 1 | Database schema design | Updated Payment entity |
| 2 | Entity implementation | Payment, CourierCashCollection, Settlement |
| 3 | Service layer | ICashPaymentService implementation |
| 4 | API endpoints | RESTful endpoints for all actors |
| 5 | Business logic | Validation, workflow, error handling |
| 6 | Testing | Unit tests, integration tests |
| 7 | Documentation | API docs, courier manual |

---

## 💡 **GELECEKTEKİ İYİLEŞTİRMELER**

### **Phase 2 Enhancements**
- [ ] **QR Code Payment:** Customer QR scan ile ödeme
- [ ] **Mobile Wallet:** BKM Express, Papara entegrasyonu
- [ ] **Digital Receipt:** SMS/Email makbuz gönderimi
- [ ] **Cash Forecasting:** Günlük nakit ihtiyacı tahmini
- [ ] **Fraud Detection:** Şüpheli cash collection patterns

### **Phase 3 Advanced Features**
- [ ] **Installment Payments:** Taksitli ödeme seçeneği
- [ ] **Loyalty Points:** Cash ödeme bonus puanları
- [ ] **Corporate Accounts:** Şirket hesapları için cash payment
- [ ] **Multi-currency:** Farklı para birimleri desteği

---

**Sonuç:** Kapıda nakit ödeme sistemi ile **2 hafta kazandık** ve Türkiye pazarına daha uygun bir çözüm elde ettik! 🎉
