# Kategori Yönetimi - Category Management

## 🎯 Overview

Merchant Portal'a **tam özellikli kategori yönetimi** eklendi. Hierarchical (hiyerarşik) yapı ile kategorileri organize edebilir, alt kategoriler oluşturabilirsiniz.

---

## ✅ Tamamlanan Özellikler

### 1. **Kategori Servisleri**
- ✅ `ICategoryService` - Interface
- ✅ `CategoryService` - Implementation
- ✅ CRUD operations (Create, Read, Update, Delete)
- ✅ Hierarchical tree building
- ✅ Parent-child relationship management

### 2. **Categories Controller**
- ✅ `/Categories/Index` - Kategori listesi (tree view)
- ✅ `/Categories/Create` - Yeni kategori ekleme
- ✅ `/Categories/Edit/{id}` - Kategori düzenleme
- ✅ `POST /Categories/Delete/{id}` - Kategori silme

### 3. **Hierarchical Tree View**
- ✅ **Visual tree structure** - Ana ve alt kategoriler
- ✅ **Expand/collapse** - Alt kategorileri göster/gizle
- ✅ **Level indicators** - Seviye bazlı renklendirme
- ✅ **Product count** - Her kategoride kaç ürün var
- ✅ **Child count** - Kaç alt kategori var

### 4. **Category CRUD**
- ✅ **Create category** - Parent selection ile
- ✅ **Edit category** - Tüm alanlar düzenlenebilir
- ✅ **Delete category** - Güvenli silme (ürün/alt kategori kontrolü)
- ✅ **Validation** - Form validasyonları

### 5. **UI Features**
- ✅ **Statistics panel** - Kategori istatistikleri
- ✅ **Top categories** - En çok ürünlü kategoriler
- ✅ **Empty state** - İlk kategori ekleme daveti
- ✅ **Tooltips & help text** - Kullanıcı rehberliği

---

## 🏗️ Teknik İmplementasyon

### **Service Layer**

```csharp
public interface ICategoryService
{
    Task<List<ProductCategoryResponse>?> GetCategoriesAsync();
    Task<List<ProductCategoryResponse>?> GetMyCategoriesAsync();
    Task<ProductCategoryResponse?> GetCategoryByIdAsync(Guid categoryId);
    Task<ProductCategoryResponse?> CreateCategoryAsync(CreateCategoryRequest request);
    Task<ProductCategoryResponse?> UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(Guid categoryId);
    Task<List<CategoryTreeNode>?> GetCategoryTreeAsync(); // Hierarchical
}
```

### **Hierarchical Tree Building**

```csharp
private List<CategoryTreeNode> BuildCategoryTree(List<ProductCategoryResponse> categories)
{
    var categoryDict = categories.ToDictionary(c => c.Id, c => new CategoryTreeNode
    {
        Category = c,
        Children = new List<CategoryTreeNode>()
    });

    var rootNodes = new List<CategoryTreeNode>();

    foreach (var node in categoryDict.Values)
    {
        if (node.Category.ParentCategoryId.HasValue && 
            categoryDict.TryGetValue(node.Category.ParentCategoryId.Value, out var parentNode))
        {
            parentNode.Children.Add(node);
        }
        else
        {
            rootNodes.Add(node);
        }
    }

    return rootNodes;
}
```

---

## 🎨 UI Components

### **1. Category Tree View**

**Ana Kategoriler (Level 0):**
- Mor gradient background
- Folder icon
- Mor border (sol tarafta kalın)

**Alt Kategoriler (Level 1):**
- Beyaz background
- Folder-open icon
- Gri border
- 2rem sol margin

**Alt Alt Kategoriler (Level 2+):**
- Beyaz background
- Tag icon
- Açık gri border
- 4rem sol margin

### **2. Category Node Structure**

```html
<div class="category-node level-0">
    <div class="d-flex align-items-center">
        <i class="fas fa-chevron-down category-toggle"></i> <!-- If has children -->
        <div class="category-icon level-0">
            <i class="fas fa-folder"></i>
        </div>
        <div class="flex-grow-1">
            <h6>Kategori Adı</h6>
            <small>Açıklama</small>
            <small>📦 5 ürün | 🌲 3 alt kategori</small>
        </div>
        <div class="btn-group">
            <button>Edit</button>
            <button>Delete</button>
        </div>
    </div>
    <div class="category-children">
        <!-- Child nodes -->
    </div>
</div>
```

### **3. Statistics Panel**

