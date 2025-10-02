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
- [ ] Kod SOLID prensiplerine uygun mu?
- [ ] DRY (Don't Repeat Yourself) prensibi uygulanmış mı?
- [ ] Naming conventions doğru mu?
- [ ] Code formatting tutarlı mı?
- [ ] Gereksiz kod var mı?
- [ ] Magic numbers/strings kullanılmış mı?
- [ ] Method/class boyutları uygun mu? (max 50 satır method, 500 satır class)
- [ ] Cyclomatic complexity düşük mü? (max 10)
- [ ] Code duplication var mı?
- [ ] Dead code temizlenmiş mi?

#### **🛡️ Security Kontrolleri**
- [ ] SQL injection koruması var mı?
- [ ] XSS koruması var mı?
- [ ] Authentication/Authorization doğru mu?
- [ ] Sensitive data expose edilmiş mi?
- [ ] Input validation yapılmış mı?
- [ ] Password hashing güvenli mi?
- [ ] JWT token güvenliği sağlanmış mı?
- [ ] CORS policy doğru mu?
- [ ] Rate limiting uygulanmış mı?
- [ ] Logging'de sensitive data var mı?
- [ ] Error message'lerde sensitive bilgi var mı?
- [ ] HTTPS zorunlu mu?

#### **⚡ Performance Kontrolleri**
- [ ] N+1 query problemi var mı?
- [ ] Gereksiz database call'lar var mı?
- [ ] Memory leak riski var mı?
- [ ] Async/await doğru kullanılmış mı?
- [ ] Caching uygun şekilde kullanılmış mı?
- [ ] Database index'leri yeterli mi?
- [ ] Pagination doğru implement edilmiş mi?
- [ ] Lazy loading gereksiz mi?
- [ ] Connection pooling kullanılmış mı?
- [ ] Response size optimize edilmiş mi?
- [ ] Background task'lar uygun mu?
- [ ] Resource disposal doğru mu?

#### **🧪 Test Kontrolleri**
- [ ] Unit testler yazılmış mı?
- [ ] Test coverage yeterli mi? (min %80)
- [ ] Integration testler gerekli mi?
- [ ] Edge case'ler test edilmiş mi?
- [ ] Mock'lar doğru kullanılmış mı?
- [ ] Test data setup'ı temiz mi?
- [ ] Test'ler deterministic mi?
- [ ] Performance test'ler gerekli mi?
- [ ] Error scenario'lar test edilmiş mi?
- [ ] Test'ler maintainable mı?

#### **📚 Documentation Kontrolleri**
- [ ] API documentation güncellenmiş mi?
- [ ] Code comment'leri yeterli mi?
- [ ] README güncellenmiş mi?
- [ ] Breaking changes dokümante edilmiş mi?
- [ ] XML documentation yazılmış mı?
- [ ] API endpoint'leri dokümante edilmiş mi?
- [ ] Error code'lar açıklanmış mı?
- [ ] Configuration guide güncel mi?
- [ ] Deployment guide var mı?
- [ ] Troubleshooting guide var mı?

#### **🏗️ Architecture Kontrolleri**
- [ ] Clean Architecture prensiplerine uygun mu?
- [ ] Dependency injection doğru mu?
- [ ] Layer separation korunmuş mu?
- [ ] Interface segregation uygulanmış mı?
- [ ] Repository pattern doğru mu?
- [ ] Service layer uygun mu?
- [ ] DTO kullanımı doğru mu?
- [ ] Domain logic business layer'da mı?
- [ ] Infrastructure concerns ayrılmış mı?

#### **🔄 Error Handling Kontrolleri**
- [ ] Exception handling doğru mu?
- [ ] Error logging yapılmış mı?
- [ ] User-friendly error message'ler var mı?
- [ ] Error code'lar tutarlı mı?
- [ ] Validation error'ları handle edilmiş mi?
- [ ] Business logic error'ları ayrı mı?
- [ ] System error'ları loglanmış mı?
- [ ] Error response format'ı tutarlı mı?

#### **🌐 API Design Kontrolleri**
- [ ] RESTful API prensiplerine uygun mu?
- [ ] HTTP status code'ları doğru mu?
- [ ] Request/Response format'ı tutarlı mı?
- [ ] API versioning uygulanmış mı?
- [ ] Pagination standart mı?
- [ ] Filtering/Sorting destekleniyor mu?
- [ ] API endpoint'leri anlamlı mı?
- [ ] Response model'leri optimize edilmiş mi?

#### **💾 Database Kontrolleri**
- [ ] Entity relationship'leri doğru mu?
- [ ] Foreign key constraint'ler var mı?
- [ ] Index'ler optimize edilmiş mi?
- [ ] Migration script'leri güvenli mi?
- [ ] Data type'lar uygun mu?
- [ ] Nullable field'lar doğru mu?
- [ ] Unique constraint'ler var mı?
- [ ] Soft delete implement edilmiş mi?

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
