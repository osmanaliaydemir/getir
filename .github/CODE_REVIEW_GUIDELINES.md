# Code Review Guidelines

## 🎯 **Code Review Süreci**

### **1. Review Zorunluluğu**
- Tüm PR'lar en az 1 reviewer tarafından onaylanmalı
- Critical değişiklikler için 2+ reviewer gerekli
- Admin/Lead developer onayı gereken durumlar:
  - Security değişiklikleri
  - Database schema değişiklikleri
  - Breaking changes
  - Performance critical değişiklikler

### **2. Review Checklist**

#### **🔍 Genel Kontroller**
- [x] Kod SOLID prensiplerine uygun mu? ✅ (Repository pattern, Interface segregation, Dependency injection)
- [x] DRY (Don't Repeat Yourself) prensibi uygulanmış mı? ✅ (BaseService, Common utilities)
- [x] Naming conventions doğru mu? ✅ (PascalCase classes, camelCase variables, UPPER_CASE constants)
- [x] Code formatting tutarlı mı? ✅ (EditorConfig ile standartlaştırılmış)
- [x] Gereksiz kod var mı? ✅ (Clean code, minimal implementation)
- [x] Magic numbers/strings kullanılmış mı? ✅ (ErrorCodes class, constants kullanılmış)
- [x] Method/class boyutları uygun mu? (max 50 satır method, 500 satır class) ✅ (Küçük methodlar, uygun class boyutları)
- [x] Cyclomatic complexity düşük mü? (max 10) ✅ (Basit methodlar, az if/else)
- [x] Code duplication var mı? ✅ (BaseService ile ortak kodlar)
- [x] Dead code temizlenmiş mi? ✅ (Kullanılmayan kod temizlenmiş)

#### **🛡️ Security Kontrolleri**
- [x] SQL injection koruması var mı? ✅ (Entity Framework parameterized queries)
- [x] XSS koruması var mı? ✅ (Security headers, input validation)
- [x] Authentication/Authorization doğru mu? ✅ (JWT Bearer, role-based auth)
- [x] Sensitive data expose edilmiş mi? ✅ (DTO'lar ile korunmuş)
- [x] Input validation yapılmış mı? ✅ (FluentValidation ile)
- [x] Password hashing güvenli mi? ✅ (BCrypt ile hash'lenmiş)
- [x] JWT token güvenliği sağlanmış mı? ✅ (HMAC SHA256, secure secret)
- [x] CORS policy doğru mu? ✅ (SignalR için yapılandırılmış)
- [x] Rate limiting uygulanmış mı? ✅ (IP ve Client rate limiting)
- [x] Logging'de sensitive data var mı? ✅ (Sensitive data loglanmıyor)
- [x] Error message'lerde sensitive bilgi var mı? ✅ (Generic error messages)
- [x] HTTPS zorunlu mu? ✅ (HTTPS redirection aktif)

#### **⚡ Performance Kontrolleri**
- [x] N+1 query problemi var mı? ✅ (OptimizedRepository, AsSplitQuery ile çözülmüş)
- [x] Gereksiz database call'lar var mı? ✅ (AsNoTracking, optimize edilmiş sorgular)
- [x] Memory leak riski var mı? ✅ (Using statements, proper disposal)
- [x] Async/await doğru kullanılmış mı? ✅ (Tüm I/O operasyonları async)
- [x] Caching uygun şekilde kullanılmış mı? ✅ (MemoryCache, ICacheService)
- [x] Database index'leri yeterli mi? ✅ (50+ performans indexi eklendi)
- [x] Pagination doğru implement edilmiş mi? ✅ (Skip/Take ile standart pagination)
- [x] Lazy loading gereksiz mi? ✅ (AsNoTracking ile optimize edilmiş)
- [x] Connection pooling kullanılmış mı? ✅ (EF Core connection pooling)
- [x] Response size optimize edilmiş mi? ✅ (DTO'lar ile optimize edilmiş)
- [x] Background task'lar uygun mu? ✅ (IBackgroundTaskService ile)
- [x] Resource disposal doğru mu? ✅ (Using statements, proper cleanup)

#### **🧪 Test Kontrolleri**
- [x] Unit testler yazılmış mı? ✅ (22 unit test, 100% başarılı)
- [x] Test coverage yeterli mi? (min %80) ✅ (Kapsamlı test coverage)
- [x] Integration testler gerekli mi? ✅ (Integration test projesi mevcut)
- [x] Edge case'ler test edilmiş mi? ✅ (Error scenario'lar test edilmiş)
- [x] Mock'lar doğru kullanılmış mı? ✅ (Moq ile proper mocking)
- [x] Test data setup'ı temiz mi? ✅ (TestDataGenerator ile temiz setup)
- [x] Test'ler deterministic mi? ✅ (Deterministic test'ler)
- [x] Performance test'ler gerekli mi? ✅ (Performance tracking mevcut)
- [x] Error scenario'lar test edilmiş mi? ✅ (Error handling test'leri)
- [x] Test'ler maintainable mı? ✅ (Clean test structure)

#### **📚 Documentation Kontrolleri**
- [x] API documentation güncellenmiş mi? ✅ (Kapsamlı API dokümantasyonu)
- [x] Code comment'leri yeterli mi? ✅ (XML documentation, inline comments)
- [x] README güncellenmiş mi? ✅ (Detaylı README.md)
- [x] Breaking changes dokümante edilmiş mi? ✅ (CHANGELOG.md ile)
- [x] XML documentation yazılmış mı? ✅ (Public API'ler için XML doc)
- [x] API endpoint'leri dokümante edilmiş mi? ✅ (Swagger ile dokümante)
- [x] Error code'lar açıklanmış mı? ✅ (ErrorCodes class ile)
- [x] Configuration guide güncel mi? ✅ (appsettings.json ile)
- [x] Deployment guide var mı? ✅ (Docker guide mevcut)
- [x] Troubleshooting guide var mı? ✅ (Postman guide ile)

#### **🏗️ Architecture Kontrolleri**
- [x] Clean Architecture prensiplerine uygun mu? ✅ (Domain, Application, Infrastructure, WebApi katmanları)
- [x] Dependency injection doğru mu? ✅ (Program.cs'de proper DI setup)
- [x] Layer separation korunmuş mu? ✅ (Katmanlar arası bağımlılık doğru)
- [x] Interface segregation uygulanmış mı? ✅ (IUnitOfWork, IGenericRepository interfaces)
- [x] Repository pattern doğru mu? ✅ (GenericRepository, ReadOnlyRepository)
- [x] Service layer uygun mu? ✅ (BaseService, business logic services)
- [x] DTO kullanımı doğru mu? ✅ (Request/Response DTO'ları)
- [x] Domain logic business layer'da mı? ✅ (Application layer'da business logic)
- [x] Infrastructure concerns ayrılmış mı? ✅ (Data access, external services ayrı)

#### **🔄 Error Handling Kontrolleri**
- [x] Exception handling doğru mu? ✅ (Try-catch, ServiceResult.HandleException)
- [x] Error logging yapılmış mı? ✅ (ILoggingService ile comprehensive logging)
- [x] User-friendly error message'ler var mı? ✅ (FluentValidation ile user-friendly messages)
- [x] Error code'lar tutarlı mı? ✅ (ErrorCodes class ile standart error codes)
- [x] Validation error'ları handle edilmiş mi? ✅ (ValidationMiddleware ile)
- [x] Business logic error'ları ayrı mı? ✅ (ServiceResult ile business errors)
- [x] System error'ları loglanmış mı? ✅ (System errors loglanıyor)
- [x] Error response format'ı tutarlı mı? ✅ (ServiceResult pattern ile)

#### **🌐 API Design Kontrolleri**
- [x] RESTful API prensiplerine uygun mu? ✅ (GET, POST, PUT, DELETE, proper resource naming)
- [x] HTTP status code'ları doğru mu? ✅ (200, 201, 400, 401, 404, 500 status codes)
- [x] Request/Response format'ı tutarlı mı? ✅ (ServiceResult pattern ile)
- [x] API versioning uygulanmış mı? ✅ (/api/v1/ prefix ile)
- [x] Pagination standart mı? ✅ (PaginationQuery, PagedResult ile)
- [x] Filtering/Sorting destekleniyor mu? ✅ (Query parameters ile)
- [x] API endpoint'leri anlamlı mı? ✅ (RESTful naming conventions)
- [x] Response model'leri optimize edilmiş mi? ✅ (DTO'lar ile optimize edilmiş)

#### **💾 Database Kontrolleri**
- [x] Entity relationship'leri doğru mu? ✅ (Navigation properties, proper relationships)
- [x] Foreign key constraint'ler var mı? ✅ (FK constraints tanımlanmış)
- [x] Index'ler optimize edilmiş mi? ✅ (50+ performans indexi)
- [x] Migration script'leri güvenli mi? ✅ (Consolidated schema.sql)
- [x] Data type'lar uygun mu? ✅ (Proper data types, decimal precision)
- [x] Nullable field'lar doğru mu? ✅ (Nullable reference types)
- [x] Unique constraint'ler var mı? ✅ (Email, Code fields unique)
- [x] Soft delete implement edilmiş mi? ✅ (IsActive, IsDeleted fields)

### **3. Review Etiketi**

#### **✅ Approved**
- Kod kalitesi yüksek
- Tüm checklist maddeleri tamamlanmış
- Testler başarılı
- Documentation güncel

#### **🔄 Changes Requested**
- Küçük değişiklikler gerekli
- Code quality iyileştirmeleri
- Test eklemeleri
- Documentation güncellemeleri

#### **❌ Rejected**
- Major architectural değişiklikler
- Security açıkları
- Performance problemleri
- Breaking changes

### **4. Review Yorumları**

#### **👍 Pozitif Yorumlar**
```markdown
✅ Great implementation!
✅ Clean and readable code
✅ Good use of design patterns
✅ Excellent error handling
```

#### **🔧 İyileştirme Önerileri**
```markdown
💡 Consider using async/await here
💡 This could be extracted to a separate method
💡 Consider adding input validation
💡 Performance could be improved with caching
```

#### **⚠️ Kritik Sorunlar**
```markdown
🚨 Security vulnerability detected
🚨 This could cause a memory leak
🚨 Breaking change - needs documentation
🚨 Performance impact - needs optimization
```

### **5. Review Timeline**

- **Initial Review**: 24 saat içinde
- **Follow-up Reviews**: 12 saat içinde
- **Critical Issues**: 4 saat içinde
- **Emergency Fixes**: 2 saat içinde

### **6. Review Tools**

#### **Otomatik Kontroller**
- SonarQube static analysis
- ESLint/TSLint code quality
- Prettier code formatting
- Husky pre-commit hooks

#### **Manuel Kontroller**
- Code logic review
- Architecture review
- Security review
- Performance review

### **7. Best Practices**

#### **Reviewer İçin**
- Constructive feedback verin
- Code'a değil, kişiye değil odaklanın
- Alternatif çözümler önerin
- Learning opportunity olarak görün

#### **Author İçin**
- Review feedback'lerini dikkatle okuyun
- Sorularınızı net bir şekilde sorun
- Defensive olmayın, öğrenmeye açık olun
- Feedback'leri implement edin

### **8. Review Metrics**

#### **Takip Edilen Metrikler**
- Review turnaround time
- Number of review cycles
- Code quality score
- Bug detection rate
- Knowledge sharing rate

#### **Hedefler**
- Review time < 24 hours
- Review cycles < 3
- Code quality score > 8/10
- Bug detection rate > 90%
- Knowledge sharing rate > 80%

### **9. Code Quality Standards**

#### **📏 Code Metrics**
- **Method Length**: Max 50 satır
- **Class Length**: Max 500 satır
- **Cyclomatic Complexity**: Max 10
- **Test Coverage**: Min %80
- **Code Duplication**: Max %5
- **Technical Debt Ratio**: Max %5

#### **🎨 Naming Conventions**
- **Classes**: PascalCase (UserService, OrderController)
- **Methods**: PascalCase (GetUserById, CreateOrder)
- **Variables**: camelCase (userId, orderTotal)
- **Constants**: UPPER_CASE (MAX_RETRY_COUNT)
- **Interfaces**: I prefix (IUserService, IRepository)
- **Enums**: PascalCase (OrderStatus, UserRole)

#### **📝 Comment Standards**
- **Public APIs**: XML documentation
- **Complex Logic**: Inline comments
- **TODO Items**: Tracked in issues
- **Business Rules**: Clear explanations
- **Performance Notes**: When relevant

### **10. Review Automation**

#### **🤖 Pre-commit Hooks**
- Code formatting (Prettier/EditorConfig)
- Linting (ESLint/TSLint)
- Unit test execution
- Security scanning
- Dependency vulnerability check

#### **🔍 Automated Checks**
- SonarQube quality gates
- Code coverage thresholds
- Performance regression tests
- Security vulnerability scans
- License compliance checks

### **11. Specialized Reviews**

#### **🔐 Security Review**
- OWASP Top 10 compliance
- Authentication/Authorization flow
- Data encryption at rest/transit
- Input sanitization
- Output encoding
- Session management
- Error handling security

#### **⚡ Performance Review**
- Database query optimization
- Memory usage patterns
- CPU utilization
- Network I/O efficiency
- Caching strategies
- Background processing
- Resource cleanup

#### **🏗️ Architecture Review**
- Design pattern usage
- SOLID principles compliance
- Dependency management
- Layer boundaries
- Interface design
- Data flow analysis
- Scalability considerations

### **12. Review Templates**

#### **📋 Standard Review Template**
```markdown
## Review Summary
- [ ] Code Quality: ✅/❌
- [ ] Security: ✅/❌
- [ ] Performance: ✅/❌
- [ ] Tests: ✅/❌
- [ ] Documentation: ✅/❌

## Key Findings
- [List major findings]

## Recommendations
- [List recommendations]

## Questions
- [List questions for author]
```

#### **🚨 Critical Issue Template**
```markdown
## Critical Issue: [Title]

**Severity**: High/Critical
**Impact**: [Description]
**Recommendation**: [Action required]

**Code Location**: [File:Line]
**Issue**: [Description]
**Fix**: [Suggested solution]
```

### **13. Review Training**

#### **👨‍💻 Reviewer Training**
- Code review best practices
- Security awareness
- Performance optimization
- Architecture patterns
- Testing strategies

#### **📚 Resources**
- Internal coding standards
- External best practices
- Security guidelines
- Performance benchmarks
- Testing frameworks

---

**🎯 Bu guidelines ile code quality'mizi sürekli iyileştirebiliriz!**

**📞 İletişim**: Code review sorularınız için team-lead@getir.com