```
📊 İstatistikler
├─ Toplam Kategori: 12
├─ Ana Kategoriler: 4
├─ Alt Kategoriler: 8
├─ Aktif Kategoriler: 11
└─ Toplam Ürün: 156

🏆 En Çok Ürünlü Kategoriler
1. Meyve & Sebze - 45 ürün
2. İçecekler - 32 ürün
3. Kahvaltılık - 28 ürün
```

---

## 🔧 API Endpoints

### **Backend Endpoints (WebApi):**

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/productcategory` | Tüm kategoriler |
| GET | `/api/v1/productcategory/my-categories` | Merchant'ın kategorileri |
| GET | `/api/v1/productcategory/{id}` | Kategori detayı |
| POST | `/api/v1/productcategory` | Yeni kategori |
| PUT | `/api/v1/productcategory/{id}` | Kategori güncelle |
| DELETE | `/api/v1/productcategory/{id}` | Kategori sil |

### **Request/Response Models:**

```csharp
// Create Request
public class CreateCategoryRequest
{
    public Guid? ParentCategoryId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

// Update Request
public class UpdateCategoryRequest
{
    // Same as Create
}

// Response
public class ProductCategoryResponse
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 📝 User Workflows

### **1. Kategori Oluşturma**

```
1. /Categories sayfasına git
2. "Yeni Kategori Ekle" butonuna tıkla
3. Form doldur:
   - Üst Kategori seç (optional)
   - İsim gir (required)
   - Açıklama (optional)
   - Görsel URL (optional)
   - Sıralama (default: 0)
   - Aktif/Pasif (default: aktif)
4. "Kategoriyi Kaydet" tıkla
5. /Categories'e redirect, success toast
```

### **2. Hiyerarşik Yapı Oluşturma**

```
Örnek Yapı:
├─ Gıda (Ana kategori)
│  ├─ Meyve & Sebze (Alt kategori)
│  │  ├─ Meyveler (Alt alt kategori)
│  │  └─ Sebzeler
│  ├─ İçecekler
│  │  ├─ Meşrubat
│  │  └─ Meyve Suları
│  └─ Kahvaltılık
└─ Temizlik (Ana kategori)
   ├─ Ev Temizliği
   └─ Kişisel Bakım
```

**Nasıl Yapılır:**
1. İlk önce "Gıda" ana kategorisini oluştur
2. Sonra "Meyve & Sebze" oluştururken üst kategori olarak "Gıda" seç
3. "Meyveler" oluştururken üst kategori "Meyve & Sebze" seç

### **3. Kategori Silme**

**Güvenli Silme Kuralları:**
- ✅ **Silinebilir:** Ürün yok + Alt kategori yok
- ❌ **Silinemez:** Ürün var
- ❌ **Silinemez:** Alt kategori var

**UI Behavior:**
- Silinebilir kategoriler: 🗑️ Delete button (kırmızı)
- Silinemez kategoriler: 🔒 Locked button (disabled, gri)

---

## 🎯 Features & Highlights

### **1. Expand/Collapse**
- Alt kategorileri göster/gizle
- Smooth slide animation
- Chevron icon rotation

### **2. Visual Hierarchy**
```css
Level 0 (Ana):       Mor gradient + kalın border
Level 1 (Alt):       Beyaz + gri border + 2rem indent
Level 2 (Alt Alt):   Beyaz + açık gri + 4rem indent
```

### **3. Smart Delete Protection**
```javascript
if (category.ProductCount > 0 || category.Children.Any())
{
    // Show locked icon
    // Disable delete button
    // Tooltip: "Bu kategori silinemez"
}
```

### **4. Parent Selection**
```html
<select asp-for="ParentCategoryId">
    <option value="">Ana Kategori</option>
    @foreach (var category in categories)
    {
        <option value="@category.Id">
            @if (category.ParentCategoryId != null) { └─ }
            @category.Name (@category.ProductCount ürün)
        </option>
    }
</select>
```

---

## 🧪 Test Scenarios

### **Senaryo 1: Ana Kategori Oluşturma**
```
Input:
  Name: "Gıda"
  ParentCategoryId: null
  IsActive: true

Expected:
  - Kategori oluşturulur
  - Level 0 olarak gösterilir
  - Mor gradient background
```

### **Senaryo 2: Alt Kategori Oluşturma**
```
Input:
  Name: "Meyve & Sebze"
  ParentCategoryId: {Gıda-ID}
  IsActive: true

Expected:
  - "Gıda" kategorisi altında görünür
  - Level 1 olarak gösterilir
  - 2rem indent
```

### **Senaryo 3: Kategori Silme Engelleme**
```
Given: Kategori "Gıda" var
And: "Gıda" altında 5 ürün var

When: Delete butonuna tıkla

Then:
  - Delete button disabled
  - Locked icon gösterilir
  - Tooltip: "Bu kategori silinemez (ürün veya alt kategori var)"
```

---

## 🔐 Security & Validation

### **Authorization:**
```csharp
[Authorize] // Sadece giriş yapmış kullanıcılar
public class CategoriesController : Controller
```

### **Validation:**
```csharp
[Required(ErrorMessage = "Kategori adı zorunludur")]
[StringLength(100, MinimumLength = 2)]
public string Name { get; set; }

[Range(0, 9999)]
public int DisplayOrder { get; set; }
```

### **Business Rules:**
- ✅ Kategori ismi unique olmalı (backend kontrolü)
- ✅ Parent kategori kendisi olamaz
- ✅ Circular reference kontrolü (A → B → A yasak)
- ✅ Soft delete (IsActive = false)

---

## 📊 Database Schema

```sql
CREATE TABLE ProductCategories (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    ParentCategoryId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    ImageUrl NVARCHAR(500) NULL,
    DisplayOrder INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    
    FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    FOREIGN KEY (ParentCategoryId) REFERENCES ProductCategories(Id)
);

-- Index for hierarchy queries
CREATE INDEX IX_ProductCategories_ParentCategoryId 
ON ProductCategories(ParentCategoryId);

-- Index for merchant queries
CREATE INDEX IX_ProductCategories_MerchantId 
ON ProductCategories(MerchantId);
```

---

## 🚀 Usage Examples

### **Dashboard'dan Kategori Ekleme:**
```
1. Sidebar → "Kategoriler" tıkla
2. "Yeni Kategori Ekle" buton
3. Form doldur ve kaydet
4. Tree view'da görüntüle
```

### **Ürün Eklerken Kategori Seçme:**
```
1. "Ürünler" → "Yeni Ürün Ekle"
2. "Kategori" dropdown'dan seç
3. Hierarchical liste gösterilir:
   - Gıda
     └─ Meyve & Sebze
        └─ Meyveler  ← Bunu seç
4. Ürün o kategoriye atanır
```

---

## 💡 Best Practices

### **Kategori İsimlendirme:**
```
✅ İyi:
- Meyve & Sebze
- İçecekler
- Kahvaltılık
- Ev Temizliği

❌ Kötü:
- Kategori 1
- Test
- aaaaa
- Çok uzun ve anlamsız kategori ismi burada yazıyor
```

### **Hiyerarşi Derinliği:**
```
✅ Önerilen: Max 3 seviye
- Ana Kategori (Level 0)
  └─ Alt Kategori (Level 1)
     └─ Alt Alt Kategori (Level 2)

❌ Önberilmeyen: 4+ seviye
- Çok derin hiyerarşi kullanıcıyı karıştırır
```

### **DisplayOrder Kullanımı:**
```
✅ İyi:
- Meyve & Sebze (0)
- İçecekler (10)
- Kahvaltılık (20)
- Temizlik (30)

← 10'ar artırarak yer bırak, ortaya eklemek için
```

---

## 🐛 Troubleshooting

### **Problem: Kategori silinmiyor**
```
Hata: "Kategori silinirken bir hata oluştu"

Çözüm:
1. Bu kategoride ürün var mı? → Önce ürünleri başka kategoriye taşı
2. Alt kategorisi var mı? → Önce alt kategorileri sil
3. Backend loglara bak
```

### **Problem: Tree view gösterilmiyor**
```
Sebep: ParentCategoryId ilişkisi yanlış

Çözüm:
1. Circular reference var mı kontrol et (A → B → A)
2. ParentCategoryId geçerli mi kontrol et
3. Backend'de GetCategoryTreeAsync() kontrol et
```

---

## 📚 API Documentation

Full API documentation for category endpoints:

**GET /api/v1/productcategory/my-categories**
```json
Response: [
  {
    "id": "guid",
    "merchantId": "guid",
    "parentCategoryId": null,
    "name": "Gıda",
    "description": "Gıda ürünleri",
    "imageUrl": "https://...",
    "displayOrder": 0,
    "isActive": true,
    "productCount": 45,
    "createdAt": "2025-10-13T..."
  }
]
```

**POST /api/v1/productcategory**
```json
Request: {
  "parentCategoryId": "guid-or-null",
  "name": "Yeni Kategori",
  "description": "Açıklama",
  "imageUrl": "https://...",
  "displayOrder": 0,
  "isActive": true
}

Response: {
  "success": true,
  "value": { /* CategoryResponse */ }
}
```

---

**✨ Kategori Yönetimi: TAMAMLANDI!**

Hierarchical tree view, CRUD operations, smart delete protection ve modern UI ile tam özellikli kategori sistemi hazır! 🎉

